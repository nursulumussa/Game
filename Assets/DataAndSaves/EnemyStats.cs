using UnityEngine;
[CreateAssetMenu(menuName="Mountain Adventure/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Senses and movement safety")]
    public float sightDistance, visionAngle, screenMargin, waypointTolerance, bodySkin, groundProbeDistance;
    [Header("Shared attacks")]
    public float windUp, attackInterval;
    public int damage, maxHealth;
    [Header("Patroller")]
    public float speed, distance, chaseSpeed, returnSpeed, maxChaseDistance, loseSightDelay, attackRange, attackHeight;
    [Header("Shooter")]
    public float scanTurnInterval, projectileSpeed, projectileLifetime;
    [Header("Charger")]
    public float patrolSpeed, dashSpeed, dashDuration, hitRange;
}
