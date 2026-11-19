
using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage => GameData.Current.player.bulletDamage;
    public LayerMask blockingLayers;

    void Awake()
    {
        if (blockingLayers.value == 0)
            blockingLayers = LayerMask.GetMask("Ground", "Obstacle");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BossHealth boss = other.GetComponent<BossHealth>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if ((blockingLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
