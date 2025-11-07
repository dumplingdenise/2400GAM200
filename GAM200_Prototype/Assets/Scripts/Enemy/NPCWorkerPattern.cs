using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCWorkerPattern : MonoBehaviour
{
    [Header("Core Timing")]
    [SerializeField] float idleDuration = 1.5f;      // idle before walking
    [SerializeField] float cycleDelay = 0.5f;      // tiny pause after reaching edge

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;           // units/sec (always positive)
    private Animator animator;

    [HideInInspector] public bool isMoving;          // external read (boss level controller)
    private bool facingRight = true;
    [SerializeField] SpriteRenderer sr;              // assign in Inspector (boss & rats)

    [Header("Follower Settings")]
    [SerializeField] bool isFollower = false;        // check for rats
    [SerializeField] NPCWorkerPattern leader;        // boss reference for followers

    [Header("March-To-Edge")]
    [SerializeField] Transform leftEdge;             // assign in Inspector
    [SerializeField] Transform rightEdge;            // assign in Inspector
    [SerializeField] float edgeTolerance = 0.06f;    // how close counts as “at the edge”
    [SerializeField] bool chooseRandomDir = true;    // randomize direction each cycle

    [Header("Mid-Walk Stops (Multiple)")]
    [SerializeField] Vector2Int midStopCountRange = new Vector2Int(1, 2); // choose 1–2 stops
    [SerializeField] Vector2 midStopWindow = new Vector2(0.30f, 0.70f); // between 30–70% of path
    [SerializeField] Vector2 midStopDuration = new Vector2(0.25f, 0.60f); // each stop lasts 0.25–0.6s

    int dir = +1; // +1 = right, -1 = left

    // Helpers
    bool AtLeft() => Mathf.Abs(transform.position.x - leftEdge.position.x) <= edgeTolerance;
    bool AtRight() => Mathf.Abs(transform.position.x - rightEdge.position.x) <= edgeTolerance;
    float TargetX() => (dir > 0 ? rightEdge.position.x : leftEdge.position.x);

    void Start()
    {
        animator = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (!isFollower)
        {
            StartCoroutine(PatternLoop()); // boss runs its own rhythm
        }

        // initial facing
        animator.SetBool("FacingRight", dir > 0);
        sr.flipX = !(dir > 0);

        // (Optional) early sync for followers only if leader exists
        if (isFollower && leader != null && Time.timeSinceLevelLoad < 0.1f)
        {
            bool leaderRight = leader.moveSpeed > 0; // (kept as-is per your request)
            animator.SetBool("FacingRight", leaderRight);
            sr.flipX = !leaderRight;
        }
    }

    IEnumerator PatternLoop()
    {
        while (true)
        {
            // ----- Idle phase -----
            isMoving = false;
            animator.SetBool("IsMoving", false);
            yield return new WaitForSeconds(idleDuration);

            // ----- Decide direction for this march -----
            if (chooseRandomDir)
            {
                if (AtLeft()) dir = +1;
                else if (AtRight()) dir = -1;
                else dir = (Random.value < 0.5f ? -1 : +1);
            }
            animator.SetBool("FacingRight", dir > 0);
            sr.flipX = !(dir > 0);

            // ----- March to edge with multiple random stops -----
            isMoving = true;
            animator.SetBool("IsMoving", true);

            float startX = transform.position.x;
            float targetX = TargetX();

            // Build a sorted list of stop positions along the path
            List<float> plannedStops = new List<float>();
            int nStops = Mathf.Max(0, Random.Range(midStopCountRange.x, midStopCountRange.y + 1));

            // only create stops if there is actual distance to travel
            if (Mathf.Abs(targetX - startX) > edgeTolerance && nStops > 0)
            {
                for (int i = 0; i < nStops; i++)
                {
                    float t = Random.Range(midStopWindow.x, midStopWindow.y); // 0..1 along the path
                    float stopX = Mathf.Lerp(startX, targetX, t);
                    plannedStops.Add(stopX);
                }
                plannedStops.Sort(); // walk will hit them in order
            }

            int nextStopIndex = 0;

            // Walk until at the edge
            while (Mathf.Abs(transform.position.x - targetX) > edgeTolerance)
            {
                // move step
                float step = moveSpeed * Time.deltaTime;
                float newX = Mathf.MoveTowards(transform.position.x, targetX, step);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);

                // mid-stop check (if any left)
                if (nextStopIndex < plannedStops.Count)
                {
                    float stopX = plannedStops[nextStopIndex];
                    if (Mathf.Abs(newX - stopX) <= edgeTolerance)
                    {
                        // brief idle in the middle
                        animator.SetBool("IsMoving", false);
                        float pause = Random.Range(midStopDuration.x, midStopDuration.y);
                        float tNow = 0f;
                        while (tNow < pause)
                        {
                            tNow += Time.deltaTime;
                            yield return null; // wait without freezing the game
                        }
                        animator.SetBool("IsMoving", true);
                        nextStopIndex++; // move on to the next planned stop
                    }
                }

                yield return null;
            }

            // Snap exactly to edge X (optional polish)
            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

            // ----- End-of-cycle pause -----
            isMoving = false;
            animator.SetBool("IsMoving", false);
            yield return new WaitForSeconds(cycleDelay);

            // If you want alternating behavior when not random, uncomment:
            // if (!chooseRandomDir) dir *= -1;
        }
    }

    void Update()
    {
        // Followers mimic leader (kept as-is per your request)
        if (isFollower && leader != null)
        {
            isMoving = leader.isMoving;

            // (kept) follower facing derived from leader.moveSpeed sign (you can improve later)
            facingRight = leader.moveSpeed > 0;
            animator.SetBool("FacingRight", facingRight);
            if (sr != null) sr.flipX = !facingRight;

            animator.SetBool("IsMoving", leader.isMoving);

            if (isMoving)
            {
                Vector2 vdir = facingRight ? Vector2.right : Vector2.left;
                transform.Translate(vdir * Mathf.Abs(moveSpeed) * Time.deltaTime);
            }
        }
    }

    // --- Reset to starting state for boss level restarts ---
    Vector3 startPos;

    void Awake()
    {
        startPos = transform.position; // remember spawn position
    }

    public void ResetToStart()
    {
        StopAllCoroutines();
        transform.position = startPos;
        isMoving = false;

        // reset animation to idle
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("FacingRight", true);
        }

        if (sr != null)
            sr.flipX = false;
    }
}
