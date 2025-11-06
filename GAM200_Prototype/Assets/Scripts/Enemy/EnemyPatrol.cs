using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] Transform leftPoint;
    [SerializeField] Transform rightPoint;
    private float speed = 2f;
    private float arriveThreshold = 0.06f;
    private float pauseAtEnds = 2f;
    private float pauseTimer;

    private Rigidbody2D rb;
    private gameController gc;

    private Transform currentTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gc = FindAnyObjectByType<gameController>();
        currentTarget = rightPoint;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //if currently pausing, count down and skip movement
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }
        else
        {
            rb.MovePosition(rb.position);
        }

        //patrol movement
        Vector2 current = rb.position;
        Vector2 target = currentTarget.position;

        float step = speed * Time.fixedDeltaTime;
        Vector2 next = Vector2.MoveTowards(current, target, step);
        rb.MovePosition(next);

        //check if reached the end of patrol path
        if (Vector2.Distance(current, target) <= arriveThreshold)
        {
            pauseTimer = pauseAtEnds; // start waiting at the end

            // swap direction
            if (currentTarget == rightPoint)
            {
                currentTarget = leftPoint;
            }
            else
            {
                currentTarget = rightPoint;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            gc.Respawn();
        }
    }
}
