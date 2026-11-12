
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Boss defeated!");

            SceneManager.LoadScene("WinScene");
        }
    }
}
