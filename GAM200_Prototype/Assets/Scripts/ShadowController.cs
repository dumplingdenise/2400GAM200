using UnityEngine;

public class ShadowController : MonoBehaviour
{
   

    // shadow following the main doll
    [SerializeField] private Transform mainDoll;
    [SerializeField] Vector2 offset = new Vector2(-0.6f, 0f); // base offset from player
    [SerializeField] float followSpeed = 6f; // speed of shadow doll following maindoll
    [SerializeField] float stopDistance = 0.12f; // close enough radius to stop
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] Vector2 groundCheckOffset = new Vector2(0f, -0.1f);
    
    [SerializeField] float xDamp = 0.10f;
    [SerializeField] float xMaxSpeed = 12f;
    

    private float xVelRef;

    private gameController controller;

    private Rigidbody2D shadowrb;
    private Rigidbody2D playerRb;

    private float originalGravity;

    int followCooldownFrames;
    public bool needInitialAlign;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = FindAnyObjectByType<gameController>();
        
        shadowrb = GetComponent<Rigidbody2D>();

        playerRb = mainDoll.GetComponent<Rigidbody2D>();

        // Small polish: Interpolate on the RB for smoother visuals when moved by MovePosition
        if (shadowrb) shadowrb.interpolation = RigidbodyInterpolation2D.Interpolate;

        originalGravity = shadowrb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (followCooldownFrames > 0)
        {
            followCooldownFrames--;
            return;
        }

        if (needInitialAlign)
        {
            // put the shadow exactly where the player is + offset
            Vector2 target = playerRb.position + offset;
            shadowrb.position = target;

            xVelRef = 0f;

            needInitialAlign = false; // reset the flag
            return; // skip the rest of follow logic this frame
        }

        if (controller == null) return;

        // Always start with original gravity
        shadowrb.gravityScale = originalGravity;

        if (controller.currentMode != gameController.WorldState.Real) return;
        if (playerRb != null && playerRb.simulated == false) return;

        Vector2 currentPos = shadowrb.position;
        RaycastHit2D hit = Physics2D.Raycast(playerRb.position + groundCheckOffset, Vector2.down, groundCheckDistance, groundMask);
        bool playerGrounded = (hit.collider != null);

        float targetX = playerRb.position.x + offset.x;
        float dx = Mathf.Abs(currentPos.x - targetX);

        if (playerGrounded == false)
        {
            // When player is airborne, shadow should float without gravity
            shadowrb.gravityScale = 0f;
            float targetY = playerRb.position.y + offset.y;

            if (dx <= stopDistance && Mathf.Abs(currentPos.y - targetY) <= stopDistance)
            {
                // shadowrb.position = new Vector2(targetX, targetY);

                float newX = Mathf.SmoothDamp(currentPos.x, targetX, ref xVelRef, xDamp, xMaxSpeed, Time.fixedDeltaTime);
                float newY = Mathf.Lerp(currentPos.y, targetY, 0.5f);
                shadowrb.MovePosition(new Vector2(newX, newY));
            }
            else
            {
                // float newY = Mathf.MoveTowards(currentPos.y, targetY, followSpeed * Time.fixedDeltaTime);
                //shadowrb.MovePosition(new Vector2(targetX, newY));

                float newX = Mathf.SmoothDamp(currentPos.x, targetX, ref xVelRef, xDamp, xMaxSpeed, Time.fixedDeltaTime);
                float newY = Mathf.MoveTowards(currentPos.y, targetY, followSpeed * Time.fixedDeltaTime);
                shadowrb.MovePosition(new Vector2(newX, newY));
            }
        }
        else
        {
            // When player is grounded, shadow should have normal gravity
            shadowrb.gravityScale = originalGravity;

            float newX = Mathf.SmoothDamp(currentPos.x, targetX, ref xVelRef, xDamp, xMaxSpeed, Time.fixedDeltaTime);
            shadowrb.MovePosition(new Vector2(newX, currentPos.y));

            /* if (dx <= stopDistance)
             {
                 Vector2 pos = shadowrb.position;
                 pos.x = targetX;
                 shadowrb.position = pos;
             }
             else
             {
                 float newX = Mathf.MoveTowards(currentPos.x, targetX, followSpeed * Time.fixedDeltaTime);
                 shadowrb.MovePosition(new Vector2(newX, currentPos.y));
             } */
        }
    }

    public void ArmFollowCooldown (int frames)
    {
        followCooldownFrames = frames;

        xVelRef = 0f; // clear velocity ref so next follow starts clean
    }
}


/*
 *  void FixedUpdate()
    {
        if (controller == null) return;

        if (controller.currentMode != gameController.GameState.Real) return;

        if (playerRb != null && playerRb.simulated == false) return;

        shadowrb.gravityScale = originalGravity;

        // Only follow when the game is in Real mode (player controls main doll).
        if (controller.currentMode != gameController.GameState.Real) return;

        Vector2 currentPos = shadowrb.position;

        RaycastHit2D hit = Physics2D.Raycast(playerRb.position + groundCheckOffset, Vector2.down, groundCheckDistance, groundMask);

        bool playerGrounded = (hit.collider != null);

        //shadowrb.gravityScale = 0;

        float targetX = playerRb.position.x + offset.x;

        float dx = Mathf.Abs(currentPos.x - targetX);

        //shadowrb.MovePosition(new Vector2(targetX, currentPos.y));

        if (playerGrounded == false)
        {
            //when player is airborne, shadow should float without gravity
            shadowrb.gravityScale = 0f;
            float targetY = playerRb.position.y + offset.y;

            if (dx <= stopDistance && Mathf.Abs(currentPos.y - targetY) <= stopDistance)
            {
                shadowrb.position = new Vector2(targetX, targetY);
               // return;
            }
            else
            {
               float newY = Mathf.MoveTowards(currentPos.y, targetY, followSpeed * Time.fixedDeltaTime);

                shadowrb.MovePosition(new Vector2(targetX, newY));
            }
        }
        else
        {
            //when player is grounded, shadow should have normal gravity
            shadowrb.gravityScale = originalGravity;

            if ( dx <= stopDistance)
            {
                Vector2 pos = shadowrb.position;
                pos.x = targetX;
                shadowrb.position = pos;
                //return;
            }
            else
            {
                float newX = Mathf.MoveTowards(currentPos.x, targetX, followSpeed * Time.fixedDeltaTime);
                shadowrb.MovePosition(new Vector2(newX, currentPos.y));
            }
        }  */

