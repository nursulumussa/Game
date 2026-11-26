using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class AdventureAudio : MonoBehaviour
{
    public static AdventureAudio Instance {get;private set;}
    public SoundBank Bank {get;private set;}
    public bool IsPaused {get;private set;}
    public float Fade {get;private set;}=1;
    public SoundEvent LastEvent {get;private set;}
    public int EventCount {get;private set;}
    public float LastPitch {get;private set;}
    public int CrossfadeOverlapFrames {get;private set;}
    public static event Action<SoundEvent> Emitted;
    AudioSource[] decks,voices;
    float[] from=new float[2],to=new float[2];
    int target=-1,nextVoice;
    bool focusPaused;
    const int VoiceCount=16;

    public static void Ensure()
    {
        if(Instance==null)new GameObject("Mountain Adventure Audio").AddComponent<AdventureAudio>();
    }
    void Awake()
    {
        if(Instance!=null){Destroy(gameObject);return;}
        Instance=this;DontDestroyOnLoad(gameObject);
        Bank=Resources.Load<SoundBank>("SoundPass/SoundBank");
        if(Bank==null){Debug.LogError("SoundBank missing: run Assignment 12 Prepare.");enabled=false;return;}
        Bank.mixer.updateMode=AudioMixerUpdateMode.UnscaledTime;
        decks=new[]{Source("Menu music",Bank.musicGroup,true),Source("Level music",Bank.musicGroup,true)};
        decks[0].clip=Bank.menuMusic;decks[1].clip=Bank.levelMusic;
        voices=new AudioSource[VoiceCount];for(int i=0;i<voices.Length;i++)voices[i]=Source("SFX "+i,Bank.sfxGroup,false);
        SceneManager.sceneLoaded+=Loaded;
    }
    AudioSource Source(string title,AudioMixerGroup group,bool loop)
    {
        var go=new GameObject(title);go.transform.SetParent(transform);
        var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;
        source.outputAudioMixerGroup=group;source.spatialBlend=0;source.ignoreListenerPause=true;
        source.volume=loop?0:Bank.effectGain;return source;
    }
    void Loaded(Scene scene,LoadSceneMode mode)
    {
        // Legacy sources remain serialized for compatibility but cannot play unlicensed old clips.
        foreach(var source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(source.transform.IsChildOf(transform))continue;
            source.Stop();source.playOnAwake=false;source.clip=null;
            source.outputAudioMixerGroup=Bank.sfxGroup;
        }
        foreach(var button in FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(button.GetComponent<AudioButton>()==null)button.gameObject.AddComponent<AudioButton>();
        var config=GameData.Current;
        SetMusic(Array.Exists(config.levels,l=>l.sceneName==scene.name) || config.enemyTrial.sceneName==scene.name);
        if(scene.name==GameData.Current.winScene)Play(SoundEvent.Win);
        Apply(GameSession.Instance.Save.settings);
    }
    public void SetMusic(bool gameplay)
    {
        int selected=gameplay?1:0;if(selected==target)return;
        target=selected;Fade=0;
        for(int i=0;i<2;i++){from[i]=decks[i].volume;to[i]=i==target?Bank.musicGain:0;}
        if(!decks[target].isPlaying)decks[target].Play();
    }
    void Update()
    {
        if(decks==null)return;
        Fade=Mathf.Min(1,Fade+Mathf.Min(Time.unscaledDeltaTime,0.05f)/Mathf.Max(0.01f,Bank.crossfadeSeconds));
        for(int i=0;i<2;i++)decks[i].volume=Mathf.Lerp(from[i],to[i],Mathf.SmoothStep(0,1,Fade));
        if(Fade<1 && decks[0].volume>.01f && decks[1].volume>.01f)CrossfadeOverlapFrames++;
        bool paused=focusPaused || Time.timeScale==0;
        if(paused!=IsPaused){IsPaused=paused;(paused?Bank.paused:Bank.normal).TransitionTo(Bank.snapshotSeconds);}
    }
    public static float Decibels(float linear)=>linear<=0.0001f?-80f:20f*Mathf.Log10(Mathf.Clamp01(linear));
    public void Apply(SavedSettings settings)
    {
        AudioListener.volume=1;
        Bank.mixer.SetFloat("MasterVolume",Decibels(settings.master));
        Bank.mixer.SetFloat("MusicVolume",Decibels(settings.music));
        Bank.mixer.SetFloat("SFXVolume",Decibels(settings.sfx));
    }
    public static void Play(SoundEvent sound)
    {
        var a=Instance;if(a==null || a.voices==null)return;
        var clip=a.Bank.events[(int)sound];if(clip==null)return;
        var source=a.voices[a.nextVoice++%VoiceCount];
        source.Stop();source.clip=clip;
        bool variation=sound!=SoundEvent.Win && sound!=SoundEvent.Lose;
        source.pitch=variation?UnityEngine.Random.Range(a.Bank.pitchRange.x,a.Bank.pitchRange.y):1;
        source.volume=a.Bank.effectGain;source.Play();
        a.LastEvent=sound;a.LastPitch=source.pitch;a.EventCount++;Emitted?.Invoke(sound);
    }
    void OnApplicationPause(bool paused){focusPaused=paused;}
    void OnDestroy(){SceneManager.sceneLoaded-=Loaded;if(Instance==this)Instance=null;}
}
