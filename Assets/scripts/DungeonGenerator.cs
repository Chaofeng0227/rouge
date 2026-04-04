using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPrefab
    {
        public string roomName;
        public GameObject prefab;
        public bool hasTop;
        public bool hasBottom;
        public bool hasLeft;
        public bool hasRight;
    }

    [Header("Game State")]
    public int currentLevel = 0;
    public int bossFloorNumber = 5;

    [Header("Room Setup")]
    public List<RoomPrefab> roomPool;

    [Header("Map Setup")]
    public int maxRooms = 20;
    public int mainPathLength = 8;
    public float stepSize = 18f;

    [Header("Branch Setup")]
    [Range(0f, 1f)] public float branchChance = 0.45f;
    public int minBranchLength = 1;
    public int maxBranchLength = 3;

    [Header("Connection Limit")]
    [Range(2, 4)] public int maxConnectionsPerRoom = 3;

    [Header("Corridors")]
    public GameObject corridorHorizontal;
    public GameObject corridorVertical;

    [Header("Player")]
    public GameObject playerPrefab;
    private GameObject currentPlayer;

    public Dictionary<Vector2Int, HashSet<Vector2Int>> Graph => graph;
    public float StepSize => stepSize;
    public Vector2Int EndRoomPos => endRoomPos;
    public Transform CurrentPlayerTransform => currentPlayer != null ? currentPlayer.transform : null;

    private Dictionary<Vector2Int, HashSet<Vector2Int>> graph = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, RoomPrefab> spawnedRooms = new Dictionary<Vector2Int, RoomPrefab>();
    private Vector2Int endRoomPos;
    private GameObject cachedBossRoomPrefab;

    void Start()
    {
        if (roomPool == null || roomPool.Count == 0)
        {
            return;
        }

        GenerateDungeon();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            JumpToBossFloorDebug();
        }
    }

    public void GenerateDungeon()
    {
        currentLevel++;
        Debug.Log($"<color=cyan>--- Floor {currentLevel} ---</color>");

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        graph.Clear();
        spawnedRooms.Clear();

        if (IsBossFloor())
        {
            BuildBossFloorLayout();
        }
        else
        {
            BuildMainPath();
            BuildBranches();
        }

        SpawnDungeonFromGraph();
        PlacePlayerAtStart();
    }

    public void JumpToBossFloorDebug()
    {
        currentLevel = bossFloorNumber - 1;
        Debug.Log($"<color=yellow>Debug jump to floor {bossFloorNumber}</color>");
        GenerateDungeon();
    }

    void BuildMainPath()
    {
        List<Vector2Int> mainPath = GenerateLinearPath(mainPathLength, 50);
        if (mainPath.Count <= 0)
        {
            return;
        }

        endRoomPos = mainPath[mainPath.Count - 1];
        for (int i = 0; i < mainPath.Count; i++)
        {
            EnsureNode(mainPath[i]);
            if (i > 0)
            {
                AddConnection(mainPath[i - 1], mainPath[i]);
            }
        }
    }

    void BuildBossFloorLayout()
    {
        Vector2Int start = Vector2Int.zero;
        Vector2Int staging = new Vector2Int(1, 0);
        Vector2Int northSupport = new Vector2Int(1, 1);
        Vector2Int southSupport = new Vector2Int(1, -1);
        Vector2Int bossApproach = new Vector2Int(2, 0);
        Vector2Int bossArena = new Vector2Int(3, 0);

        endRoomPos = bossArena;

        AddConnection(start, staging);
        AddConnection(staging, northSupport);
        AddConnection(staging, southSupport);
        AddConnection(staging, bossApproach);
        AddConnection(bossApproach, bossArena);
    }

    void BuildBranches()
    {
        for (int pass = 0; pass < 6; pass++)
        {
            if (graph.Count >= maxRooms)
            {
                break;
            }

            List<Vector2Int> candidates = new List<Vector2Int>(graph.Keys);
            ShuffleList(candidates);
            bool addedAny = false;

            foreach (Vector2Int origin in candidates)
            {
                if (graph.Count >= maxRooms)
                {
                    break;
                }

                if (!CanBranchFrom(origin))
                {
                    continue;
                }

                if (Random.value > branchChance)
                {
                    continue;
                }

                int added = GrowBranchFrom(origin, Random.Range(minBranchLength, maxBranchLength + 1));
                if (added > 0)
                {
                    addedAny = true;
                }
            }

            if (!addedAny)
            {
                break;
            }
        }
    }

    bool CanBranchFrom(Vector2Int pos)
    {
        if (!graph.ContainsKey(pos) || graph[pos].Count >= maxConnectionsPerRoom)
        {
            return false;
        }

        return GetEmptyNeighbors(pos, new HashSet<Vector2Int>(graph.Keys)).Count > 0;
    }

    int GrowBranchFrom(Vector2Int origin, int targetLength)
    {
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>(graph.Keys);
        List<Vector2Int> localPath = new List<Vector2Int>();
        Vector2Int current = origin;

        for (int i = 0; i < targetLength; i++)
        {
            if (graph.Count + localPath.Count >= maxRooms)
            {
                break;
            }

            List<Vector2Int> available = GetEmptyNeighbors(current, blocked);
            if (available.Count == 0)
            {
                break;
            }

            Vector2Int next = available[Random.Range(0, available.Count)];
            localPath.Add(next);
            blocked.Add(next);
            current = next;
        }

        if (localPath.Count == 0)
        {
            return 0;
        }

        AddConnection(origin, localPath[0]);
        for (int i = 1; i < localPath.Count; i++)
        {
            AddConnection(localPath[i - 1], localPath[i]);
        }

        return localPath.Count;
    }

    void PlacePlayerAtStart()
    {
        Vector3 startPos = Vector3.zero;
        if (currentPlayer == null)
        {
            currentPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
        }
        else
        {
            currentPlayer.transform.position = startPos;
        }

        if (Camera.main != null)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.target = currentPlayer.transform;
            }
        }
    }

    void EnsureNode(Vector2Int pos)
    {
        if (!graph.ContainsKey(pos))
        {
            graph.Add(pos, new HashSet<Vector2Int>());
        }
    }

    void AddConnection(Vector2Int a, Vector2Int b)
    {
        EnsureNode(a);
        EnsureNode(b);
        graph[a].Add(b);
        graph[b].Add(a);
    }

    bool IsConnected(Vector2Int a, Vector2Int b)
    {
        return graph.ContainsKey(a) && graph[a].Contains(b);
    }

    List<Vector2Int> GetEmptyNeighbors(Vector2Int pos, HashSet<Vector2Int> blocked)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in dirs)
        {
            if (!blocked.Contains(pos + dir))
            {
                result.Add(pos + dir);
            }
        }
        return result;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    void SpawnDungeonFromGraph()
    {
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> kv in graph)
        {
            RoomPrefab room = GetExactRoomForPosition(kv.Key);
            if (room == null)
            {
                room = GetBackupRoomForPosition(kv.Key);
            }

            if (room != null)
            {
                PlaceRoom(kv.Key, room);
            }
        }

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> kv in graph)
        {
            foreach (Vector2Int b in kv.Value)
            {
                if (kv.Key.x < b.x || (kv.Key.x == b.x && kv.Key.y < b.y))
                {
                    SpawnCorridorBetween(kv.Key, b);
                }
            }
        }
    }

    void PlaceRoom(Vector2Int pos, RoomPrefab room)
    {
        GameObject prefabToSpawn = room.prefab;
        if (IsBossFloor() && pos == endRoomPos)
        {
            GameObject bossRoomPrefab = GetBossRoomPrefab();
            if (bossRoomPrefab != null)
            {
                prefabToSpawn = bossRoomPrefab;
            }
        }

        GameObject roomInstance = Instantiate(prefabToSpawn, new Vector3(pos.x * stepSize, pos.y * stepSize, 0f), Quaternion.identity, transform);
        spawnedRooms.Add(pos, room);

        RoomManager rm = roomInstance.GetComponent<RoomManager>();
        if (rm != null)
        {
            rm.InitializeRoom(
                pos == Vector2Int.zero,
                graph[pos].Count == 1 && pos != Vector2Int.zero && pos != endRoomPos,
                pos == endRoomPos,
                IsBossFloor() && pos == endRoomPos);
        }
    }

    RoomPrefab GetExactRoomForPosition(Vector2Int pos)
    {
        bool top = IsConnected(pos, pos + Vector2Int.up);
        bool bottom = IsConnected(pos, pos + Vector2Int.down);
        bool left = IsConnected(pos, pos + Vector2Int.left);
        bool right = IsConnected(pos, pos + Vector2Int.right);

        List<RoomPrefab> matches = roomPool.FindAll(room =>
            room.hasTop == top &&
            room.hasBottom == bottom &&
            room.hasLeft == left &&
            room.hasRight == right);

        return matches.Count > 0 ? matches[Random.Range(0, matches.Count)] : null;
    }

    RoomPrefab GetBackupRoomForPosition(Vector2Int pos)
    {
        bool top = IsConnected(pos, pos + Vector2Int.up);
        bool bottom = IsConnected(pos, pos + Vector2Int.down);
        bool left = IsConnected(pos, pos + Vector2Int.left);
        bool right = IsConnected(pos, pos + Vector2Int.right);

        List<RoomPrefab> candidates = roomPool.FindAll(room =>
            (!top || room.hasTop) &&
            (!bottom || room.hasBottom) &&
            (!left || room.hasLeft) &&
            (!right || room.hasRight));

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    void SpawnCorridorBetween(Vector2Int a, Vector2Int b)
    {
        Vector3 mid = new Vector3((a.x + b.x) * 0.5f * stepSize, (a.y + b.y) * 0.5f * stepSize, 0f);
        if (a.y == b.y && corridorHorizontal != null)
        {
            Instantiate(corridorHorizontal, mid, Quaternion.identity, transform);
        }
        else if (a.x == b.x && corridorVertical != null)
        {
            Instantiate(corridorVertical, mid, Quaternion.identity, transform);
        }
    }

    List<Vector2Int> GenerateLinearPath(int len, int retries)
    {
        List<Vector2Int> best = new List<Vector2Int> { Vector2Int.zero };
        for (int i = 0; i < retries; i++)
        {
            List<Vector2Int> path = new List<Vector2Int> { Vector2Int.zero };
            HashSet<Vector2Int> used = new HashSet<Vector2Int> { Vector2Int.zero };

            while (path.Count < len)
            {
                List<Vector2Int> available = GetEmptyNeighbors(path[path.Count - 1], used);
                if (available.Count == 0)
                {
                    break;
                }

                Vector2Int next = available[Random.Range(0, available.Count)];
                path.Add(next);
                used.Add(next);
            }

            if (path.Count > best.Count)
            {
                best = new List<Vector2Int>(path);
            }

            if (best.Count >= len)
            {
                break;
            }
        }

        return best;
    }

    bool IsBossFloor()
    {
        return currentLevel >= bossFloorNumber;
    }

    GameObject GetBossRoomPrefab()
    {
        if (cachedBossRoomPrefab == null)
        {
            cachedBossRoomPrefab = Resources.Load<GameObject>("Rooms/BossRoom");
        }

        return cachedBossRoomPrefab;
    }
}
