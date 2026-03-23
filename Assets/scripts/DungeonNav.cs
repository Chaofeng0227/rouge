using System.Collections.Generic;
using UnityEngine;

public class DungeonNav : MonoBehaviour
{
    public static DungeonNav Instance { get; private set; }

    private DungeonGenerator generator;
    private Dictionary<Vector2Int, HashSet<Vector2Int>> graph;
    private float stepSize;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        generator = GetComponent<DungeonGenerator>();
        if (generator == null)
        {
            Debug.LogError("DungeonNav 需要和 DungeonGenerator 挂在同一个物体上！");
            return;
        }

        graph = generator.Graph;
        stepSize = generator.StepSize;
    }

    // 世界坐标 -> 最近的房间网格坐标（简单用四舍五入）
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / stepSize);
        int y = Mathf.RoundToInt(worldPos.y / stepSize);
        return new Vector2Int(x, y);
    }

    // 网格坐标 -> 世界坐标（房间中心）
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * stepSize, gridPos.y * stepSize, 0f);
    }

    // 对外：从起点世界坐标，到终点世界坐标，求一条路径（世界坐标路径点）
    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        if (graph == null || graph.Count == 0)
            return null;

        Vector2Int startNode = WorldToGrid(startWorld);
        Vector2Int targetNode = WorldToGrid(targetWorld);

        if (!graph.ContainsKey(startNode) || !graph.ContainsKey(targetNode))
        {
            // 找不到合法节点，直接失败
            return null;
        }

        List<Vector2Int> gridPath = AStar(startNode, targetNode);

        if (gridPath == null || gridPath.Count == 0)
            return null;

        // 转成世界坐标路径
        List<Vector3> worldPath = new List<Vector3>();
        foreach (var node in gridPath)
        {
            worldPath.Add(GridToWorld(node));
        }
        return worldPath;
    }

    // ===== 简单 A* 实现（在 graph 上跑） =====
    List<Vector2Int> AStar(Vector2Int start, Vector2Int goal)
    {
        var openSet = new HashSet<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        foreach (var node in graph.Keys)
        {
            gScore[node] = float.PositiveInfinity;
            fScore[node] = float.PositiveInfinity;
        }
        gScore[start] = 0f;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            // 找 fScore 最小的节点
            Vector2Int current = default;
            float bestF = float.PositiveInfinity;
            foreach (var node in openSet)
            {
                float f = fScore[node];
                if (f < bestF)
                {
                    bestF = f;
                    current = node;
                }
            }

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in graph[current])
            {
                float tentativeG = gScore[current] + Vector2Int.Distance(current, neighbor);

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // 找不到路径
        return null;
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        // 曼哈顿或欧式都行
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current)
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
