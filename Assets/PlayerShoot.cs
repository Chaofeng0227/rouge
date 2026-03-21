using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;   // 子弹预制体
    public Transform firePoint;       // 发射点
    public float bulletSpeed = 10f;   // 子弹速度
    public float fireRate = 0.2f;     // 射击间隔

    private float fireTimer;

    void Update()
    {
        fireTimer += Time.deltaTime;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 shootDirection = (mouseWorldPos - firePoint.position).normalized;

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot(shootDirection);
        }
    }

    void Shoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}