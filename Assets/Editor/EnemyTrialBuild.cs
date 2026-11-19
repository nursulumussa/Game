using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EnemyTrialBuild
{
    [MenuItem("Mountain Adventure/Build Android APK")]
    public static void BuildAndroid()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building.");
        if (GameData.Current != null)
        {
            Assignment11Integration.BuildAndroid();
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.nursulu.mountainadventure");
        PlayerSettings.productName = "Mountain Adventure";
        PlayerSettings.bundleVersion = "1.10.0";
        EditorUserBuildSettings.buildAppBundle = false;
        string[] scenes = {
            "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Level1.unity",
            "Assets/Scenes/EnemyTrial.unity", "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity", "Assets/Scenes/WinScene.unity"
        };
        Directory.CreateDirectory("Builds");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = scenes, locationPathName = "Builds/MountainAdventure_ThreeEnemies.apk",
            target = BuildTarget.Android, options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Android build failed: " + report.summary.result);
        Debug.Log("APK ready: " + Path.GetFullPath("Builds/MountainAdventure_ThreeEnemies.apk"));
    }

    [MenuItem("Mountain Adventure/Open Three Enemy Level")]
    public static void OpenTrial()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene("Assets/Scenes/EnemyTrial.unity");
    }
}
