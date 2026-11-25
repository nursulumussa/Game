
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    private PlayerStats Stats => GameData.Current.player;


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

        nextShot = Time.time + Stats.shotCooldown;

        AdventureAudio.Play(SoundEvent.Shot);

        Vector3 spawnPosition = transform.position
            + new Vector3(direction * Stats.muzzleOffset, 0f, 0f);

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb == null) bulletRb = bullet.AddComponent<Rigidbody2D>();

        bulletRb.gravityScale = 0f;
        bulletRb.linearVelocity =
            Vector2.right * direction * Stats.bulletSpeed;

        Destroy(bullet, Stats.bulletLifetime);
    }
}
