
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;
    public float cooldown = 0.3f;

    public AudioSource audioSource;
    public AudioClip shootSound;

    private float nextShot;
    private int direction = 1;

    public void SetDirection(float x)
    {
        if (x > 0.01f) direction = 1;
        if (x < -0.01f) direction = -1;
    }

    public void Shoot()
    {
        if (Time.time < nextShot || bulletPrefab == null)
            return;

        nextShot = Time.time + cooldown;

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Vector3 spawnPosition = transform.position
            + new Vector3(direction * 0.7f, 0f, 0f);

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody2D bulletRb =
            bullet.AddComponent<Rigidbody2D>();

        bulletRb.gravityScale = 0f;
        bulletRb.linearVelocity =
            Vector2.right * direction * bulletSpeed;

        Destroy(bullet, 3f);
    }
}
