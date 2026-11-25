using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;

// Editor-only reflection accesses Unity's own mixer authoring API, never the player.
public static class Assignment12Setup
{
    const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static Type T(string name)=>typeof(Editor).Assembly.GetType("UnityEditor.Audio."+name,true);
    static object Get(object o,string n)=>o.GetType().GetProperty(n,Flags).GetValue(o);
    static void Set(object o,string n,object v)=>o.GetType().GetProperty(n,Flags).SetValue(o,v);
    static object Call(object o,string n,params object[] a)=>o.GetType().GetMethods(Flags).First(m=>m.Name==n && m.GetParameters().Length==a.Length).Invoke(o,a);
    public static void InspectAPI()
    {
        foreach(var name in new[]{"AudioMixerController","AudioMixerGroupController","AudioMixerEffectController","AudioMixerSnapshotController"})
        {
            var t=T(name);Debug.Log("A12_API "+name+"\n"+string.Join("\n",t.GetMembers(Flags).Where(m=>m.DeclaringType==t).Select(m=>m.ToString())));
        }
    }
    [MenuItem("Mountain Adventure/Assignment 12/Prepare sound pass")]
    public static void Prepare()
    {
        const string dir="Assets/Resources/SoundPass/";
        Directory.CreateDirectory(dir);
        var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(dir+"SoundBank.asset");
        if(bank==null)
        {
            // A failed first-time setup can leave only this generated mixer behind.
            if(AssetDatabase.LoadMainAssetAtPath(dir+"Adventure.mixer")!=null)AssetDatabase.DeleteAsset(dir+"Adventure.mixer");
            var mixer=(AudioMixer)T("AudioMixerController").GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{dir+"Adventure.mixer"});
            var master=Get(mixer,"masterGroup");
            var music=Call(mixer,"CreateNewGroup","Music",false);Call(mixer,"AddChildToParent",music,master);
            var tone=Call(mixer,"CreateNewGroup","Music Tone",false);Call(mixer,"AddChildToParent",tone,music);
            var sfx=Call(mixer,"CreateNewGroup","SFX",false);Call(mixer,"AddChildToParent",sfx,master);
            // Snapshot volume is on a child, so it never overrides the exposed saved slider.
            var lowpass=Activator.CreateInstance(T("AudioMixerEffectController"),Flags,null,new object[]{"Lowpass"},null);
            Call(lowpass,"PreallocateGUIDs");AssetDatabase.AddObjectToAsset((UnityEngine.Object)lowpass,mixer);Call(tone,"InsertEffect",lowpass,1);
            var normal=((Array)Get(mixer,"snapshots")).GetValue(0);((UnityEngine.Object)normal).name="Normal";
            var volume=Call(tone,"GetGUIDForVolume");var cutoff=Call(lowpass,"GetGUIDForParameter","Cutoff freq");
            Call(normal,"SetValue",volume,0f);Call(normal,"SetValue",cutoff,22000f);
            Call(mixer,"CloneNewSnapshotFromTarget",false);
            var paused=Get(mixer,"TargetSnapshot");((UnityEngine.Object)paused).name="Paused";
            Call(paused,"SetValue",volume,-12f);Call(paused,"SetValue",cutoff,1000f);
            Set(mixer,"TargetSnapshot",normal);Set(mixer,"startSnapshot",normal);
            // ExposedAudioParameter uses GUIDs allocated by Unity rather than handwritten YAML.
            var prop=mixer.GetType().GetProperty("exposedParameters",Flags);var element=prop.PropertyType.GetElementType();var parameters=Array.CreateInstance(element,3);
            object[] groups={master,music,sfx};string[] names={"MasterVolume","MusicVolume","SFXVolume"};
            for(int i=0;i<3;i++)
            {
                var param=Activator.CreateInstance(element);element.GetField("guid",Flags).SetValue(param,Call(groups[i],"GetGUIDForVolume"));element.GetField("name",Flags).SetValue(param,names[i]);parameters.SetValue(param,i);
            }
            prop.SetValue(mixer,parameters);
            var viewProp=mixer.GetType().GetProperty("views",Flags);var viewType=viewProp.PropertyType.GetElementType();
            var view=Activator.CreateInstance(viewType);viewType.GetField("name",Flags).SetValue(view,"All channels");
            var allGroups=groups.Concat(new[]{tone}).ToArray();var guidType=Get(master,"groupID").GetType();var guids=Array.CreateInstance(guidType,allGroups.Length);
            for(int i=0;i<allGroups.Length;i++)guids.SetValue(Get(allGroups[i],"groupID"),i);
            viewType.GetField("guids",Flags).SetValue(view,guids);var views=Array.CreateInstance(viewType,1);views.SetValue(view,0);viewProp.SetValue(mixer,views);
            bank=ScriptableObject.CreateInstance<SoundBank>();bank.mixer=mixer;bank.musicGroup=(AudioMixerGroup)tone;bank.sfxGroup=(AudioMixerGroup)sfx;
            bank.normal=(AudioMixerSnapshot)normal;bank.paused=(AudioMixerSnapshot)paused;
            bank.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(dir+"menu-music.wav");bank.levelMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(dir+"level-music.wav");
            bank.events=Enum.GetNames(typeof(SoundEvent)).Select(n=>AssetDatabase.LoadAssetAtPath<AudioClip>(dir+n+".wav")).ToArray();
            if(bank.events.Any(c=>c==null) || bank.menuMusic==null || bank.levelMusic==null)throw new Exception("Generated audio missing");
            AssetDatabase.CreateAsset(bank,dir+"SoundBank.asset");
            foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(dir+"Adventure.mixer"))EditorUtility.SetDirty(obj);
        }
        foreach(var path in Directory.GetFiles(dir,"*.wav"))
        {
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var sample=importer.defaultSampleSettings;
            sample.loadType=path.Contains("music")?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.quality=0.7f;importer.defaultSampleSettings=sample;importer.forceToMono=true;importer.SaveAndReimport();
        }
        // Remove old clip references from build scenes, leaving all visuals and geometry alone.
        foreach(var sceneInfo in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            var scene=EditorSceneManager.OpenScene(sceneInfo.path);
            foreach(var go in scene.GetRootGameObjects())foreach(var source in go.GetComponentsInChildren<AudioSource>(true))
            {source.clip=null;source.playOnAwake=false;source.outputAudioMixerGroup=bank.sfxGroup;EditorUtility.SetDirty(source);}
            foreach(var root in scene.GetRootGameObjects())foreach(var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(!(behaviour is PlayerController || behaviour is Shooter || behaviour is Coin || behaviour is ButtonSound))continue;
                var serialized=new SerializedObject(behaviour);
                foreach(string field in new[]{"jumpSound","shootSound","coinSound","clickSound"}){var property=serialized.FindProperty(field);if(property!=null)property.objectReferenceValue=null;}
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");Debug.Log("A12_PREPARED");
    }
    public static void Build()
    {
        PlayerSettings.bundleVersion="1.1";PlayerSettings.Android.bundleVersionCode=2;
        Assignment11Integration.BuildAndroid();
    }
    public static void OpenForReview()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Selection.activeObject=Resources.Load<SoundBank>("SoundPass/SoundBank").mixer;
        EditorApplication.ExecuteMenuItem("Window/Audio/Audio Mixer");
    }
}
