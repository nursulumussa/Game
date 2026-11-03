
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null && player.currentHealth < player.maxHealth)
        {
            player.Heal(1);
            Destroy(gameObject);
        }
    }
}
