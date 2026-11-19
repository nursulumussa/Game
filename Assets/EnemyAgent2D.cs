using UnityEngine;
using TMPro;

// Shared senses and presentation. The three subclasses own different FSMs.
public abstract class EnemyAgent2D : MonoBehaviour
{
    public EnemyStats stats;
    protected EnemyStats Balance => stats!=null?stats:GameData.Current.defaultEnemy;
    [Header("References")]
    public PlayerHealth player;
    public Transform eyePoint;
    public TMP_Text stateText;
    public Transform[] waypoints;
    [Header("Sight and movement safety")]
    public LayerMask sightMask;
    public LayerMask solidMask;
    public float sightDistance => Balance.sightDistance;
    public float visionAngle => Balance.visionAngle;
    public float screenMargin => Balance.screenMargin;
    public int startingDirection = -1;
    public float waypointTolerance => Balance.waypointTolerance;
    public float bodySkin => Balance.bodySkin;
    public float groundProbeDistance => Balance.groundProbeDistance;
    public float windUp => Balance.windUp;
    public float attackInterval => Balance.attackInterval;
    public int damage => Balance.damage;
    [Header("Warning")]
    public Color warningColor = new Color(1f, 0.25f, 0.12f);
    public Color labelColor = new Color(0.08f, 0.16f, 0.22f);
    public float warningPulseSpeed = 12f;
    [Range(0f, 1f)] public float warningStrength = 0.9f;
    protected Collider2D body, playerBody;
    protected Camera gameCamera;
    protected Vector3 home;
    protected int facing, waypointIndex;
    protected float stateTime;
    SpriteRenderer[] sprites;
    Color[] colors;
    string shownState;
    readonly RaycastHit2D[] hits = new RaycastHit2D[16];
    protected virtual void Start()
    {
        home = transform.position;
        facing = startingDirection < 0 ? -1 : 1;
        body = GetComponent<Collider2D>();
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
        if (eyePoint == null) eyePoint = transform;
        if (player != null) playerBody = player.GetComponent<Collider2D>();
        gameCamera = Camera.main;
        if (sightMask.value == 0) sightMask = LayerMask.GetMask("Player", "Ground", "Obstacle");
        if (solidMask.value == 0) solidMask = LayerMask.GetMask("Ground", "Obstacle");
        sprites = GetComponentsInChildren<SpriteRenderer>();
        colors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) colors[i] = sprites[i].color;
        if (body == null || playerBody == null || gameCamera == null)
        { Debug.LogError(name + ": collider, player or Main Camera missing.", this); enabled = false; }
    }
    protected bool Ready => player != null && playerBody != null;
    protected Vector2 PlayerCenter => playerBody.bounds.center;
    protected Vector2 Center => body.bounds.center;
    protected bool OnScreen(Vector3 point)
    {
        Vector3 v = gameCamera.WorldToViewportPoint(point);
        return v.z > 0f && v.x > screenMargin && v.x < 1f - screenMargin
            && v.y > screenMargin && v.y < 1f - screenMargin;
    }
    protected bool AttackAllowed => Ready && OnScreen(Center) && OnScreen(PlayerCenter);
    protected bool HasLineOfSight(bool checkAngle = true)
    {
        if (!Ready) return false;
        Vector2 origin = eyePoint.position;
        Vector2 offset = PlayerCenter - origin;
        if (offset.magnitude > sightDistance) return false;
        if (checkAngle && Vector2.Angle(Vector2.right * facing, offset) > visionAngle * 0.5f) return false;
        Physics2D.SyncTransforms();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(sightMask);
        filter.useTriggers = true;
        int count = Physics2D.Raycast(origin, offset.normalized, filter, hits, offset.magnitude + bodySkin);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.transform.IsChildOf(transform)) continue;
            bool found = hits[i].collider.GetComponentInParent<PlayerHealth>() == player;
            Debug.DrawLine(origin, hits[i].point, found ? Color.green : Color.red);
            return found;
        }
        return false;
    }
    protected void FacePlayer()
    {
        if (Mathf.Abs(PlayerCenter.x - Center.x) > waypointTolerance)
            facing = PlayerCenter.x > Center.x ? 1 : -1;
    }
    // Swept wall collision and a downward ground probe protect moving enemies.
    protected bool MoveX(float target, float speed)
    {
        float delta = Mathf.MoveTowards(transform.position.x, target, speed * Time.deltaTime) - transform.position.x;
        if (Mathf.Abs(delta) < Mathf.Epsilon) return true;
        facing = delta > 0f ? 1 : -1;
        Physics2D.SyncTransforms();
        Bounds b = body.bounds;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(solidMask); filter.useTriggers = false;
        Vector2 size = (Vector2)b.size - Vector2.one * bodySkin * 2f;
        int count = Physics2D.BoxCast(b.center, size, 0f, Vector2.right * facing,
            filter, hits, Mathf.Abs(delta) + bodySkin);
        if (count > 0) return false;
        Vector2 foot = new Vector2(b.center.x + facing * (b.extents.x + Mathf.Abs(delta)), b.min.y + bodySkin);
        count = Physics2D.Raycast(foot, Vector2.down, filter, hits, groundProbeDistance);
        if (count == 0) return false;
        transform.position += Vector3.right * delta;
        return true;
    }
    protected void FollowWaypoints(float speed)
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Transform point = waypoints[waypointIndex];
        if (point == null) return;
        if (Mathf.Abs(transform.position.x - point.position.x) <= waypointTolerance || !MoveX(point.position.x, speed))
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }
    protected void Enter(string label)
    { stateTime = 0f; shownState = label; if (stateText != null) stateText.text = label; }
    protected void Tick(bool warning)
    {
        stateTime += Time.deltaTime;
        float amount = warning ? warningStrength * (0.65f + 0.35f * Mathf.Sin(stateTime * warningPulseSpeed)) : 0f;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = Color.Lerp(colors[i], warningColor, amount);
        if (stateText != null)
        { stateText.text = warning ? shownState + " !" : shownState; stateText.color = warning ? warningColor : labelColor; }
    }
    protected virtual void OnDrawGizmosSelected()
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position;
        int dir = Application.isPlaying ? facing : startingDirection;
        Gizmos.color = Color.yellow;
        Vector3 forward = Vector3.right * dir;
        Gizmos.DrawRay(origin, Quaternion.Euler(0, 0, visionAngle * 0.5f) * forward * sightDistance);
        Gizmos.DrawRay(origin, Quaternion.Euler(0, 0, -visionAngle * 0.5f) * forward * sightDistance);
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++) if (waypoints[i] != null)
        {
            Gizmos.DrawWireSphere(waypoints[i].position, waypointTolerance);
            if (i > 0 && waypoints[i - 1] != null) Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
        }
    }
}
