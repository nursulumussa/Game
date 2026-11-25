
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossHealth : MonoBehaviour
{
    public EnemyStats stats;
    public int maxHealth => stats.maxHealth;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        AdventureAudio.Play(SoundEvent.Hit);
        currentHealth -= damage;

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Boss defeated!");
            AdventureAudio.Play(SoundEvent.Death);

            GameSession.Instance.BossDefeated();
        }
    }
}
