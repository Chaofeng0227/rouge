using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMoveToPlayer : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float stopDistance = 0.8f;   // Õ£÷πæ‡¿Î

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (Vector2)player.position - rb.position;
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            direction.Normalize();
            Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }
    }
}