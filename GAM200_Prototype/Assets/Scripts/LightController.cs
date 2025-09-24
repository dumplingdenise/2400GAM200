using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationStep = 90f; // how much to rotate each press
    private float currentRotation = 0f;

    private Rigidbody2D rb;

    // test shadow
    public Vector2 currentDirection = Vector2.down;
    private ShadowSource[] shadowSources;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*if (light2D == null)
        {
            Debug.LogWarning("No Light2D component found. Add one to this GameObject.");
        }*/

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No rigidbody2d found");
        }
        rb.freezeRotation = true; // stops spinning from collisions

        //test shadow
        shadowSources = FindObjectsOfType<ShadowSource>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLightMovement();
        HandleRotationInput();
    }

    void HandleLightMovement()
    {
        // Only move light when left mouse button is held down
        if (Input.GetMouseButton(0)) // 0 = Left Click, 1 = Right Click
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // lock to 2D plane
            rb.MovePosition(mousePos);
        }
    }

    void HandleRotationInput()
    {
        // Example: rotate 90° clockwise each time you press Space
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentRotation += rotationStep;
            if (currentRotation >= 360f) currentRotation -= 360f;

            transform.rotation = Quaternion.Euler(0, 0, currentRotation);

            // test shadow
            // Update currentDirection based on rotation
            currentDirection = AngleToDirection(currentRotation);

            // Tell all shadows to update
            UpdateShadows();
        }
    }

    // test shadow
    // Convert rotation angle to direction vector
    Vector2 AngleToDirection(float angle)
    {
        angle = Mathf.Round(angle) % 360;

        if (Mathf.Approximately(angle, 0f)) return Vector2.up;
        if (Mathf.Approximately(angle, 90f)) return Vector2.left;
        if (Mathf.Approximately(angle, 180f)) return Vector2.down;
        if (Mathf.Approximately(angle, 270f) || Mathf.Approximately(angle, -90f)) return Vector2.right;

        return Vector2.down; // fallback
    }

    void UpdateShadows()
    {
        foreach (var src in shadowSources)
        {
            src.UpdateShadow(currentDirection);
        }
    }
}
