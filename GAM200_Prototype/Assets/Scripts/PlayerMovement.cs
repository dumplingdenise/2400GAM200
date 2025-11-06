using UnityEngine;
using static GameController;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float crouchMultiplier = 0.5f;

    private Animator animator;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isCrouching;
    private bool canControl = true;

    /*[Header("SFX")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip jumpClip, landClip;
    [SerializeField] public AudioSource footstepSource;*/

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameController.IsPaused) return;
        if (!canControl) return;

        CheckGround();
        HandleMovement();
        HandleJump();
        HandleCrouch();

    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        /*float vertical = Input.GetAxisRaw("Vertical");*/
        float speed = isCrouching ? moveSpeed * crouchMultiplier : moveSpeed;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // optional: ladder/stair movement
        /* if (vertical != 0 && IsOnLadder())
             rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * moveSpeed);*/

        var s = transform.localScale;

        if (move > 0)
        {
            s.x = Mathf.Abs(s.x);
        }
        else if (move < 0)
        {
            s.x = -Mathf.Abs(s.x);
        }
        else
        {

        }

        /*if (!isGrounded)
        {
            // In the air → always idle for now
            animator.SetBool("isWalking", false);
        }
        else
        {
            // On the ground → walk only if moving
            animator.SetBool("isWalking", move != 0);
        }*/

        transform.localScale = s;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            /*if (sfx && jumpClip)
            {
                //sfx
                sfx.pitch = Random.Range(0.98f, 1.02f);
                sfx.PlayOneShot(jumpClip);
            }*/
        }          
    }

    void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    bool IsOnLadder()
    {
        // placeholder for ladder trigger logic
        return false;
    }

    public void SetActiveControl(bool active)
    {
        canControl = active;
        rb.simulated = active;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}

