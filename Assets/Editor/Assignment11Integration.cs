using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Assignment11Integration
{
    [Serializable] class Number {public string name;public float value;}
    [Serializable] class Captured {public string scene,id,typeName;public Number[] values;}
    [Serializable] class CaptureFile {public Captured[] entries;}
    static CaptureFile original;
    const string Folder="Assets/Resources/Assignment11";
    static T Asset<T>(string name) where T:ScriptableObject
    {
        string path=Folder+"/"+name+".asset";var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
    }
    static Captured First(string scene,string type)=>original.entries.FirstOrDefault(e=>e.scene==scene && e.typeName==type);
    static Captured Capture(Component component)
    {
        string id=GlobalObjectId.GetGlobalObjectIdSlow(component).targetObjectId.ToString();
        return original.entries.FirstOrDefault(e=>e.scene==component.gameObject.scene.name && e.id==id && e.typeName==component.GetType().Name);
    }
    static float NumberValue(Captured source,string name,float fallback)
    {
        var value=source?.values.FirstOrDefault(v=>v.name==name);return value==null?fallback:value.value;
    }
    static void CopyEnemy(Captured captured,EnemyStats stats)
    {
        if(captured==null)throw new InvalidOperationException("An original enemy balance record is missing; refusing to guess its tuning.");
        foreach(var field in typeof(EnemyStats).GetFields(BindingFlags.Instance|BindingFlags.Public))
        {
            var number=captured.values.FirstOrDefault(v=>v.name==field.Name);
            if(number==null)continue;
            if(field.FieldType==typeof(float))field.SetValue(stats,number.value);
            else if(field.FieldType==typeof(int))field.SetValue(stats,(int)number.value);
        }
        EditorUtility.SetDirty(stats);
    }
    static string ObjectPath(Transform target)
    {
        string path=target.name;while(target.parent!=null){target=target.parent;path=target.name+"/"+path;}return path;
    }
    static Component[] Components(Scene scene)=>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Component>(true)).Where(c=>c!=null).ToArray();
    [MenuItem("Mountain Adventure/Assignment 11/Prepare data")]
    public static void Prepare()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
        if(GameData.Current!=null)throw new InvalidOperationException("Assignment 11 is already initialized. Existing data assets will not be overwritten.");
        EditorSceneManager.SaveOpenScenes();
        original=JsonUtility.FromJson<CaptureFile>(File.ReadAllText("Assets/Editor/Assignment11OriginalBalance.json"));
        if(original==null || original.entries.Length==0)throw new InvalidOperationException("Original tuning capture is missing.");
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
        var player=Asset<PlayerStats>("PlayerStats");
        var controller=First("EnemyTrial","PlayerController");var health=First("EnemyTrial","PlayerHealth");var weapon=First("EnemyTrial","Shooter");
        player.moveSpeed=NumberValue(controller,"speed",7);player.jumpForce=NumberValue(controller,"jumpForce",12);
        player.maxHealth=(int)NumberValue(health,"maxHealth",3);player.invincibleTime=NumberValue(health,"invincibleTime",1);player.blinkCount=6;
        player.bulletSpeed=NumberValue(weapon,"bulletSpeed",12);player.shotCooldown=NumberValue(weapon,"cooldown",0.3f);
        player.muzzleOffset=0.7f;player.bulletLifetime=3;player.bulletDamage=(int)NumberValue(First("EnemyTrial","BulletDamage"),"damage",1);EditorUtility.SetDirty(player);
        var items=Asset<ItemValues>("ItemValues");items.coinValue=1;items.healAmount=1;items.scorePerCoin=100;items.coinSoundVolume=NumberValue(First("EnemyTrial","Coin"),"volume",0.7f);EditorUtility.SetDirty(items);
        var fallback=Asset<EnemyStats>("DefaultEnemyStats");CopyEnemy(First("EnemyTrial","EnemyPatrol"),fallback);
        var levels=new List<LevelData>();LevelData trial=null;
        foreach(string sceneName in new[]{"Level1","Level2","Level3","EnemyTrial"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity",OpenSceneMode.Single);
            var all=Components(scene);
            foreach(var agent in all.OfType<EnemyAgent2D>())
            {
                var captured=Capture(agent);
                var stats=Asset<EnemyStats>(sceneName+"_"+agent.GetType().Name+"_"+GlobalObjectId.GetGlobalObjectIdSlow(agent).targetObjectId);
                CopyEnemy(captured,stats);agent.stats=stats;
                var enemyHealth=agent.GetComponent<EnemyHealth2D>();
                if(enemyHealth!=null){enemyHealth.stats=stats;stats.maxHealth=(int)NumberValue(Capture(enemyHealth),"maxHealth",2);EditorUtility.SetDirty(enemyHealth);}
                EditorUtility.SetDirty(agent);EditorUtility.SetDirty(stats);
            }
            foreach(var boss in all.OfType<BossHealth>())
            {
                var stats=Asset<EnemyStats>(sceneName+"_BossStats");stats.maxHealth=(int)NumberValue(Capture(boss),"maxHealth",5);boss.stats=stats;EditorUtility.SetDirty(stats);EditorUtility.SetDirty(boss);
            }
            bool isTrial=sceneName=="EnemyTrial";int id=isTrial?0:int.Parse(sceneName.Substring(5));
            var level=Asset<LevelData>(isTrial?"EnemyTrial_Data":"Level_0"+id);
            level.id=id;level.displayName=isTrial?"ENEMY TRIAL":("LEVEL "+id);level.sceneName=sceneName;
            level.nextLevelId=isTrial || id==3?0:id+1;
            level.goal=all.OfType<BossHealth>().Any()?LevelGoal.Boss:LevelGoal.CoinsAndPortal;
            level.requiredCoins=level.goal==LevelGoal.Boss?0:3;
            level.timeLimit=isTrial?180:new[]{90f,120f,150f}[id-1];level.completionScore=isTrial?0:id*250;level.scorePerSecond=5;
            var objects=all.Where(c=>c is EnemyAgent2D || c is BossHealth || c is PlayerController || c is Coin || c is HealthPickup).Select(c=>c.gameObject).Distinct().ToArray();
            level.spawns=objects.Select(go=>new LevelSpawn{sceneObjectPath=ObjectPath(go.transform),position=go.transform.position,rotation=go.transform.rotation,active=go.activeSelf}).ToArray();
            if(level.spawns.Select(s=>s.sceneObjectPath).Distinct().Count()!=level.spawns.Length)throw new Exception("Duplicate object paths in "+sceneName);
            EditorUtility.SetDirty(level);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            if(isTrial)trial=level;else levels.Add(level);
        }
        var config=ScriptableObject.CreateInstance<GameData>();config.player=player;config.items=items;config.defaultEnemy=fallback;config.enemyProjectileRadius=0.1f;
        config.levels=levels.OrderBy(l=>l.id).ToArray();config.enemyTrial=trial;config.trialAfterCampaign=true;config.defaultMaster=1;config.defaultMusic=0.8f;config.defaultSfx=0.8f;config.menuScene="MainMenu";config.winScene="WinScene";
        AssetDatabase.CreateAsset(config,"Assets/Resources/GameData.asset");
        var buildScenes=EditorBuildSettings.scenes.ToList();
        foreach(string name in new[]{"MainMenu","Level1","Level2","Level3","EnemyTrial","WinScene"})
        {
            string path="Assets/Scenes/"+name+".unity";
            var present=buildScenes.FirstOrDefault(s=>s.path==path);if(present==null)buildScenes.Add(new EditorBuildSettingsScene(path,true));else present.enabled=true;
        }
        EditorBuildSettings.scenes=buildScenes.ToArray();AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("A11_EXISTING_PROJECT_PREPARED: original scenes, FSMs, colliders, renderers and references retained.");
    }
    [MenuItem("Mountain Adventure/Assignment 11/Verify integration")]
    public static void Verify()=>Assignment11Verification.Run();
    [MenuItem("Mountain Adventure/Assignment 11/Open original trial and player data")]
    public static void OpenForReview()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before opening the review scene.");
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/Scenes/EnemyTrial.unity");
        Selection.activeObject=GameData.Current.player;
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
    public static void OpenCampaignForReview()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before opening the campaign.");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Selection.activeObject=GameData.Current;
    }
    public static void FinalizeData()
    {
        original=JsonUtility.FromJson<CaptureFile>(File.ReadAllText("Assets/Editor/Assignment11OriginalBalance.json"));
        var config=GameData.Current;
        config.trialAfterCampaign=true;EditorUtility.SetDirty(config);
        for(int i=0;i<config.levels.Length;i++)
        {
            config.levels[i].nextLevelId=i+1<config.levels.Length?config.levels[i+1].id:0;
            EditorUtility.SetDirty(config.levels[i]);
        }
        config.enemyTrial.nextLevelId=0;EditorUtility.SetDirty(config.enemyTrial);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/EnemyTrial.unity");
        foreach(var agent in Components(scene).OfType<EnemyAgent2D>())
        {
            if(agent.stateText==null)continue;
            var captured=Capture(agent);
            var label=agent.stateText.rectTransform;
            var position=new Vector3(NumberValue(captured,"stateLabelX",0),NumberValue(captured,"stateLabelY",1.15f),NumberValue(captured,"stateLabelZ",-0.1f));
            // Keep the original world-label placement through RectTransform serialization.
            label.anchoredPosition=new Vector2(position.x,position.y);label.localPosition=position;
            EditorUtility.SetDirty(label);
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("A11_EXISTING_PROJECT_ROUTES_AND_LABELS_CONFIGURED");
    }
    [MenuItem("Mountain Adventure/Assignment 11/Build Android APK")]
    public static void BuildAndroid()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before building.");
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        string output=Environment.GetEnvironmentVariable("MOUNTAIN_ADVENTURE_APK");
        if(string.IsNullOrEmpty(output)){Directory.CreateDirectory("Builds");output=Path.GetFullPath("Builds/MountainAdventure_Assignment11.apk");}
        // Keep the existing application identifier, company and product settings.
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64|AndroidArchitecture.ARMv7;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle=false;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed: "+report.summary.result);
        Debug.Log("A11_EXISTING_PROJECT_APK_BUILT "+output);
    }
}
