using UnityEngine;

// Stationary ranged enemy: commits to its aim during the visible warning.
public class EnemyShooter2D : EnemyAgent2D
{
    public enum State { Scan, Aim, Shoot, Cooldown }
    [Header("Shooter tuning")]
    public EnemyProjectile2D projectilePrefab;
    public Transform muzzle;
    public float scanTurnInterval => Balance.scanTurnInterval;
    public float projectileSpeed => Balance.projectileSpeed;
    public float projectileLifetime => Balance.projectileLifetime;
    public float aimLineWidth = 0.035f;
    [SerializeField] private State currentState;
    Vector2 lockedDirection;
    LineRenderer aimLine;
    protected override void Start()
    {
        base.Start();
        if (muzzle == null) muzzle = eyePoint;
        aimLine = GetComponent<LineRenderer>();
        if (aimLine == null) aimLine = gameObject.AddComponent<LineRenderer>();
        aimLine.sharedMaterial = GetComponent<SpriteRenderer>().sharedMaterial;
        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;
        aimLine.startWidth = aimLine.endWidth = aimLineWidth;
        aimLine.startColor = aimLine.endColor = warningColor;
        aimLine.sortingOrder = 15;
        aimLine.enabled = false;
        Set(State.Scan);
    }
    void Set(State s) { currentState = s; Enter("SHOOTER / " + s); }
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (!Ready) return;
        Tick(currentState == State.Aim);
        if (aimLine != null)
        {
            aimLine.enabled = currentState == State.Aim && AttackAllowed;
            if (aimLine.enabled)
            {
                RaycastHit2D wall = Physics2D.Raycast(muzzle.position, lockedDirection, sightDistance, solidMask);
                float length = wall.collider != null ? wall.distance : sightDistance;
                aimLine.SetPosition(0, muzzle.position);
                aimLine.SetPosition(1, muzzle.position + (Vector3)lockedDirection * length);
            }
        }
        switch (currentState)
        {
            case State.Scan:
                if (HasLineOfSight() && AttackAllowed)
                { lockedDirection = (PlayerCenter - (Vector2)muzzle.position).normalized; Set(State.Aim); }
                else if (stateTime >= scanTurnInterval) { facing *= -1; stateTime = 0f; }
                break;
            case State.Aim:
                if (!AttackAllowed || !HasLineOfSight(false)) Set(State.Cooldown);
                else if (stateTime >= windUp) Set(State.Shoot);
                break;
            case State.Shoot:
                if (AttackAllowed && HasLineOfSight(false) && projectilePrefab != null)
                {
                    EnemyProjectile2D shot = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
                    shot.Launch(lockedDirection, projectileSpeed, projectileLifetime, damage, sightMask, gameCamera, screenMargin);
                }
                Set(State.Cooldown);
                break;
            case State.Cooldown:
                if (stateTime >= attackInterval) Set(State.Scan);
                break;
        }
    }
    void OnDisable() { if (aimLine != null) aimLine.enabled = false; }
}
