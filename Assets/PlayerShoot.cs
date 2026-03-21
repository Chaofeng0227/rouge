using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float fireRate = 0.2f;
    public float firePointDistance = 0.45f;

    private float fireTimer;

    void Update()
    {
        fireTimer += Time.deltaTime;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 shootDirection = (mouseWorldPos - transform.position).normalized;
        UpdateFirePoint(shootDirection);

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot(shootDirection);
        }
    }

    void Shoot(Vector2 direction)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateFirePoint(Vector2 direction)
    {
        if (firePoint == null || direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        firePoint.localPosition = (Vector3)(direction.normalized * firePointDistance);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
