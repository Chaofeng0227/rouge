using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject frostBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float fireRate = 0.2f;
    public float firePointDistance = 0.45f;
    public int bulletDamage = 1;
    public int splitShotSideCount;
    public float splitShotSpreadAngle = 12f;
    public int splitShotDamageBonus;
    public float splitShotSpeedBonus;
    public bool frostShotEnabled;
    public float frostSlowMultiplier = 0.6f;
    public float frostDuration = 1.5f;
    public int frostFreezeThreshold = 3;
    public float frostFreezeDuration = 1.25f;

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
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        SpawnBullet(spawnPosition, direction, bulletDamage, bulletSpeed);

        for (int i = 1; i <= splitShotSideCount; i++)
        {
            float angleOffset = splitShotSpreadAngle * i;
            int splitDamage = bulletDamage + splitShotDamageBonus;
            float splitSpeed = bulletSpeed + splitShotSpeedBonus;

            SpawnBullet(spawnPosition, Rotate(direction, angleOffset), splitDamage, splitSpeed);
            SpawnBullet(spawnPosition, Rotate(direction, -angleOffset), splitDamage, splitSpeed);
        }
    }

    void SpawnBullet(Vector3 spawnPosition, Vector2 direction, int damage, float speed)
    {
        GameObject prefabToSpawn = frostShotEnabled && frostBulletPrefab != null ? frostBulletPrefab : bulletPrefab;
        GameObject bullet = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * Mathf.Max(0.1f, speed);
        }

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.damage = Mathf.Max(1, damage);
            bulletComponent.appliesFrost = frostShotEnabled;
            bulletComponent.frostSlowMultiplier = frostSlowMultiplier;
            bulletComponent.frostDuration = frostDuration;
            bulletComponent.frostFreezeThreshold = frostFreezeThreshold;
            bulletComponent.frostFreezeDuration = frostFreezeDuration;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos).normalized;
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

