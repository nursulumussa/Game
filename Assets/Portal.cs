
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string nextScene = "Level2";
    public CoinCounter coinCounter;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            if (coinCounter != null && coinCounter.HasAllCoins())
            {
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}
