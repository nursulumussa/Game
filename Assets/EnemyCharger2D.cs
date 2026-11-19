using UnityEngine;

// Locks a direction during wind-up, dashes once, then recovers and returns.
public class EnemyCharger2D : EnemyAgent2D
{
    public enum State { Patrol, WindUp, Dash, Recover, Return }
    public float patrolSpeed => Balance.patrolSpeed;
    public float dashSpeed => Balance.dashSpeed;
    public float dashDuration => Balance.dashDuration;
    public float returnSpeed => Balance.returnSpeed;
    public float attackHeight => Balance.attackHeight;
    public float hitRange => Balance.hitRange;
    [SerializeField] private State currentState;
    int dashDirection;
    bool hitThisDash;
    protected override void Start() { base.Start(); Set(State.Patrol); }
    void Set(State s) { currentState = s; Enter("CHARGER / " + s); }
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (!Ready) return;
        Tick(currentState == State.WindUp);
        switch (currentState)
        {
            case State.Patrol:
                if (HasLineOfSight() && AttackAllowed && Mathf.Abs(PlayerCenter.y - Center.y) <= attackHeight)
                { FacePlayer(); dashDirection = facing; hitThisDash = false; Set(State.WindUp); }
                else FollowWaypoints(patrolSpeed);
                break;
            case State.WindUp:
                if (!AttackAllowed) Set(State.Recover);
                else if (stateTime >= windUp) Set(State.Dash);
                break;
            case State.Dash:
                if (!AttackAllowed || stateTime >= dashDuration) { Set(State.Recover); break; }
                bool moved = MoveX(transform.position.x + dashDirection * dashSpeed, dashSpeed);
                if (!hitThisDash && AttackAllowed && HasLineOfSight(false)
                    && Mathf.Abs(PlayerCenter.x - Center.x) <= hitRange
                    && Mathf.Abs(PlayerCenter.y - Center.y) <= attackHeight)
                { hitThisDash = true; player.TakeDamage(damage); }
                if (!moved) Set(State.Recover);
                break;
            case State.Recover:
                if (stateTime >= attackInterval) Set(State.Return);
                break;
            case State.Return:
                if (Mathf.Abs(transform.position.x - home.x) <= waypointTolerance) Set(State.Patrol);
                else if (!MoveX(home.x, returnSpeed)) Set(State.Patrol);
                break;
        }
    }
}
