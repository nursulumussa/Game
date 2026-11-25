
using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip coinSound;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.GetComponent<PlayerController>() != null)
        {
            collected = true;
            CoinCounter counter =
                FindFirstObjectByType<CoinCounter>();

            if (counter != null)
            {
                counter.AddCoin();

                AdventureAudio.Play(SoundEvent.Pickup);

                Destroy(gameObject);
            }
        }
    }
}
