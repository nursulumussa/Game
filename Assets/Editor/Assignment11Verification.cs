using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class Assignment11Verification
{
    const string Prefix="MountainAdventure.A11.";
    static double next;static int errors;static bool busy;
    static Assignment11Verification()
    {
        if(SessionState.GetBool(Prefix+"Restore",false))EditorApplication.delayCall+=Restore;
        if(SessionState.GetBool(Prefix+"Running",false))Hook();
    }
    static void Hook()
    {
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;
        next=EditorApplication.timeSinceStartup+2;
    }
    static void Log(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)errors++;}
    static void Check(bool condition,string message)
    {
        if(!condition)throw new Exception(message);
        Debug.Log("A11_PASS "+message);
    }
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before verification.");
        if(!Application.isBatchMode && Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Save open scenes before running verification. Verification closes Unity when finished.");
        var c=GameData.Current;Check(c!=null && c.levels.Length==3,"Three LevelData assets reference the existing campaign scenes");
        Check(c.levels.Select(l=>l.sceneName).SequenceEqual(new[]{"Level1","Level2","Level3"}),"Original Level1/2/3 names and scene routes retained");
        Check(c.enemyTrial!=null && c.enemyTrial.sceneName=="EnemyTrial","Assignment 10 EnemyTrial remains accessible");
        Check(!File.Exists("Assets/Scenes/Gameplay.unity"),"No replacement Gameplay scene created");
        string folder=Path.Combine(Path.GetTempPath(),"MountainAdventure-A11-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        string path=Path.Combine(folder,"save.json");
        try
        {
            Check(SaveStore.Load(path,c,out _).unlockedLevels.SequenceEqual(new[]{1}),"Missing save restores defaults");
            foreach(string json in new[]{"{broken","{}","null","[]","{\"version\":99}",""})
            {
                File.WriteAllText(path,json);var data=SaveStore.Load(path,c,out _);
                Check(data.bestScore==0 && data.unlockedLevels.SequenceEqual(new[]{1}),"Invalid save restores defaults: "+json);
            }
            var d=SaveStore.Defaults(c);d.bestScore=1234;d.lastLevel=2;d.unlockedLevels.Add(2);d.completedLevels.Add(1);d.settings.music=0.25f;
            Check(SaveStore.Write(path,d,out _),"JSON writes successfully");var loaded=SaveStore.Load(path,c,out _);
            Check(loaded.bestScore==1234 && loaded.lastLevel==2 && loaded.unlockedLevels.Contains(2) && loaded.completedLevels.Contains(1) && loaded.settings.music==0.25f,"Progress, score and settings survive a disk round trip");
            loaded.unlockedLevels.Add(99);SaveStore.Write(path,loaded,out _);Check(SaveStore.Load(path,c,out _).unlockedLevels.Count==1,"Unknown level IDs rejected");
            d.settings.sfx=float.NaN;SaveStore.Write(path,d,out _);Check(SaveStore.Load(path,c,out _).bestScore==0,"Nonfinite volume rejected");
            Check(!SaveStore.Write(Path.Combine(path,"blocked.json"),d,out _),"Write failure handled without an exception");
            var legacy=SaveStore.Defaults(c);legacy.bestScore=42;
            string legacyJson=JsonUtility.ToJson(legacy).Replace(",\"campaignTrialPending\":false","").Replace(",\"campaignCompleted\":false","");
            File.WriteAllText(path,legacyJson);var migrated=SaveStore.Load(path,c,out _);
            Check(migrated.bestScore==42 && !migrated.campaignTrialPending && !migrated.campaignCompleted,"Existing version-1 saves remain compatible without finale fields");
            var finaleSave=SaveStore.Defaults(c);foreach(var level in c.levels){if(!finaleSave.unlockedLevels.Contains(level.id))finaleSave.unlockedLevels.Add(level.id);finaleSave.completedLevels.Add(level.id);}
            finaleSave.campaignTrialPending=true;SaveStore.Write(path,finaleSave,out _);
            Check(SaveStore.Load(path,c,out _).campaignTrialPending,"Pending finale is valid persistent progress");
            finaleSave.campaignCompleted=true;SaveStore.Write(path,finaleSave,out _);
            Check(SaveStore.Load(path,c,out _).completedLevels.Count==0,"Contradictory finale flags safely restore defaults");
            foreach(var level in c.levels.Concat(new[]{c.enemyTrial}))
            {
                Check(level.spawns.Length>0 && level.timeLimit>0,"LevelData has spawn bindings and timer: "+level.sceneName);
                Check(level.spawns.Select(s=>s.sceneObjectPath).Distinct().Count()==level.spawns.Length,"Spawn bindings are unique: "+level.sceneName);
            }
        }
        finally{Directory.Delete(folder,true);}
        string old=File.Exists(SaveStore.PathForSave)?Convert.ToBase64String(File.ReadAllBytes(SaveStore.PathForSave)):"MISSING";
        SessionState.SetString(Prefix+"OldSave",old);if(File.Exists(SaveStore.PathForSave))File.Delete(SaveStore.PathForSave);
        SessionState.SetBool(Prefix+"Running",true);SessionState.SetInt(Prefix+"Stage",0);SessionState.SetFloat(Prefix+"Started",(float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");Hook();EditorApplication.isPlaying=true;
    }
    static void Capture(string name)
    {
        string folder=Environment.GetEnvironmentVariable("MOUNTAIN_ADVENTURE_REVIEW");
        if(string.IsNullOrEmpty(folder))return;
        Directory.CreateDirectory(folder);
        var camera=Camera.main;if(camera==null)return;
        var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);target.Create();
        var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;float previousAspect=camera.aspect;float previousTimeScale=Time.timeScale;
        var overlays=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        var oldCameras=overlays.Select(c=>c.worldCamera).ToArray();var oldDistances=overlays.Select(c=>c.planeDistance).ToArray();
        Texture2D pixels=null;
        try
        {
            Time.timeScale=0;
            camera.targetTexture=target;camera.aspect=1280f/720f;
            foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            Canvas.ForceUpdateCanvases();
            var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};
            if(!RenderPipeline.SupportsRenderRequest(camera,request)){Debug.LogWarning("Scene render request unavailable: "+name);return;}
            RenderPipeline.SubmitRenderRequest(camera,request);RenderTexture.active=target;
            pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
            File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Debug.Log("A11_SCENE_RENDERED "+name);
        }
        finally
        {
            for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=oldCameras[i];overlays[i].planeDistance=oldDistances[i];}
            camera.targetTexture=previousTarget;camera.aspect=previousAspect;RenderTexture.active=previousActive;Canvas.ForceUpdateCanvases();Time.timeScale=previousTimeScale;
            if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);
        }
    }
    static void CheckLevel(GameSession s,int id)
    {
        Check(s.Level!=null && s.Level.id==id && SceneManager.GetActiveScene().name=="Level"+id,"Existing Level"+id+" runs directly with its LevelData");
        Check(UnityEngine.Object.FindFirstObjectByType<PlayerController>()!=null,"Player retained in Level"+id);
        Check(UnityEngine.Object.FindFirstObjectByType<PlayerInput>()!=null,"Existing Input System retained in Level"+id);
        Check(UnityEngine.Object.FindObjectsByType<OnScreenButton>(FindObjectsSortMode.None).Length>=2,"Mobile JUMP/FIRE retained in Level"+id);
        foreach(var row in s.Level.spawns)
        {
            var target=s.Level.FindObject(SceneManager.GetActiveScene(),row.sceneObjectPath);
            Check(target!=null,"Original spawn object resolves: "+s.Level.sceneName+"/"+row.sceneObjectPath);
        }
    }
    static void PortalFinish(GameSession s)
    {
        s.AddCoins(s.Level.requiredCoins);
        Check(s.Playing && s.GoalMet,"Data goal is ready for the original portal: "+s.Level.sceneName);
        var portal=UnityEngine.Object.FindFirstObjectByType<Portal>();
        Check(portal!=null,"Original portal retained in "+s.Level.sceneName);
        portal.SendMessage("OnTriggerEnter2D",UnityEngine.Object.FindFirstObjectByType<PlayerController>().GetComponent<Collider2D>());
    }
    static BindingFlags Hidden=>BindingFlags.Instance|BindingFlags.NonPublic;
    static void SetBase(EnemyAgent2D agent,string field,object value)=>typeof(EnemyAgent2D).GetField(field,Hidden).SetValue(agent,value);
    static void State(EnemyAgent2D agent,string name)
    {
        var field=agent.GetType().GetField("currentState",Hidden);field.SetValue(agent,Enum.Parse(field.FieldType,name));SetBase(agent,"stateTime",0f);
    }
    static string State(EnemyAgent2D agent)=>agent.GetType().GetField("currentState",Hidden).GetValue(agent).ToString();
    static void Step(EnemyAgent2D agent)=>agent.GetType().GetMethod("Update",Hidden).Invoke(agent,null);
    static void CombatChecks()
    {
        var player=UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();var playerBody=player.GetComponent<Collider2D>();
        Vector3 originalPlayer=player.transform.position;
        foreach(var agent in UnityEngine.Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None))
        {
            Vector3 originalAgent=agent.transform.position;
            try
            {
                agent.transform.position=new Vector3(0,2.5f,0);Physics2D.SyncTransforms();
                player.transform.position+=(Vector3)(agent.GetComponent<Collider2D>().bounds.center+Vector3.right-playerBody.bounds.center);Physics2D.SyncTransforms();
                SetBase(agent,"home",agent.transform.position);SetBase(agent,"facing",1);
                Check(agent.stats!=null && agent.windUp>=0.3f && agent.windUp<=0.8f && agent.attackInterval>0,"Asset-backed fair wind-up and recovery: "+agent.GetType().Name);
                var sight=typeof(EnemyAgent2D).GetMethod("HasLineOfSight",Hidden);
                Check((bool)sight.Invoke(agent,new object[]{false}),"Clear line of sight works: "+agent.GetType().Name);
                var wall=new GameObject("A11 test occluder");wall.layer=LayerMask.NameToLayer("Obstacle");wall.transform.position=(agent.eyePoint.position+playerBody.bounds.center)/2;
                wall.AddComponent<BoxCollider2D>().size=new Vector2(0.1f,3);Physics2D.SyncTransforms();
                Check(!(bool)sight.Invoke(agent,new object[]{false}),"Wall blocks sight: "+agent.GetType().Name);
                UnityEngine.Object.DestroyImmediate(wall);Physics2D.SyncTransforms();
                Vector3 visiblePlayer=player.transform.position;player.transform.position+=Vector3.right*1000;Physics2D.SyncTransforms();
                Check(!(bool)typeof(EnemyAgent2D).GetProperty("AttackAllowed",Hidden).GetValue(agent),"Off-screen attacks blocked: "+agent.GetType().Name);
                player.transform.position=visiblePlayer;Physics2D.SyncTransforms();
                if(agent is EnemyPatrol guard)
                {
                    State(guard,"Patrol");Step(guard);Check(State(guard)=="Chase","Patroller enters Chase");
                    Step(guard);Check(State(guard)=="WindUp","Patroller warns before attack");
                    SetBase(guard,"stateTime",guard.windUp+0.1f);Step(guard);Check(State(guard)=="Attack","Patroller waits for data wind-up");
                    Step(guard);Check(State(guard)=="Recover","Patroller recovers after one attack");
                    typeof(EnemyPatrol).GetField("recoveryUntil",Hidden).SetValue(guard,Time.time-1);Step(guard);Check(State(guard)=="Return","Patroller returns after cooldown");
                    player.transform.position+=Vector3.right*1000;Physics2D.SyncTransforms();
                    Step(guard);Check(State(guard)=="Patrol","Patroller resumes patrol after returning without seeing the player");
                }
                else if(agent is EnemyShooter2D shooter)
                {
                    State(shooter,"Scan");Step(shooter);Check(State(shooter)=="Aim","Shooter locks aim");
                    SetBase(shooter,"stateTime",shooter.windUp+0.1f);Step(shooter);Check(State(shooter)=="Shoot","Shooter waits for its wind-up");
                    Step(shooter);Check(State(shooter)=="Cooldown" && UnityEngine.Object.FindFirstObjectByType<EnemyProjectile2D>()!=null,"Shooter fires its original projectile and cools down");
                    SetBase(shooter,"stateTime",shooter.attackInterval+0.1f);Step(shooter);Check(State(shooter)=="Scan","Shooter completes its original FSM loop");
                }
                else if(agent is EnemyCharger2D charger)
                {
                    State(charger,"Patrol");Step(charger);Check(State(charger)=="WindUp","Charger commits after warning");
                    SetBase(charger,"stateTime",charger.windUp+0.1f);Step(charger);Check(State(charger)=="Dash","Charger enters Dash after wind-up");
                    SetBase(charger,"stateTime",charger.dashDuration+0.1f);Step(charger);Check(State(charger)=="Recover","Charger ends its dash using data duration");
                    SetBase(charger,"stateTime",charger.attackInterval+0.1f);Step(charger);Check(State(charger)=="Return","Charger returns after recovery");
                    Step(charger);Check(State(charger)=="Patrol","Charger completes its original FSM loop");
                }
            }
            finally{agent.transform.position=originalAgent;player.transform.position=originalPlayer;Physics2D.SyncTransforms();}
        }
    }
    static void Tick()
    {
        if(busy || !SessionState.GetBool(Prefix+"Running",false) || !EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+1;
        busy=true;
        try
        {
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Started",0)>240)throw new Exception("Play-mode verification timeout");
            var s=GameSession.Instance;if(s==null)return;int stage=SessionState.GetInt(Prefix+"Stage",0);
            switch(stage)
            {
                case 0:Check(SceneManager.GetActiveScene().name=="MainMenu","Original MainMenu starts normally");Capture("main-menu");s.StartLevel(1);break;
                case 1:
                    CheckLevel(s,1);Capture("level-1");
                    var player=UnityEngine.Object.FindFirstObjectByType<PlayerController>();var move=typeof(PlayerController).GetField("move",Hidden);float speed=s.Config.player.moveSpeed;
                    try{s.Config.player.moveSpeed=speed*2;move.SetValue(player,Vector2.right);player.SendMessage("FixedUpdate");Check(Mathf.Approximately(player.GetComponent<Rigidbody2D>().linearVelocity.x,speed*2),"Live data asset change updates actual player motion");}
                    finally{s.Config.player.moveSpeed=speed;move.SetValue(player,Vector2.zero);player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;}
                    player.GetComponent<Shooter>().Shoot();var bullet=UnityEngine.Object.FindFirstObjectByType<BulletDamage>();Check(bullet!=null && bullet.GetComponent<Rigidbody2D>()!=null,"Original shooting creates bullet physics safely");break;
                case 2:PortalFinish(s);break;
                case 3:CheckLevel(s,2);Check(s.Save.completedLevels.Contains(1) && s.Save.unlockedLevels.Contains(2),"Original Level1 portal unlocks and opens Level2");Capture("level-2");break;
                case 4:PortalFinish(s);break;
                case 5:CheckLevel(s,3);Check(s.Save.completedLevels.Contains(2) && s.Save.unlockedLevels.Contains(3),"Original Level2 portal unlocks and opens Level3");Capture("level-3");break;
                case 6:var boss=UnityEngine.Object.FindFirstObjectByType<BossHealth>();Check(boss!=null && boss.stats!=null,"Original boss uses its balance asset");boss.TakeDamage(boss.maxHealth);break;
                case 7:
                    Check(SceneManager.GetActiveScene().name=="EnemyTrial" && s.IsCampaignTrial && s.Save.completedLevels.Count==3,"Level3 boss automatically opens EnemyTrial as the campaign finale");
                    Check(s.Save.campaignTrialPending && !s.Save.campaignCompleted,"Winning Level3 does not finish the campaign before EnemyTrial");
                    s.Settings(0.6f,0.3f,0.4f);s.SendMessage("OnApplicationPause",true);var disk=SaveStore.Load(SaveStore.PathForSave,s.Config,out _);
                    Check(disk.completedLevels.Count==3 && disk.campaignTrialPending && disk.bestScore>0 && disk.settings.sfx==0.4f,"Pending finale, progress and settings survive disk reload and pause");
                    s.Menu();break;
                case 8:
                    Check(s.Save.campaignTrialPending && SceneManager.GetActiveScene().name=="MainMenu","Returning to the menu preserves the pending finale");s.StartTrial();break;
                case 9:
                    Check(SceneManager.GetActiveScene().name=="EnemyTrial" && !s.IsCampaignTrial,"Separate practice entry remains independent of campaign mode");
                    Check(UnityEngine.Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None).Length==3,"All three existing enemy types are retained");
                    Check(UnityEngine.Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None).All(e=>e.stateText!=null && e.stateText.rectTransform.anchoredPosition.y>0),"Original enemy state labels remain above the enemies");
                    Check(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name.EndsWith("_Grass")),"Existing decorated ground retained");
                    Check(UnityEngine.Object.FindObjectsByType<OnScreenButton>(FindObjectsSortMode.None).Length>=2,"Trial mobile controls retained");Capture("enemy-trial-practice");break;
                case 10:CombatChecks();PortalFinish(s);break;
                case 11:
                    Check(SceneManager.GetActiveScene().name=="WinScene" && s.Save.campaignTrialPending && !s.Save.campaignCompleted,"Finishing practice does not consume the pending campaign finale");s.Menu();break;
                case 12:
                    typeof(GameSession).GetProperty("Save").SetValue(s,SaveStore.Load(SaveStore.PathForSave,s.Config,out _));
                    s.UI.ShowLevels();Capture("level-select-sequential");Button("Continue finale / EnemyTrial").onClick.Invoke();break;
                case 13:
                    Check(s.IsCampaignTrial && s.Save.campaignTrialPending,"Continue finale restores campaign mode from saved progress");s.Retry();break;
                case 14:
                    Check(s.IsCampaignTrial && s.Save.campaignTrialPending,"Retry preserves finale mode and progress");Capture("enemy-trial");break;
                case 15:PortalFinish(s);break;
                case 16:
                    Check(SceneManager.GetActiveScene().name=="WinScene" && s.Save.campaignCompleted && !s.Save.campaignTrialPending,"Victory follows the full Level1-Level2-Level3-EnemyTrial sequence");
                    var finished=SaveStore.Load(SaveStore.PathForSave,s.Config,out _);Check(finished.campaignCompleted && !finished.campaignTrialPending,"Full campaign completion survives disk reload");
                    int score=s.Save.bestScore;s.UI.ShowSettings();Button("Reset progress").onClick.Invoke();Button("Cancel").onClick.Invoke();Check(s.Save.bestScore==score && s.Save.campaignCompleted,"Reset Cancel preserves campaign completion");
                    Button("Reset progress").onClick.Invoke();Button("Yes, reset").onClick.Invoke();break;
                case 17:
                    Check(s.Save.bestScore==0 && !s.Save.campaignCompleted && !s.Save.campaignTrialPending && s.Save.unlockedLevels.SequenceEqual(new[]{1}) && SceneManager.GetActiveScene().name=="MainMenu","Confirmed reset clears levels and finale flags");s.StartTrial();break;
                case 18:PortalFinish(s);break;
                case 19:
                    Check(SceneManager.GetActiveScene().name=="WinScene" && !s.Save.campaignCompleted && !s.Save.campaignTrialPending && s.Save.unlockedLevels.SequenceEqual(new[]{1}) && s.Save.completedLevels.Count==0,"Fresh practice does not unlock or finish the campaign");s.StartLevel(1);break;
                case 20:typeof(GameSession).GetProperty("Remaining").SetValue(s,0f);break;
                case 21:Check(!s.Playing && Button("Retry this level")!=null,"Data timer shows retry without crashing");Check(errors==0,"No runtime errors during integration checks");Finish(0);return;
            }
            SessionState.SetInt(Prefix+"Stage",stage+1);
            SessionState.SetFloat(Prefix+"Started",(float)EditorApplication.timeSinceStartup);
        }
        catch(Exception e){Debug.LogException(e);Finish(1);}
        finally{busy=false;}
    }
    static Button Button(string name)=>UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.gameObject.name==name && b.gameObject.activeInHierarchy);
    static void Finish(int code)
    {
        SessionState.SetBool(Prefix+"Running",false);EditorApplication.update-=Tick;SessionState.SetBool(Prefix+"Restore",true);SessionState.SetInt(Prefix+"Exit",code);
        EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode)Restore();};EditorApplication.isPlaying=false;
    }
    static void Restore()
    {
        if(EditorApplication.isPlaying || !SessionState.GetBool(Prefix+"Restore",false))return;
        SessionState.SetBool(Prefix+"Restore",false);string old=SessionState.GetString(Prefix+"OldSave","MISSING");
        if(old=="MISSING"){if(File.Exists(SaveStore.PathForSave))File.Delete(SaveStore.PathForSave);}else File.WriteAllBytes(SaveStore.PathForSave,Convert.FromBase64String(old));
        int code=SessionState.GetInt(Prefix+"Exit",1);Debug.Log(code==0?"A11_EXISTING_PROJECT_ALL_TESTS_PASSED":"A11_EXISTING_PROJECT_TESTS_FAILED");
        if(code==0 && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MOUNTAIN_ADVENTURE_APK")))
        {try{Assignment11Integration.BuildAndroid();}catch(Exception e){Debug.LogException(e);code=1;}}
        EditorApplication.Exit(code);
    }
}
