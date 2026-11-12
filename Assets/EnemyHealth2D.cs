using UnityEngine;

public class EnemyHealth2D : MonoBehaviour
{
    [Min(1)] public int maxHealth = 2;
    [SerializeField] private int currentHealth;
    void Awake() { currentHealth = maxHealth; }
    void OnTriggerEnter2D(Collider2D other)
    {
        BulletDamage bullet = other.GetComponent<BulletDamage>();
        if (bullet == null || !other.name.StartsWith("Bullet")) return;
        other.enabled = false;
        currentHealth -= bullet.damage;
        Destroy(other.gameObject);
        if (currentHealth <= 0) Destroy(gameObject);
    }
}
