using UnityEngine;
using TMPro;

public class EnemyPatrol : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack,
        Return
    }

    [Header("References")]
    public PlayerHealth player;
    public Transform eyePoint;
    public TMP_Text stateText;

    [Header("Movement")]
    public float speed = 1f;
    public float distance = 0.8f;
    public float chaseSpeed = 1.8f;
    public float returnSpeed = 1f;
    public float maxChaseDistance = 4f;

    [Header("Vision")]
    public float sightDistance = 4f;
    public float loseSightDelay = 0.5f;
    public LayerMask sightMask;

    [Header("Attack")]
    public float attackRange = 1f;
    public float attackHeight = 0.8f;
    public float attackInterval = 1.2f;
    public int damage = 1;

    [Header("Current state — observation only")]
    [SerializeField] private State currentState = State.Patrol;

    private Vector3 home;
    private int direction = 1;
    private float lostSightTimer;
    private float nextAttackTime;
    private Collider2D playerCollider;

    void Start()
    {
        home = transform.position;

        if (player == null || eyePoint == null)
        {
            Debug.LogError(
                "Enemy: assign Player and Eye Point.", this
            );

            enabled = false;
            return;
        }

        playerCollider = player.GetComponent<Collider2D>();

        if (playerCollider == null)
        {
            Debug.LogError(
                "Enemy: Player needs a Collider2D.", this
            );

            enabled = false;
            return;
        }

        currentState = State.Patrol;
        UpdateStateText();
    }

    void Update()
    {
        if (player == null)
            return;

        bool canSee = CanSeePlayer();

        // The player must stay within the enemy's territory.
        bool playerInsideTerritory =
            Mathf.Abs(player.transform.position.x - home.x)
            <= maxChaseDistance;

        bool detected = canSee && playerInsideTerritory;

        if (detected)
            lostSightTimer = 0f;
        else
            lostSightTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol:
                if (detected)
                {
                    ChangeState(State.Chase);
                    return;
                }

                Patrol();
                break;

            case State.Chase:
                if (!playerInsideTerritory ||
                    lostSightTimer >= loseSightDelay)
                {
                    ChangeState(State.Return);
                    return;
                }

                if (detected && PlayerInAttackRange())
                {
                    ChangeState(State.Attack);
                    return;
                }

                MoveTowardsX(
                    player.transform.position.x,
                    chaseSpeed
                );
                break;

            case State.Attack:
                if (!playerInsideTerritory ||
                    lostSightTimer >= loseSightDelay)
                {
                    ChangeState(State.Return);
                    return;
                }

                FacePlayer();

                if (!PlayerInAttackRange())
                {
                    ChangeState(State.Chase);
                    return;
                }

                // Recheck visibility before every hit.
                if (CanSeePlayer() &&
                    Time.time >= nextAttackTime)
                {
                    player.TakeDamage(damage);
                    nextAttackTime =
                        Time.time + attackInterval;
                }
                break;

            case State.Return:
                if (detected)
                {
                    ChangeState(State.Chase);
                    return;
                }

                if (Mathf.Abs(transform.position.x - home.x)
                    <= 0.05f)
                {
                    ChangeState(State.Patrol);
                    return;
                }

                MoveTowardsX(home.x, returnSpeed);
                break;
        }
    }

    void Patrol()
    {
        float left = home.x - distance;
        float right = home.x + distance;

        if (transform.position.x >= right)
            direction = -1;
        else if (transform.position.x <= left)
            direction = 1;

        float destination = direction > 0 ? right : left;

        MoveTowardsX(destination, speed);
    }

    void MoveTowardsX(float targetX, float moveSpeed)
    {
        float difference = targetX - transform.position.x;

        if (Mathf.Abs(difference) > 0.01f)
            direction = difference > 0f ? 1 : -1;

        Vector3 position = transform.position;

        position.x = Mathf.MoveTowards(
            position.x,
            targetX,
            moveSpeed * Time.deltaTime
        );

        transform.position = position;
    }

    void FacePlayer()
    {
        float difference =
            player.transform.position.x - transform.position.x;

        if (Mathf.Abs(difference) > 0.01f)
            direction = difference > 0f ? 1 : -1;
    }

    bool CanSeePlayer()
    {
        Vector2 origin = eyePoint.position;
        Vector2 target = playerCollider.bounds.center;
        Vector2 offset = target - origin;

        float length = offset.magnitude;

        if (length <= 0.001f || length > sightDistance)
            return false;

        // Ignore a player behind the enemy.
        if (offset.x * direction < 0f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            offset.normalized,
            length,
            sightMask
        );

        bool seesPlayer =
            hit.collider != null &&
            hit.collider.GetComponentInParent<PlayerHealth>()
            == player;

        Debug.DrawRay(
            origin,
            offset,
            seesPlayer ? Color.green : Color.red
        );

        return seesPlayer;
    }

    bool PlayerInAttackRange()
    {
        Vector2 difference =
            playerCollider.bounds.center -
            transform.position;

        return Mathf.Abs(difference.x) <= attackRange
            && Mathf.Abs(difference.y) <= attackHeight;
    }

    void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        Debug.Log(
            name + ": " + currentState,
            this
        );

        UpdateStateText();
    }

    void UpdateStateText()
    {
        if (stateText != null)
            stateText.text = currentState.ToString();
    }
}