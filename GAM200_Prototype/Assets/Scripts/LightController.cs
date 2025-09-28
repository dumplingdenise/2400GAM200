using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationStep = 90f; // how much to rotate each press
    private float currentRotation = 0f;

    private Rigidbody2D rb;

    // with movement animation
    /*public float speed = 20f;
    private Vector3 target;*/


    private ShadowSource[] shadowSources;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No rigidbody2d found");
        }
        rb.freezeRotation = true; // stops spinning from collisions

        /*target = transform.position;*/ // for movement animation
    }

    // Update is called once per frame
    void Update()
    {
        HandleLightMovement();
        HandleRotationInput();
    }

    void HandleLightMovement()
    {
        // with movement animation
        /*if (Input.GetMouseButton(0)) // 0 = Left Click, 1 = Right Click
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = transform.position.z;
        }

        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        rb.MovePosition(newPos);*/

        // without movement animation
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // lock to 2D plane
            rb.MovePosition(mousePos);
        }
    }

    void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentRotation += rotationStep;
            if (currentRotation >= 360f) currentRotation -= 360f;

            transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Platforms")
        {
            Debug.Log("Light shining platform");

            // If this platform has a ShadowSource, update its shadow
            ShadowSource src = collision.GetComponent<ShadowSource>();
            if (src != null)
                src.ShowShadow(transform.up);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Platforms"))
        {
            ShadowSource src = collision.GetComponent<ShadowSource>();
            if (src != null)
                src.ShowShadow(transform.up); // keep updating shadow direction
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Platforms")
        {
            Debug.Log("Light not shining on platforms");

            ShadowSource src = collision.GetComponent<ShadowSource>();
            if (src != null)
                src.HideShadow();
        }
    }
}
