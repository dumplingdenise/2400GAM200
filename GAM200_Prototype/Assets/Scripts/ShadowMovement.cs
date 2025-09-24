using UnityEngine;

public class ShadowMovement : MonoBehaviour
{
    // CONTROL SHADOW MODE ONLY

    // movement
    [SerializeField] public float speed = 5f;
    [SerializeField] public float jumpspeed = 10f;

    // For a quick POC we just use collision enter/exit below.
    // Later, prefer: groundCheck Transform + OverlapCircle + shadowGroundMask
    // [SerializeField] private Transform groundCheck;
    // [SerializeField] private float groundRadius = 0.15f;
    // [SerializeField] private LayerMask shadowGroundMask;

    private BoxCollider2D boxCollider;
    private Rigidbody2D shadowrb;

    private bool isGrounded = false; // check for jumping

    //animation
    private Animator animator;
    //private bool isWalking = false;

    private ShadowController shadowController;
    private gameController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shadowrb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        controller = FindAnyObjectByType<gameController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.currentMode != gameController.GameState.Shadow) return;

        shadowMovement();
    }

    void shadowMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(moveInput) < 0.01f) moveInput = 0f; // deadzone (helps avoid flicker)

        shadowrb.linearVelocity = new Vector2(moveInput * speed, shadowrb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            shadowrb.linearVelocity = new Vector2(shadowrb.linearVelocity.x, jumpspeed);
        }

        // face direction (fli[s sprite)
        var s = transform.localScale;

        if (moveInput > 0)
        {
            s.x = +1f;
        }
        else if (moveInput < 0)
        {
            s.x = -1f;
        }
        // if 0, keep last facing
        transform.localScale = s;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
        // animator.SetBool("Grounded", isGrounded);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        // animator.SetBool("Grounded", isGrounded);
    }
}
