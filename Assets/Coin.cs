
using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip coinSound;
    private bool collected;
    private float Volume => GameData.Current.items.coinSoundVolume * GameSession.Instance.Save.settings.sfx;

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

                if (coinSound != null)
                {
                    AudioSource.PlayClipAtPoint(
                        coinSound,
                        Camera.main.transform.position,
                        Volume
                    );
                }

                Destroy(gameObject);
            }
        }
    }
}
