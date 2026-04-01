using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPrefab
    {
        public string roomName;
        public GameObject prefab;
        public bool hasTop, hasBottom, hasLeft, hasRight;
    }

    [Header("游戏状态")]
    public int currentLevel = 0;

    [Header("房间配置")]
    public List<RoomPrefab> roomPool;

    [Header("地图配置")]
    public int maxRooms = 20;
    public int mainPathLength = 8;
    public float stepSize = 18f;

    [Header("分支配置")]
    [Range(0f, 1f)] public float branchChance = 0.45f;
    public int minBranchLength = 1;
    public int maxBranchLength = 3;

    [Header("连接数限制")]
    [Range(2, 4)] public int maxConnectionsPerRoom = 3;

    [Header("走廊预制体")]
    public GameObject corridorHorizontal;
    public GameObject corridorVertical;

    [Header("玩家配置")]
    public GameObject playerPrefab;
    private GameObject currentPlayer;

    // --- 寻路系统需要的公开接口 ---
    public Dictionary<Vector2Int, HashSet<Vector2Int>> Graph => graph;
    public float StepSize => stepSize;

    private Dictionary<Vector2Int, HashSet<Vector2Int>> graph = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, RoomPrefab> spawnedRooms = new Dictionary<Vector2Int, RoomPrefab>();
    private Vector2Int endRoomPos;

    void Start()
    {
        if (roomPool == null || roomPool.Count == 0) return;
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        currentLevel++;
        Debug.Log($"<color=cyan>--- 第 {currentLevel} 层 ---</color>");

        foreach (Transform child in transform) { Destroy(child.gameObject); }

        graph.Clear();
        spawnedRooms.Clear();

        BuildMainPath();
        BuildBranches(); // 确保这里被调用
        SpawnDungeonFromGraph();
        PlacePlayerAtStart();
    }

    void BuildMainPath()
    {
        List<Vector2Int> mainPath = GenerateLinearPath(mainPathLength, 50);
        if (mainPath.Count > 0)
        {
            endRoomPos = mainPath[mainPath.Count - 1];
            for (int i = 0; i < mainPath.Count; i++)
            {
                EnsureNode(mainPath[i]);
                if (i > 0) AddConnection(mainPath[i - 1], mainPath[i]);
            }
        }
    }

    // --- 支线生成算法 ---
    void BuildBranches()
    {
        for (int pass = 0; pass < 6; pass++)
        {
            if (graph.Count >= maxRooms) break;
            List<Vector2Int> candidates = new List<Vector2Int>(graph.Keys);
            ShuffleList(candidates);
            bool addedAny = false;
            foreach (Vector2Int origin in candidates)
            {
                if (graph.Count >= maxRooms) break;
                if (!CanBranchFrom(origin)) continue;
                if (Random.value > branchChance) continue;
                int added = GrowBranchFrom(origin, Random.Range(minBranchLength, maxBranchLength + 1));
                if (added > 0) addedAny = true;
            }
            if (!addedAny) break;
        }
    }

    bool CanBranchFrom(Vector2Int pos)
    {
        if (!graph.ContainsKey(pos) || graph[pos].Count >= maxConnectionsPerRoom) return false;
        return GetEmptyNeighbors(pos, new HashSet<Vector2Int>(graph.Keys)).Count > 0;
    }

    int GrowBranchFrom(Vector2Int origin, int targetLength)
    {
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>(graph.Keys);
        List<Vector2Int> localPath = new List<Vector2Int>();
        Vector2Int current = origin;
        for (int i = 0; i < targetLength; i++)
        {
            if (graph.Count + localPath.Count >= maxRooms) break;
            var avail = GetEmptyNeighbors(current, blocked);
            if (avail.Count == 0) break;
            Vector2Int next = avail[Random.Range(0, avail.Count)];
            localPath.Add(next); blocked.Add(next); current = next;
        }
        if (localPath.Count == 0) return 0;
        AddConnection(origin, localPath[0]);
        for (int i = 1; i < localPath.Count; i++) AddConnection(localPath[i - 1], localPath[i]);
        return localPath.Count;
    }

    void PlacePlayerAtStart()
    {
        Vector3 startPos = Vector3.zero;
        if (currentPlayer == null) currentPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
        else currentPlayer.transform.position = startPos;

        if (Camera.main != null)
        {
            var cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null) cam.target = currentPlayer.transform;
        }
    }

    // --- 基础工具方法 ---
    void EnsureNode(Vector2Int p) { if (!graph.ContainsKey(p)) graph.Add(p, new HashSet<Vector2Int>()); }
    void AddConnection(Vector2Int a, Vector2Int b) { EnsureNode(a); EnsureNode(b); graph[a].Add(b); graph[b].Add(a); }
    bool IsConnected(Vector2Int a, Vector2Int b) => graph.ContainsKey(a) && graph[a].Contains(b);
    List<Vector2Int> GetEmptyNeighbors(Vector2Int p, HashSet<Vector2Int> b)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs) if (!b.Contains(p + d)) res.Add(p + d);
        return res;
    }
    void ShuffleList<T>(List<T> list) { for (int i = 0; i < list.Count; i++) { int j = Random.Range(i, list.Count); T t = list[i]; list[i] = list[j]; list[j] = t; } }

    void SpawnDungeonFromGraph()
    {
        foreach (var kv in graph)
        {
            RoomPrefab room = GetExactRoomForPosition(kv.Key);
            if (room == null) room = GetBackupRoomForPosition(kv.Key);
            if (room != null) PlaceRoom(kv.Key, room);
        }
        foreach (var kv in graph) foreach (var b in kv.Value) if (kv.Key.x < b.x || (kv.Key.x == b.x && kv.Key.y < b.y)) SpawnCorridorBetween(kv.Key, b);
    }

    void PlaceRoom(Vector2Int pos, RoomPrefab room)
    {
        GameObject roomInstance = Instantiate(room.prefab, new Vector3(pos.x * stepSize, pos.y * stepSize, 0f), Quaternion.identity, transform);
        spawnedRooms.Add(pos, room);
        RoomManager rm = roomInstance.GetComponent<RoomManager>();
        if (rm != null) rm.InitializeRoom(pos == Vector2Int.zero, graph[pos].Count == 1 && pos != Vector2Int.zero && pos != endRoomPos, pos == endRoomPos);
    }

    RoomPrefab GetExactRoomForPosition(Vector2Int p)
    {
        bool t = IsConnected(p, p + Vector2Int.up), b = IsConnected(p, p + Vector2Int.down), l = IsConnected(p, p + Vector2Int.left), r = IsConnected(p, p + Vector2Int.right);
        var matches = roomPool.FindAll(rm => rm.hasTop == t && rm.hasBottom == b && rm.hasLeft == l && rm.hasRight == r);
        return matches.Count > 0 ? matches[Random.Range(0, matches.Count)] : null;
    }

    RoomPrefab GetBackupRoomForPosition(Vector2Int p)
    {
        bool t = IsConnected(p, p + Vector2Int.up), b = IsConnected(p, p + Vector2Int.down), l = IsConnected(p, p + Vector2Int.left), r = IsConnected(p, p + Vector2Int.right);
        var cands = roomPool.FindAll(rm => (!t || rm.hasTop) && (!b || rm.hasBottom) && (!l || rm.hasLeft) && (!r || rm.hasRight));
        if (cands.Count == 0) return null;
        return cands[Random.Range(0, cands.Count)];
    }

    void SpawnCorridorBetween(Vector2Int a, Vector2Int b)
    {
        Vector3 mid = new Vector3((a.x + b.x) * 0.5f * stepSize, (a.y + b.y) * 0.5f * stepSize, 0f);
        if (a.y == b.y && corridorHorizontal != null) Instantiate(corridorHorizontal, mid, Quaternion.identity, transform);
        else if (a.x == b.x && corridorVertical != null) Instantiate(corridorVertical, mid, Quaternion.identity, transform);
    }

    List<Vector2Int> GenerateLinearPath(int len, int retries)
    {
        List<Vector2Int> best = new List<Vector2Int> { Vector2Int.zero };
        for (int i = 0; i < retries; i++)
        {
            List<Vector2Int> p = new List<Vector2Int> { Vector2Int.zero };
            HashSet<Vector2Int> used = new HashSet<Vector2Int> { Vector2Int.zero };
            while (p.Count < len)
            {
                var avail = GetEmptyNeighbors(p[p.Count - 1], used);
                if (avail.Count == 0) break;
                var next = avail[Random.Range(0, avail.Count)];
                p.Add(next); used.Add(next);
            }
            if (p.Count > best.Count) best = new List<Vector2Int>(p);
            if (best.Count >= len) break;
        }
        return best;
    }
}