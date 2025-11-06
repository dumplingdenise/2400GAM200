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
    private bool isFacingRight = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Climbing Settings")]
    public bool canClimb = false;
    public float climbSpeed = 3f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isCrouching;
    private bool canControl = true;
    private bool isTouchingWall = false;
    private bool isJumping = false;

    /*[Header("SFX")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip jumpClip, landClip;
    [SerializeField] public AudioSource footstepSource;*/

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        if (isGrounded)
        {
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float speed = isCrouching ? moveSpeed * crouchMultiplier : moveSpeed;

        // --- CLIMBING ---
        if (canClimb)
        {
            rb.gravityScale = 0f; // disable gravity when climbing
            rb.linearVelocity = new Vector2(move * speed, vertical * moveSpeed); // W/S move up/down
        }
        else
        {
            rb.gravityScale = 1f; // normal gravity
            /*rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);*/
            if (isJumping && isTouchingWall)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
            }
        }

        /*var s = transform.localScale;*/

        bool isWalking = move != 0 && isGrounded;
        if (move > 0) isFacingRight = true;
        else if (move < 0) isFacingRight = false;

        // Send to animator
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isFacingRight", isFacingRight);
        if (!isGrounded)
        {
            // In the air → always idle for now
            animator.SetBool("isWalking", false);
        }
        else
        {
            // On the ground → walk only if moving
            animator.SetBool("isWalking", move != 0);
        }

        /*transform.localScale = s;*/
    }

    void HandleJump()
    {
        if (InputLockManager.instance.canJump && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;

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

    /*bool IsOnLadder()
    {
        // placeholder for ladder trigger logic
        return false;
    }*/

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
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Climbable"))
            canClimb = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Climbable"))
            canClimb = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // ignore ground and ladders
        if (collision.collider.CompareTag("Climbable")) return;

        // only mark as touching wall if contact normal is mostly horizontal
        foreach (var contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isTouchingWall = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingWall = false;
    }
}

