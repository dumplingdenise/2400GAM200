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
