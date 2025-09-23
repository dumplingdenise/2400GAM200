using UnityEngine;

public class ShadowController : MonoBehaviour
{
    // shadow following the main doll
    [SerializeField] private Transform mainDoll;
    [SerializeField] Vector2 offset = new Vector2(-0.6f, 0f);
    [SerializeField] float followSpeed = 6f; // speed of shadow doll following maindoll
    [SerializeField] float stopDistance = 0.08f;

    private gameController controller;

    private Rigidbody2D shadowrb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = FindAnyObjectByType<gameController>();
        
        shadowrb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (controller == null) return;

        if (controller.currentMode != gameController.GameState.Real) return;

        Vector2 currentPos = shadowrb.position;

        Vector2 targetPos = mainDoll.position;

        targetPos += offset;

        float dist = Vector2.Distance(currentPos, targetPos);

        if (dist <= stopDistance) return; 

        float step = followSpeed * Time.fixedDeltaTime;

        Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, step);

        shadowrb.MovePosition(nextPos);
    }
}
