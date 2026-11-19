using UnityEngine;

public class EnemyPatrol : EnemyAgent2D
{
    public enum State { Patrol, Chase, WindUp, Attack, Recover, Return }
    public float speed => Balance.speed;
    public float distance => Balance.distance; // Compatibility with previous scenes.
    public float chaseSpeed => Balance.chaseSpeed;
    public float returnSpeed => Balance.returnSpeed;
    public float maxChaseDistance => Balance.maxChaseDistance;
    public float loseSightDelay => Balance.loseSightDelay;
    public float attackRange => Balance.attackRange;
    public float attackHeight => Balance.attackHeight;
    [SerializeField] private State currentState;
    float lostTime, recoveryUntil;
    GameObject fallbackRoute;
    protected override void Start()
    {
        base.Start();
        // Older practice scenes also keep a real waypoint route.
        if (waypoints == null || waypoints.Length == 0)
        {
            fallbackRoute = new GameObject(name + "_Waypoints");
            waypoints = new Transform[2];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = new GameObject("Point_" + i).transform;
                waypoints[i].SetParent(fallbackRoute.transform);
                waypoints[i].position = home + Vector3.right * distance * (i == 0 ? -1f : 1f);
            }
        }
        Set(State.Patrol);
    }
    void OnDestroy() { if (fallbackRoute != null) Destroy(fallbackRoute); }
    void Set(State s) { currentState = s; Enter("GUARD / " + s); }
    bool InRange => Mathf.Abs(PlayerCenter.x - Center.x) <= attackRange
        && Mathf.Abs(PlayerCenter.y - Center.y) <= attackHeight;
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (!Ready) return;
        Tick(currentState == State.WindUp);
        bool seen = HasLineOfSight();
        bool territory = Mathf.Abs(PlayerCenter.x - home.x) <= maxChaseDistance;
        lostTime = seen ? 0f : lostTime + Time.deltaTime;
        switch (currentState)
        {
            case State.Patrol:
                if (seen && territory && AttackAllowed) Set(State.Chase); else FollowWaypoints(speed);
                break;
            case State.Chase:
                FacePlayer();
                if (!territory || lostTime > loseSightDelay) Set(State.Return);
                else if (InRange && AttackAllowed && HasLineOfSight()) Set(State.WindUp);
                else MoveX(Mathf.Clamp(PlayerCenter.x, home.x - maxChaseDistance, home.x + maxChaseDistance), chaseSpeed);
                break;
            case State.WindUp:
                if (!AttackAllowed) { recoveryUntil = Time.time + attackInterval; Set(State.Recover); }
                else if (stateTime >= windUp) Set(State.Attack);
                break;
            case State.Attack:
                if (AttackAllowed && InRange && HasLineOfSight()) player.TakeDamage(damage);
                recoveryUntil = Time.time + attackInterval; Set(State.Recover);
                break;
            case State.Recover:
                if (Time.time >= recoveryUntil) Set(State.Return);
                break;
            case State.Return:
                if (seen && territory && AttackAllowed) Set(State.Chase);
                else if (Mathf.Abs(transform.position.x - home.x) <= waypointTolerance) Set(State.Patrol);
                else if (!MoveX(home.x, returnSpeed)) Set(State.Patrol);
                break;
        }
    }
}
