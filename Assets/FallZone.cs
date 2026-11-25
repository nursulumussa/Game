
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            AdventureAudio.Play(SoundEvent.Death);
            AdventureAudio.Play(SoundEvent.Lose);
            string sceneName =
                SceneManager.GetActiveScene().name;

            SceneManager.LoadScene(sceneName);
        }
    }
}
