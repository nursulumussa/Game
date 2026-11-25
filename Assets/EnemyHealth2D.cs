using UnityEngine;

public class EnemyHealth2D : MonoBehaviour
{
    public EnemyStats stats;
    public int maxHealth => stats.maxHealth;
    [SerializeField] private int currentHealth;
    void Awake() { currentHealth = maxHealth; }
    void OnTriggerEnter2D(Collider2D other)
    {
        BulletDamage bullet = other.GetComponent<BulletDamage>();
        if (bullet == null || !other.name.StartsWith("Bullet")) return;
        other.enabled = false;
        AdventureAudio.Play(SoundEvent.Hit);
        currentHealth -= bullet.damage;
        Destroy(other.gameObject);
        if (currentHealth <= 0) {AdventureAudio.Play(SoundEvent.Death);Destroy(gameObject);}
    }
}
