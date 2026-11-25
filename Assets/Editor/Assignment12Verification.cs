using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class Assignment12Verification
{
    const string Key="MountainAdventure.A12.";
    static double next;
    static int errors;
    static bool busy,midFade;
    static AdventureAudio original;
    static AudioSource levelDeck;
    static int beforeSamples;
    static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance;
    static Assignment12Verification()
    {
        if(SessionState.GetBool(Key+"Running",false))Hook();
        if(SessionState.GetBool(Key+"Restore",false))EditorApplication.delayCall+=Restore;
    }
    static void Hook()
    {
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
        next=EditorApplication.timeSinceStartup+3;
    }
    static void Log(string text,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)errors++;}
    static void Check(bool condition,string text){if(!condition)throw new Exception(text);Debug.Log("A12_PASS "+text);}
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before verifying.");
        var bank=Resources.Load<SoundBank>("SoundPass/SoundBank");
        Check(bank!=null && bank.events.Length==12 && bank.events.All(c=>c!=null),"12 original sound events and valid bank");
        Check(bank.menuMusic!=bank.levelMusic && bank.menuMusic.length>30 && bank.levelMusic.length>30,"Two different original music tracks");
        Check(bank.mixer.FindMatchingGroups("").Any(g=>g.name=="Master") && bank.mixer.FindMatchingGroups("").Any(g=>g.name=="Music") && bank.mixer.FindMatchingGroups("").Any(g=>g.name=="SFX"),"Mixer has Master Music and SFX groups");
        Check(bank.normal.name=="Normal" && bank.paused.name=="Paused","Normal and Paused snapshots exist");
        Check(AdventureAudio.Decibels(0)==-80 && Mathf.Abs(AdventureAudio.Decibels(.5f)+6.0206f)<.001f && AdventureAudio.Decibels(1)==0,"Logarithmic volume and safe zero handling");
        SessionState.SetString(Key+"OldSave",File.Exists(SaveStore.PathForSave)?Convert.ToBase64String(File.ReadAllBytes(SaveStore.PathForSave)):"MISSING");
        SessionState.SetBool(Key+"Running",true);SessionState.SetInt(Key+"Stage",0);SessionState.SetFloat(Key+"Started",(float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");Hook();EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key+"Running",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling || busy)return;
        var audio=AdventureAudio.Instance;
        if(audio!=null && audio.Fade>0 && audio.Fade<1)
        {
            var music=audio.GetComponentsInChildren<AudioSource>().Where(a=>a.loop).ToArray();
            if(music.Length==2 && music.All(a=>a.volume>.01f))midFade=true;
        }
        if(EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+2;
        if(EditorApplication.timeSinceStartup-SessionState.GetFloat(Key+"Started",0)>240){Finish(false);return;}
        busy=true;
        try
        {
            var session=GameSession.Instance;Check(session!=null && audio!=null,"Persistent audio and game session available");
            int stage=SessionState.GetInt(Key+"Stage",0);
            // Batch mode can render much slower than a phone; wait for the actual transition.
            if((stage==0 || stage==1 || stage==2 || stage==6) && audio.Fade<0.999f)return;
            switch(stage)
            {
                case 0:
                    original=audio;
                    Check(audio.GetComponentsInChildren<AudioSource>().Length==18,"Two music decks and bounded pool of 16 SFX voices");
                    Check(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(a=>a.outputAudioMixerGroup!=null),"Every loaded AudioSource routed to the mixer");
                    Check(audio.GetComponentsInChildren<AudioSource>().Single(a=>a.clip==audio.Bank.menuMusic).isPlaying,"Menu track plays");
                    session.Settings(.65f,.35f,.45f);session.StartLevel(1);break;
                case 1:
                    Check(audio==original,"Audio manager persists into Level1");
                    levelDeck=audio.GetComponentsInChildren<AudioSource>().Single(a=>a.clip==audio.Bank.levelMusic);
                    Check(levelDeck.isPlaying && levelDeck.volume>.1f,"Level music fades in (playing="+levelDeck.isPlaying+", gain="+levelDeck.volume+", fade="+audio.Fade+")");
                    Check(midFade || audio.CrossfadeOverlapFrames>0,"Menu and level music overlap during the crossfade");
                    audio.Bank.mixer.GetFloat("MusicVolume",out float musicDB);
                    Check(Mathf.Abs(musicDB-AdventureAudio.Decibels(.35f))<.01f,"Saved Music slider reaches exposed mixer dB parameter");
                    var saved=SaveStore.Load(SaveStore.PathForSave,session.Config,out _);
                    Check(Mathf.Abs(saved.settings.music-.35f)<.001f && Mathf.Abs(saved.settings.sfx-.45f)<.001f,"Music and SFX settings survive disk reload");
                    var player=UnityEngine.Object.FindFirstObjectByType<PlayerController>();
                    typeof(PlayerController).GetField("grounded",flags).SetValue(player,true);
                    int count=audio.EventCount;player.OnJump();Check(audio.EventCount==count+1 && audio.LastEvent==SoundEvent.Jump,"Actual player jump emits its sound once");
                    count=audio.EventCount;player.GetComponent<Shooter>().Shoot();Check(audio.EventCount==count+1 && audio.LastEvent==SoundEvent.Shot,"Actual player shooting emits its sound once");
                    beforeSamples=levelDeck.timeSamples;SceneManager.LoadScene("Level2");break;
                case 2:
                    Check(audio==original && levelDeck.isPlaying && levelDeck.timeSamples>beforeSamples,"Level1 to Level2 retains the playing track position");
                    session.UI.ShowSettings();break;
                case 3:
                    Check(Time.timeScale==0 && audio.IsPaused,"Settings pause activates the Paused snapshot using unscaled time");
                    Check(levelDeck.isPlaying,"Music keeps playing softly on pause");
                    int count3=audio.EventCount;
                    var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.gameObject.name=="Back");
                    button.GetComponent<AudioButton>().OnPointerClick(new PointerEventData(EventSystem.current));
                    Check(audio.EventCount==count3+1 && audio.LastEvent==SoundEvent.Click,"UI click is audible while gameplay is paused");
                    session.UI.ShowGame();break;
                case 4:
                    Check(!audio.IsPaused,"Resume restores the Normal snapshot");
                    session.StartTrial();break;
                case 5:
                    foreach(var agent in UnityEngine.Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None))
                    {
                        var expected=agent is EnemyShooter2D?SoundEvent.ShooterWindup:agent is EnemyCharger2D?SoundEvent.ChargerWindup:SoundEvent.GuardWindup;
                        int count5=audio.EventCount;
                        var method=agent.GetType().GetMethod("Set",flags);var stateType=method.GetParameters()[0].ParameterType;
                        method.Invoke(agent,new[]{Enum.Parse(stateType,agent is EnemyShooter2D?"Aim":"WindUp")});
                        Check(audio.EventCount==count5+1 && audio.LastEvent==expected,"Distinct warning starts at FSM wind-up entry: "+agent.GetType().Name);
                    }
                    float first=-1;bool varied=false;
                    for(int i=0;i<20;i++){AdventureAudio.Play(SoundEvent.Pickup);Check(audio.LastPitch>=.92f && audio.LastPitch<=1.08f,"Pitch within 0.92 to 1.08");if(first<0)first=audio.LastPitch;else varied|=audio.LastPitch!=first;}
                    Check(varied,"Repeated pickups vary their pitch");
                    foreach(SoundEvent sound in Enum.GetValues(typeof(SoundEvent))){AdventureAudio.Play(sound);Check(audio.LastEvent==sound,"Playable event: "+sound);}
                    Check(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(a=>a.outputAudioMixerGroup!=null),"All trial and persistent sources routed");
                    session.Menu();break;
                case 6:
                    Check(audio==original && audio.GetComponentsInChildren<AudioSource>().Single(a=>a.clip==audio.Bank.menuMusic).isPlaying,"Return to menu crossfades to persistent menu deck");
                    session.StartTrial();break;
                case 7:
                    var zone=UnityEngine.Object.FindFirstObjectByType<FallZone>();
                    Check(zone!=null,"Original fall zone remains present");
                    int beforeFall=audio.EventCount;
                    typeof(FallZone).GetMethod("OnTriggerEnter2D",flags).Invoke(zone,new object[]{UnityEngine.Object.FindFirstObjectByType<PlayerController>().GetComponent<Collider2D>()});
                    Check(audio.EventCount==beforeFall+2 && audio.LastEvent==SoundEvent.Lose,"Actual fall trigger emits death and defeat before restart");break;
                case 8:
                    Check(audio==original && SceneManager.GetActiveScene().name=="EnemyTrial","Fall restart preserves the persistent sound manager");
                    var health=UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();int beforeDeath=audio.EventCount;
                    typeof(PlayerHealth).GetField("invincible",flags).SetValue(health,false);
                    health.TakeDamage(health.currentHealth);
                    Check(audio.EventCount==beforeDeath+3 && audio.LastEvent==SoundEvent.Lose,"Lethal damage emits hit death and defeat before restart");break;
                case 9:
                    Check(audio==original && SceneManager.GetActiveScene().name=="EnemyTrial","Damage restart preserves the persistent sound manager");
                    Check(errors==0,"No runtime audio errors");Finish(true);return;
            }
            SessionState.SetInt(Key+"Stage",stage+1);SessionState.SetFloat(Key+"Started",(float)EditorApplication.timeSinceStartup);
        }
        catch(Exception e){Debug.LogException(e);Finish(false);}
        finally{busy=false;}
    }
    static void Finish(bool success)
    {
        SessionState.SetBool(Key+"Running",false);SessionState.SetBool(Key+"Restore",true);SessionState.SetBool(Key+"Success",success);
        EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
    }
    static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key+"Restore",false))EditorApplication.delayCall+=Restore;}
    static void Restore()
    {
        if(EditorApplication.isPlaying)return;
        SessionState.SetBool(Key+"Restore",false);
        var old=SessionState.GetString(Key+"OldSave","MISSING");if(old=="MISSING"){if(File.Exists(SaveStore.PathForSave))File.Delete(SaveStore.PathForSave);}else File.WriteAllBytes(SaveStore.PathForSave,Convert.FromBase64String(old));
        bool success=SessionState.GetBool(Key+"Success",false);Debug.Log(success?"A12_SOUND_CHECKS_PASSED":"A12_SOUND_CHECKS_FAILED");
        if(success)
        {
            PlayerSettings.bundleVersion="1.1";PlayerSettings.Android.bundleVersionCode=2;
            Assignment11Verification.Run(); // Recheck the original campaign, saves and all three FSMs, then build.
        }
        else EditorApplication.Exit(1);
    }
}
