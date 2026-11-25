
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth => GameData.Current.player.maxHealth;
    public int currentHealth;
    public TMP_Text healthText;
    private PlayerStats Stats => GameData.Current.player;

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

        AdventureAudio.Play(SoundEvent.Hit);
        currentHealth -= damage;
        UpdateHealthText();

        Debug.Log("HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            AdventureAudio.Play(SoundEvent.Death);
            AdventureAudio.Play(SoundEvent.Lose);
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

        for (int i = 0; i < Stats.blinkCount; i++)
        {
            if (sprite != null)
            {
                sprite.enabled = !sprite.enabled;
            }

            yield return new WaitForSeconds(Stats.invincibleTime / Stats.blinkCount);
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
