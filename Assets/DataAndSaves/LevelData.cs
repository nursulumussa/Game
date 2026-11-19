using System;
using UnityEngine;
public enum LevelGoal { CoinsAndPortal, Boss }
[Serializable]
public class LevelSpawn
{
    [Tooltip("Path to an existing object. Its references and components stay in the authored scene.")]
    public string sceneObjectPath;
    public GameObject extraPrefab;
    public Vector3 position;
    public Quaternion rotation;
    public bool active;
}
[CreateAssetMenu(menuName="Mountain Adventure/Level Data")]
public class LevelData : ScriptableObject
{
    public int id;
    public int nextLevelId;
    public string displayName, sceneName;
    public LevelGoal goal;
    public int requiredCoins, completionScore;
    public float timeLimit, scorePerSecond;
    public LevelSpawn[] spawns;
    public Transform FindObject(UnityEngine.SceneManagement.Scene scene, string path)
    {
        if(string.IsNullOrEmpty(path)) return null;
        int slash=path.IndexOf('/');
        string rootName=slash<0?path:path.Substring(0,slash);
        foreach(var root in scene.GetRootGameObjects())
            if(root.name==rootName) return slash<0?root.transform:root.transform.Find(path.Substring(slash+1));
        return null;
    }
    public void Apply(UnityEngine.SceneManagement.Scene scene)
    {
        foreach(var spawn in spawns)
        {
            if(spawn.extraPrefab!=null)
            {
                var added=Instantiate(spawn.extraPrefab,spawn.position,spawn.rotation);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(added,scene);
                added.SetActive(spawn.active);
                continue;
            }
            var target=FindObject(scene,spawn.sceneObjectPath);
            if(target==null) { Debug.LogError("LevelData object not found: "+sceneName+"/"+spawn.sceneObjectPath);continue; }
            target.SetPositionAndRotation(spawn.position,spawn.rotation);
            target.gameObject.SetActive(spawn.active);
        }
    }
}
