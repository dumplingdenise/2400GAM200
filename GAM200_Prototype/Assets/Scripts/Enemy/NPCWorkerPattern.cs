using UnityEngine;

public class NPCWorkerPattern : MonoBehaviour
{
    [SerializeField] float idleDuration = 1.5f; // seconds to stay idle
    [SerializeField] float moveDuration = 1.5f; // seconds to walk
    [SerializeField] float cycleDelay = 0.5f;  // small delay before repeating
    [SerializeField] float moveSpeed = 2f;  // how fast to walk
    private Animator animator; // reference for state changes
    
    [HideInInspector] public bool isMoving; // tells other if NPC is walking 
    private bool facingRight = true; //default start direction

    [SerializeField] bool isFollower = false;  //check this in Inspector for followers
    [SerializeField] NPCWorkerPattern leader;  // assign the boss here for followers

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        if (!isFollower)
        {
            StartCoroutine(PatternLoop()); // boss runs its own rhythm
        }

        bool goingRight = moveSpeed > 0;
        animator.SetBool("FacingRight", goingRight);

        // one-time sync at the beginning
        if (Time.timeSinceLevelLoad < 0.1f)
        {
            bool leaderRight = leader.moveSpeed > 0;
            animator.SetBool("FacingRight", leaderRight);
        }

    }

    System.Collections.IEnumerator PatternLoop()
    {
        while (true)
        {
            //Idle phase
            isMoving = false;
            animator.SetBool("IsMoving", false);
            yield return new WaitForSeconds(idleDuration);

            //Move phase
            isMoving = true;
            animator.SetBool("IsMoving", true);

            float moveTimer = moveDuration;
            while(moveTimer > 0f)
            {
                moveTimer -= Time.deltaTime;
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
                yield return null;
            }

            // small delay before repeating
            animator.SetBool("IsMoving", false);
            isMoving = false;
            yield return new WaitForSeconds(cycleDelay);

            // reverse diretion for next cycle
            moveSpeed = -moveSpeed;

            //update animator with current facing
            bool goingRight = moveSpeed > 0;
            animator.SetBool("FacingRight", goingRight);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (isFollower && leader != null)
        {
            // Copy the leader's movement and animation state
            isMoving = leader.isMoving;

            //copy facing direction of the boss
            facingRight = leader.moveSpeed > 0;
            animator.SetBool("FacingRight", facingRight);
            animator.SetBool("IsMoving", leader.isMoving);

            // Move in same direction as boss
            if (isMoving)
            {
                Vector2 dir = facingRight ? Vector2.right : Vector2.left;
                transform.Translate(dir * Mathf.Abs(moveSpeed) * Time.deltaTime);
            }
        }
    }
}
