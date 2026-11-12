
using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip coinSound;
    public float volume = 0.7f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            CoinCounter counter =
                FindFirstObjectByType<CoinCounter>();

            if (counter != null)
            {
                counter.AddCoin();

                if (coinSound != null)
                {
                    AudioSource.PlayClipAtPoint(
                        coinSound,
                        Camera.main.transform.position,
                        volume
                    );
                }

                Destroy(gameObject);
            }
        }
    }
}
