
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void PlayClick()
    {
        // AudioButton handles pointer and submit once; legacy UnityEvents remain compatible.
    }
}
