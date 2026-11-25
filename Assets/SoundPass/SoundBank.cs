using UnityEngine;
using UnityEngine.Audio;

public enum SoundEvent { Jump, Shot, Hit, Pickup, GuardWindup, ShooterWindup, ChargerWindup, EnemyAttack, Death, Click, Win, Lose }

[CreateAssetMenu(menuName="Mountain Adventure/Sound Bank")]
public class SoundBank : ScriptableObject
{
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup, sfxGroup;
    public AudioMixerSnapshot normal, paused;
    public AudioClip menuMusic, levelMusic;
    public AudioClip[] events;
    [Range(0,1)] public float musicGain=0.55f, effectGain=0.65f;
    public float crossfadeSeconds=1f, snapshotSeconds=0.25f;
    public Vector2 pitchRange=new Vector2(0.92f,1.08f);
}
