
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public TMP_Text healthText;
    public float invincibleTime = 1f;

    private bool invincible = false;
    private SpriteRenderer sprite;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();

        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (invincible) return;

        currentHealth -= damage;
        UpdateHealthText();

        Debug.Log("HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }
        else
        {
            StartCoroutine(DamageEffect());
        }
    }

    IEnumerator DamageEffect()
    {
        invincible = true;

        for (int i = 0; i < 6; i++)
        {
            if (sprite != null)
            {
                sprite.enabled = !sprite.enabled;
            }

            yield return new WaitForSeconds(invincibleTime / 6f);
        }

        if (sprite != null)
        {
            sprite.enabled = true;
        }

        invincible = false;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );

        UpdateHealthText();
    }
}
