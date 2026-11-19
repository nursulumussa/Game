using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
[Serializable] public class SavedSettings { public float master, music, sfx; }
[Serializable] public class SaveData {
    public string format;
    public int version, bestScore, lastLevel;
    public List<int> unlockedLevels, completedLevels;
    public SavedSettings settings;
}
public static class SaveStore {
    public static string PathForSave => Path.Combine(Application.persistentDataPath,"save.json");
    public static SaveData Defaults(GameData config) => new SaveData {
        format="MountainAdventure", version=1, lastLevel=config.levels[0].id,
        unlockedLevels=new List<int>{config.levels[0].id}, completedLevels=new List<int>(),
        settings=new SavedSettings {master=config.defaultMaster,music=config.defaultMusic,sfx=config.defaultSfx}
    };
    public static SaveData Load(string path, GameData config, out string status) {
        try {
            if (!File.Exists(path)) { status="New game"; return Defaults(config); }
            if (new FileInfo(path).Length > 1024*1024) throw new InvalidDataException("Save is too large");
            var data=JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (!Valid(data,config)) throw new InvalidDataException("Invalid save data");
            status="Progress loaded"; return data;
        } catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException) {
            status="Missing or damaged save: defaults restored";
            Debug.LogWarning(status+" ("+e.GetType().Name+")"); return Defaults(config);
        }
    }
    static bool Volume(float v) => !float.IsNaN(v) && !float.IsInfinity(v) && v>=0 && v<=1;
    static bool Known(int id, GameData config) => Array.Exists(config.levels,l=>l.id==id);
    static bool Valid(SaveData d, GameData c) {
        if(d==null || d.format!="MountainAdventure" || d.version!=1 || d.bestScore<0 ||
           d.settings==null || d.unlockedLevels==null || d.completedLevels==null ||
           !Volume(d.settings.master) || !Volume(d.settings.music) || !Volume(d.settings.sfx) ||
           !Known(d.lastLevel,c) || !d.unlockedLevels.Contains(c.levels[0].id) || !d.unlockedLevels.Contains(d.lastLevel)) return false;
        var unlocked=new HashSet<int>(d.unlockedLevels); var completed=new HashSet<int>(d.completedLevels);
        if(unlocked.Count!=d.unlockedLevels.Count || completed.Count!=d.completedLevels.Count) return false;
        foreach(int id in unlocked) if(!Known(id,c)) return false;
        foreach(int id in completed) if(!Known(id,c) || !unlocked.Contains(id)) return false;
        return true;
    }
    public static bool Write(string path, SaveData data, out string status) {
        string temp=path+".tmp";
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write)) {
                byte[] bytes=System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data,true));
                stream.Write(bytes,0,bytes.Length); stream.Flush(true);
            }
            if(File.Exists(path)) {
                try { File.Replace(temp,path,null); }
                catch(PlatformNotSupportedException) { File.Copy(temp,path,true); File.Delete(temp); }
            } else File.Move(temp,path);
            status="Progress saved"; return true;
        } catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException) {
            status="Could not save progress: "+e.GetType().Name; Debug.LogWarning(status); return false;
        }
    }
}
