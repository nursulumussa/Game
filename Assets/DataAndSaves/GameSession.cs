using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Common data/save integration for the original scenes. No replacement Gameplay scene.
public class GameSession : MonoBehaviour
{
    public static GameSession Instance {get;private set;}
    public GameData Config {get;private set;}
    public SaveData Save {get;private set;}
    public LevelData Level {get;private set;}
    public string SaveStatus {get;private set;}
    public int Coins {get;private set;}
    public int LastScore {get;private set;}
    public float Remaining {get;private set;}
    public bool Playing {get;private set;}
    public AdventureUI UI {get;private set;}
    public bool IsTrial => Level!=null && Level==Config.enemyTrial;
    public bool IsCampaignTrial => IsTrial && campaignTrial;
    public bool CanEnterFinale => Config.trialAfterCampaign && Save.completedLevels.Contains(Config.levels[Config.levels.Length-1].id);
    public bool GoalMet => Level!=null && (Level.goal==LevelGoal.Boss?bossDefeated:Coins>=Level.requiredCoins);
    bool bossDefeated;
    bool campaignTrial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if(Instance==null) new GameObject("Mountain Adventure - Data and Saves").AddComponent<GameSession>();
    }
    void Awake()
    {
        if(Instance!=null && Instance!=this) {Destroy(gameObject);return;}
        Instance=this;DontDestroyOnLoad(gameObject);Config=GameData.Current;
        if(Config==null) {Debug.LogError("Run Mountain Adventure/Assignment 11/Prepare data first.");enabled=false;return;}
        Save=SaveStore.Load(SaveStore.PathForSave,Config,out var status);SaveStatus=status;
        AdventureAudio.Ensure();
        UI=gameObject.AddComponent<AdventureUI>();
        SceneManager.sceneLoaded+=Loaded;
    }
    void OnDestroy() {SceneManager.sceneLoaded-=Loaded;if(Instance==this)Instance=null;}
    void Loaded(Scene scene,LoadSceneMode mode)
    {
        Time.timeScale=1;Playing=false;Coins=0;bossDefeated=false;
        Level=Array.Find(Config.levels,l=>l.sceneName==scene.name);
        if(Level==null && Config.enemyTrial!=null && Config.enemyTrial.sceneName==scene.name)Level=Config.enemyTrial;
        if(!IsTrial)campaignTrial=false;
        if(Level!=null)
        {
            Level.Apply(scene);Remaining=Level.timeLimit;Playing=true;UI.ShowGame();
        }
        else UI.ShowHome();
        ApplyAudio();
    }
    void Update()
    {
        if(!Playing)return;
        Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
        if(Remaining<=0) {Playing=false;AdventureAudio.Play(SoundEvent.Lose);UI.ShowFailure();}
    }
    public void StartLevel(int id)
    {
        var level=Array.Find(Config.levels,l=>l.id==id);
        if(level==null || !Save.unlockedLevels.Contains(id))return;
        Save.lastLevel=id;Persist();Time.timeScale=1;SceneManager.LoadScene(level.sceneName);
    }
    public void StartTrial()
    {
        LoadTrial(false);
    }
    void LoadTrial(bool finale)
    {
        campaignTrial=finale;EnemyTrialSession.ResetAttempts();Time.timeScale=1;SceneManager.LoadScene(Config.enemyTrial.sceneName);
    }
    public void ContinueFinalTrial()
    {
        if(!CanEnterFinale)return;
        Save.campaignTrialPending=!Save.campaignCompleted;
        Persist();LoadTrial(true);
    }
    public void Retry() {if(Level!=null){Time.timeScale=1;SceneManager.LoadScene(Level.sceneName);}}
    public void Menu() {Persist();Time.timeScale=1;SceneManager.LoadScene(Config.menuScene);}
    public void AddCoins(int value) {if(Playing)Coins+=value;}
    public void BossDefeated()
    {
        if(!Playing || Level==null || Level.goal!=LevelGoal.Boss)return;
        bossDefeated=true;Complete(Config.winScene);
    }
    public void Complete(string nextScene)
    {
        if(!Playing || !GoalMet)return;
        Playing=false;
        LastScore=Coins*Config.items.scorePerCoin+Level.completionScore+Mathf.FloorToInt(Remaining*Level.scorePerSecond);
        Save.bestScore=Mathf.Max(Save.bestScore,LastScore);
        if(!IsTrial)
        {
            if(!Save.unlockedLevels.Contains(Level.id))Save.unlockedLevels.Add(Level.id);
            if(!Save.completedLevels.Contains(Level.id))Save.completedLevels.Add(Level.id);
            int index=Array.IndexOf(Config.levels,Level);
            if(index+1<Config.levels.Length && !Save.unlockedLevels.Contains(Config.levels[index+1].id))Save.unlockedLevels.Add(Config.levels[index+1].id);
        }
        var next=IsTrial?null:Array.Find(Config.levels,l=>l.id==Level.nextLevelId);
        if(IsCampaignTrial)
        {
            Save.campaignTrialPending=false;Save.campaignCompleted=true;
        }
        if(!IsTrial && next==null && Config.trialAfterCampaign)
        {
            ContinueFinalTrial();return;
        }
        Persist();
        if(next!=null && Save.unlockedLevels.Contains(next.id))StartLevel(next.id);
        else {Time.timeScale=1;SceneManager.LoadScene(Config.winScene);}
    }
    public void Persist()
    {
        if(Save==null)return;
        SaveStore.Write(SaveStore.PathForSave,Save,out var status);SaveStatus=status;
    }
    public void Settings(float master,float music,float sfx)
    {
        Save.settings.master=Mathf.Clamp01(master);Save.settings.music=Mathf.Clamp01(music);Save.settings.sfx=Mathf.Clamp01(sfx);
        ApplyAudio();Persist();
    }
    public void ApplyAudio()
    {
        if(AdventureAudio.Instance!=null)AdventureAudio.Instance.Apply(Save.settings);
    }
    public void ResetProgress() {Save=SaveStore.Defaults(Config);Persist();ApplyAudio();Menu();}
    void OnApplicationPause(bool paused) {if(paused)Persist();}
    void OnApplicationFocus(bool focused) {if(!focused)Persist();}
    void OnApplicationQuit() {Persist();}
}
