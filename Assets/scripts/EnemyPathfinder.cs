using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathfinder : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 3f;
    public float repathInterval = 0.5f;          // 多久重新算一次路径
    public float waypointReachThreshold = 0.2f;  // 到达一个路径点的判定距离
    public float directChaseDistance = 5f;       // 距离玩家很近时，直接追逐，不再用路径

    private Transform target;          // 玩家
    private Rigidbody2D rb;
    private List<Vector3> path;        // 当前路径（世界坐标）
    private int currentWaypointIndex;
    private float repathTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 找玩家：确保玩家 Tag = "Player"
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            target = p.transform;
        }
        else
        {
            Debug.LogWarning("EnemyPathfinder: 没找到 Tag 为 'Player' 的对象。");
        }
    }

    void Update()
    {
        if (target == null)
            return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            RecalculatePath();
            repathTimer = repathInterval;
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        MoveAlongPathOrDirect();
    }

    // 重新计算路径
    void RecalculatePath()
    {
        // 如果离玩家很近，没必要跑 A*，直接追
        float distToPlayer = Vector2.Distance(transform.position, target.position);
        if (distToPlayer <= directChaseDistance || DungeonNav.Instance == null)
        {
            path = null;
            currentWaypointIndex = 0;
            return;
        }

        List<Vector3> newPath = DungeonNav.Instance.FindPath(transform.position, target.position);

        // 只有在找到合法路径时才替换当前路径，避免路径频繁在 null 和 有 路之间跳变
        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            currentWaypointIndex = 0;
        }
        // 找不到路就保持原来的 path（如果有），或者保持直线追逐
    }

    void MoveAlongPathOrDirect()
    {
        // 如果没有路径（距离近或没找到路），就直接追玩家
        if (path == null || path.Count == 0 || currentWaypointIndex >= path.Count)
        {
            Vector2 dirDirect = (target.position - transform.position).normalized;
            rb.velocity = dirDirect * moveSpeed;
            return;
        }

        // 按路径点移动
        Vector3 targetPos = path[currentWaypointIndex];
        Vector2 dir = (targetPos - transform.position);
        float dist = dir.magnitude;

        // 距离当前路径点很近了，切换到下一个
        if (dist < waypointReachThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= path.Count)
            {
                // 已经走完路径了，改为直接追玩家
                Vector2 finalDir = (target.position - transform.position).normalized;
                rb.velocity = finalDir * moveSpeed;
                return;
            }

            targetPos = path[currentWaypointIndex];
            dir = (targetPos - transform.position);
        }

        dir.Normalize();
        rb.velocity = dir * moveSpeed;
    }
}
