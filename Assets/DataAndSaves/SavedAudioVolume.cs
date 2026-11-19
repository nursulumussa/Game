using UnityEngine;
public class SavedAudioVolume : MonoBehaviour
{
    AudioSource[] sources;float[] original;
    void Awake()
    {
        sources=GetComponents<AudioSource>();original=new float[sources.Length];
        for(int i=0;i<sources.Length;i++)original[i]=sources[i].volume;
    }
    public void Apply(SavedSettings settings)
    {
        for(int i=0;i<sources.Length;i++)if(sources[i]!=null)sources[i].volume=original[i]*(sources[i].loop?settings.music:settings.sfx);
    }
}
