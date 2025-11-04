using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float crouchMultiplier = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isCrouching;
    private bool canControl = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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
        float vertical = Input.GetAxisRaw("Vertical");
        float speed = isCrouching ? moveSpeed * crouchMultiplier : moveSpeed;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // optional: ladder/stair movement
        if (vertical != 0 && IsOnLadder())
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * moveSpeed);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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

