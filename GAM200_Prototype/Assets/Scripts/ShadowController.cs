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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = FindAnyObjectByType<gameController>();
        
        shadowrb = GetComponent<Rigidbody2D>();

        // Small polish: Interpolate on the RB for smoother visuals when moved by MovePosition
        if (shadowrb) shadowrb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (controller == null) return;

        if (controller.IsRecalling) return;

        // Only follow when the game is in Real mode (player controls main doll).
        if (controller.currentMode != gameController.GameState.Real) return;

        // Current shadow position (RB space).
        Vector2 currentPos = shadowrb.position;

        // Target = main doll position + desired offset.
        Vector2 targetPos = mainDoll.position;
        targetPos += offset;

        // If close enough, don’t move (avoids buzzing at the end).
        float dist = Vector2.Distance(currentPos, targetPos);
        if (dist <= stopDistance) return; 

        float step = followSpeed * Time.fixedDeltaTime;

        Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, step);

        shadowrb.MovePosition(nextPos);

    }
}
