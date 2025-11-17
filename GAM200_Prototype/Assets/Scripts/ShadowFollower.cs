using UnityEngine;
using static GameController;

public class ShadowFollower : MonoBehaviour
{
    [HideInInspector] public Transform target;
    public float followSpeed = 8f;
    public float followDistance = 0.8f; // distance behind player

    private Animator animator;
    private Vector3 velocity;
    private bool isFacingRight = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameController.IsPaused) return;
        if (target == null)
        {
            // Stop any residual animation when idle
            StopAnimation();
            return;
        }

        // --- FOLLOWING BEHAVIOR ---
        Vector3 targetPos = target.position;

        // Offset shadow slightly behind player
        float direction = isFacingRight ? -1f : 1f;
        targetPos.x += direction * followDistance;

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.1f);

        // --- DIRECTION & ANIMATION ---
        // Check actual movement this frame
        Vector3 moveDir = velocity.normalized;
        bool isWalking = velocity.magnitude > 0.02f;

        // Match player direction based on velocity
        if (moveDir.x > 0.05f) isFacingRight = true;
        else if (moveDir.x < -0.05f) isFacingRight = false;

        // Apply animator states
        if (animator)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isFacingRight", isFacingRight);
        }
    }
    public void StopAnimation()
    {
        velocity = Vector3.zero;
        if (animator)
        {
            animator.SetBool("isWalking", false);
            // Optionally keep facing direction same:
            animator.SetBool("isFacingRight", isFacingRight);
        }
    }
}


// TESTING shadow rise visual cue

/*using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    public Transform physicalPlayer;
    public Transform target;

    public float followSpeed = 8f;
    public float followDistance = 0.8f;

    public Vector3 flatLocalPos;
    public Quaternion flatLocalRot;
    public Vector3 flatLocalScale;

    private Animator animator;
    private Vector3 velocity;
    private bool isFacingRight = true;

    [HideInInspector] public Vector3 uprightLocalPos;
    [HideInInspector] public Quaternion uprightLocalRot;
    [HideInInspector] public Vector3 uprightLocalScale;


    public bool isFlatMode = true;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Save editor values EXACTLY
        flatLocalPos = transform.localPosition;
        flatLocalRot = transform.localRotation;
        flatLocalScale = transform.localScale;

        // Upright values
        uprightLocalPos = Vector3.zero;
        uprightLocalRot = Quaternion.identity;
        uprightLocalScale = new Vector3(0.2219f, 0.2219f, 0.2219f);

    }

    public void SetFlatMode(bool flat)
    {
        isFlatMode = flat;

        if (flat)
        {
            // ALWAYS USE EDITOR VALUES – DO NOT AUTO UPDATE
            transform.localPosition = flatLocalPos;
            transform.localRotation = flatLocalRot;
            transform.localScale = flatLocalScale;
        }
        else
        {
            transform.localRotation = uprightLocalRot;
            transform.localScale = uprightLocalScale;
        }
    }

    void Update()
    {
        if (GameController.IsPaused) return;
        if (target == null) return;

        // Follow ONLY X axis
        Vector3 pos = transform.position;
        float newX = Mathf.Lerp(pos.x, target.position.x - followDistance, followSpeed * Time.deltaTime);

        // Detect WALKING by checking X movement
        float xVelocity = newX - pos.x;
        bool isWalking = Mathf.Abs(xVelocity) > 0.001f;

        // Apply X but keep original Y
        pos.x = newX;
        transform.position = pos;

        // Update animation
        animator.SetBool("isWalking", isWalking);

        // Update facing direction
        if (xVelocity > 0.001f) animator.SetBool("isFacingRight", true);
        else if (xVelocity < -0.001f) animator.SetBool("isFacingRight", false);
    }


    public void StopAnimation()
    {
        velocity = Vector3.zero;

        if (animator)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isFacingRight", isFacingRight);
        }
    }

}*/
