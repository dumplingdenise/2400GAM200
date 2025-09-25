using UnityEngine;

public class gameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    /* This script is for handling of the game session.
        Use for game state -> running, paused, gameOver etc.
        Stores & Restore checkpoints (**TBC if script separated or not yet)
        Handles respawning when player die (**TBC if script separated or not yet)
        Control UI Menus (**TBC if script separated or not yet)    
     */
    public enum GameState
    {
        Real,
        Shadow
    }

    [SerializeField] private GameObject mainDoll, shadowDoll;
    public GameState currentMode;

    // switching for player movement control
    [SerializeField] playerController playerMove;
    [SerializeField] ShadowMovement shadowMove;

    //player recall function
    [SerializeField] float recallSpeed = 7f;
    [SerializeField] float arriveThreshold = 0.15f;
    private bool isRecalling;
    private Rigidbody2D playerrb;

    public bool IsRecalling => isRecalling;

    private Rigidbody2D shadowRb;

    void Start()
    {
        // start with main player state
        currentMode = GameState.Real;
        playerrb = mainDoll.GetComponent<Rigidbody2D>();
        shadowRb = shadowDoll.GetComponent<Rigidbody2D>();
        SetControlForMode();
    }

    // Update is called once per frame
    void Update()
    {
        ShadowSwitchMode();
    }

    void ShadowSwitchMode()
    {
        //check if previous mode is shadow to decide for the recall function
        bool wasShadow = (currentMode == GameState.Shadow);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentMode == GameState.Real)
            {
                currentMode = GameState.Shadow;
                Debug.Log("Now controlling: " + currentMode);
                SetControlForMode();
            }
            else
            {
                currentMode = GameState.Real;
                Debug.Log("Now controlling: " + currentMode);
                isRecalling = wasShadow;

                var sv = shadowRb.linearVelocity;
                sv = Vector2.zero;
                shadowRb.linearVelocity = sv;

                // zero out player's x velocity so recall doesnt fight momentum
                var v = playerrb.linearVelocity; 
                v.x = 0f;
                playerrb.linearVelocity = v;
                SetControlForMode();
            }

        }
    }

    void SetControlForMode()
    {
       
        if (currentMode == GameState.Real)
        {
            playerMove.enabled = !isRecalling;
            shadowMove.enabled = false;
        }
        else if (currentMode == GameState.Shadow)
        {
            playerMove.enabled = false;
            shadowMove.enabled = true;
        }
    }

    void FixedUpdate()
    {
        // Only run recall logic while we're auto-walking the player to the shadow.
        if (!isRecalling)
        {
            return;
        }

        // current player position
        Vector2 p = playerrb.position;

        //target is shadow's x, keep player's current Y
        float targetX = shadowDoll.transform.position.x;

        // how far away from the target on the x-asix
        float distX = Mathf.Abs(p.x - targetX);

        // Which direction to move in X? (+1 right, -1 left, 0 if already aligned)
        float dirX = Mathf.Sign(targetX - p.x);

        // Desired horizontal velocity: move at recallSpeed until we're "close enough",
        // then stop (0) so we can snap once and finish recall without jitter.
        float desiredVX = (distX > arriveThreshold) ? dirX * recallSpeed : 0f;

        // Apply ONLY the X velocity; keep whatever Y velocity physics has (gravity, slopes).
        var v = playerrb.linearVelocity;
        v.x = desiredVX;
        playerrb.linearVelocity = v;

        // If within the "close enough" band on X, finish recall cleanly.
        if (distX <= arriveThreshold)
        {
            // Snap exactly to the shadow's X once (prevents tiny drift),
            // but DO NOT touch Y so it doesn't fight gravity/grounding.
            playerrb.position = new Vector2 (targetX, playerrb.position.y);

            // End recall and hand control back to the player.
            isRecalling = false;
            currentMode = GameState.Real;
            SetControlForMode();
        }
    }
}
