
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private PlayerStats Stats => GameData.Current.player;


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
        if (Time.timeScale == 0) return;
        if (grounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x, Stats.jumpForce
            );

            grounded = false;

            AdventureAudio.Play(SoundEvent.Jump);
        }
    }

    public void OnFire()
    {
        if (Time.timeScale == 0) return;
        GetComponent<Shooter>().Shoot();
    }

    void FixedUpdate()
    {
        GetComponent<Shooter>().SetDirection(move.x);

        rb.linearVelocity = new Vector2(
            move.x * Stats.moveSpeed,
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
