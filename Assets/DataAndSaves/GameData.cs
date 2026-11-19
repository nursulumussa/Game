using UnityEngine;
[CreateAssetMenu(menuName="Mountain Adventure/Game Data")]
public class GameData : ScriptableObject
{
    public PlayerStats player;
    public ItemValues items;
    public LevelData[] levels;
    public LevelData enemyTrial;
    public bool trialAfterCampaign;
    public EnemyStats defaultEnemy;
    public float enemyProjectileRadius;
    [Range(0,1)] public float defaultMaster, defaultMusic, defaultSfx;
    public string menuScene, winScene;
    static GameData current;
    public static GameData Current => current!=null?current:(current=Resources.Load<GameData>("GameData"));
}
