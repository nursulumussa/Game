using UnityEngine;
// Legacy serialized component: AudioMixer now owns saved volume for every source.
public class SavedAudioVolume : MonoBehaviour
{
    public void Apply(SavedSettings settings) {if(AdventureAudio.Instance!=null)AdventureAudio.Instance.Apply(settings);}
}
