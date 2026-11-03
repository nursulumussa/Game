
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 7f;
    public float jumpForce = 12f;

    public AudioSource audioSource;
    public AudioClip jumpSound;

    private Rigidbody2D rb;
    private Vector2 move;
    private bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (grounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x, jumpForce
            );

            grounded = false;

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }

    public void OnFire()
    {
        GetComponent<Shooter>().Shoot();
    }

    void FixedUpdate()
    {
        GetComponent<Shooter>().SetDirection(move.x);

        rb.linearVelocity = new Vector2(
            move.x * speed,
            rb.linearVelocity.y
        );
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("GroundSurface"))
        {
            grounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("GroundSurface"))
        {
            grounded = false;
        }
    }
}
