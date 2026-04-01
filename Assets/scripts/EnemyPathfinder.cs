using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathfinder : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 3f;
    public float repathInterval = 0.5f;
    public float waypointReachThreshold = 0.2f;
    public float directChaseDistance = 5f;

    [Header("状态与索敌 (新功能)")]
    public float aggroRadius = 8f;       // 仇恨范围：玩家靠近到这个距离，怪物醒来
    private bool isAggro = false;        // 当前是否处于追击状态

    [Header("随机漫游配置 (新功能)")]
    public float wanderRadius = 3f;      // 围绕出生点散步的最大半径
    public float wanderInterval = 2f;    // 每隔几秒换一个地方溜达
    private float wanderTimer;
    private Vector2 wanderTarget;        // 当前溜达的目标点
    private Vector2 startPosition;       // 记录刚出生时的位置

    private Transform target;
    private Rigidbody2D rb;
    private List<Vector3> path;
    private int currentWaypointIndex;
    private float repathTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 记录出生点，以后就在这附近溜达
        startPosition = transform.position;
        // 马上选定第一个溜达目标
        PickNewWanderTarget();
    }

    void Update()
    {
        if (target == null)
        {
            FindPlayer();
        }

        // --- 核心逻辑：状态判断 ---
        if (target != null && !isAggro)
        {
            // 算一下和玩家的距离
            float distToPlayer = Vector2.Distance(transform.position, target.position);

            // 如果玩家进入了仇恨范围（或者你可以理解为玩家踏入了房间）
            if (distToPlayer <= aggroRadius)
            {
                // 发现玩家！醒来！
                isAggro = true;
                RecalculatePath(); // 立刻算一条路追上去
            }
        }

        // --- 状态执行：追击 vs 漫游 ---
        if (isAggro)
        {
            // 疯狂追击模式的倒计时
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                RecalculatePath();
                repathTimer = repathInterval;
            }
        }
        else
        {
            // 闲逛漫游模式的倒计时
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                PickNewWanderTarget();
                wanderTimer = wanderInterval;
            }
        }
    }

    void FixedUpdate()
    {
        if (isAggro)
        {
            // 如果发现玩家了，就不死不休地追
            if (target == null)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            MoveAlongPathOrDirect();
        }
        else
        {
            // 如果没发现玩家，就慢悠悠地溜达
            MoveTowardsWanderTarget();
        }
    }

    // ==========================================
    // 新增：漫游逻辑
    // ==========================================
    void PickNewWanderTarget()
    {
        // 在出生点周围画个圆，随机挑一个点
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + randomOffset;
    }

    void MoveTowardsWanderTarget()
    {
        Vector2 dir = (wanderTarget - (Vector2)transform.position);

        // 如果离目标点很近了，就停下来发呆，等倒计时结束换下一个点
        if (dir.magnitude < 0.2f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 溜达的时候不用跑那么快，一半速度就行（闲庭信步）
        rb.velocity = dir.normalized * (moveSpeed * 0.4f);
    }

    // ==========================================
    // 原有逻辑：找玩家 & 追击
    // ==========================================
    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            target = p.transform;
        }
    }

    void RecalculatePath()
    {
        if (target == null) return;

        float distToPlayer = Vector2.Distance(transform.position, target.position);
        if (distToPlayer <= directChaseDistance || DungeonNav.Instance == null)
        {
            path = null;
            currentWaypointIndex = 0;
            return;
        }

        List<Vector3> newPath = DungeonNav.Instance.FindPath(transform.position, target.position);

        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            currentWaypointIndex = 0;
        }
    }

    void MoveAlongPathOrDirect()
    {
        if (path == null || path.Count == 0 || currentWaypointIndex >= path.Count)
        {
            Vector2 dirDirect = (target.position - transform.position).normalized;
            rb.velocity = dirDirect * moveSpeed;
            return;
        }

        Vector3 targetPos = path[currentWaypointIndex];
        Vector2 dir = (targetPos - transform.position);
        float dist = dir.magnitude;

        if (dist < waypointReachThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= path.Count)
            {
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