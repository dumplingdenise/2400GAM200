using UnityEngine;

public class ShadowController : MonoBehaviour
{
   

    // shadow following the main doll
    [SerializeField] private Transform mainDoll;
    [SerializeField] Vector2 offset = new Vector2(-0.6f, 0f); // base offset from player
    [SerializeField] float followSpeed = 6f; // speed of shadow doll following maindoll
    [SerializeField] float stopDistance = 0.16f; // close enough radius to stop

    private gameController controller;

    private Rigidbody2D shadowrb;
    private Rigidbody2D playerRb;

    private float originalGravity;

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
        if (controller == null) return;

        shadowrb.gravityScale = originalGravity;

        if (controller.IsRecalling) 
        {
            return; 
        }

        // Only follow when the game is in Real mode (player controls main doll).
        if (controller.currentMode != gameController.GameState.Real) return;

        Vector2 currentPos = shadowrb.position;

        //shadowrb.gravityScale = 0;

        float targetX = playerRb.position.x + offset.x;

        float dx = Mathf.Abs(currentPos.x - targetX);
        if (dx <= stopDistance)
        {
            Vector2 pos = shadowrb.position;
            pos.x = targetX;
            shadowrb.position = pos;
            return;
        }

        shadowrb.MovePosition(new Vector2(targetX, currentPos.y));

    }
}
