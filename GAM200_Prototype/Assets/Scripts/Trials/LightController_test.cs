using UnityEngine;
using UnityEngine.Rendering.Universal;
using static gameController;

public class LightController_Test : MonoBehaviour
{
    [Header("Rotation Settings")]
    /*public float rotationStep = 90f; // how much to rotate each press
    private float currentRotation = 0f;*/
    public bool lightOn = true;
    public float followSpeed = 10f;
    public float moveSpeed = 10f;


    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Light2D light;
    private PolygonCollider2D collider;

    private Vector2 currentDirection; // new

    /*private bool isDragging = false;*/

    private ShadowSource[] shadowSources;

    public AudioSource audioSource;
    public AudioClip clickSound;
    // public AudioClip lightDragSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No rigidbody2d found");
        }
        rb.freezeRotation = true; // stops spinning from collisions


        // sprite renderer
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("No sprite renderer found");
        }

        // light 2D
        light = GetComponentInChildren<Light2D>();
        if (light == null)
        {
            Debug.LogError("Light2D not found");
        }

        collider = GetComponentInChildren<PolygonCollider2D>();
        if (collider == null)
        {
            Debug.LogError("collider not found");
        }

        // start ON
        /*SetLightActive(true);*/

    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.Instance == null) return;
        if (gameController.Instance.currentGameState == GameState.Paused)
        {
            return;
        }
        /*else
        {
            HandleLightMovement();
            HandleRotationInput();
        }*/
        HandleLightToggle();
        MoveToCursor();

        if (lightOn)
        {
            UpdateLightDirection();
        }
    }

/*    void HandleLightMovement()
    {
        if (Input.GetMouseButtonDown(0)) // start dragging
        {
           // PlayDragSound();
            isDragging = true;
            SetLightActive(false); // turn OFF light while dragging
        }
        else if (Input.GetMouseButtonUp(0)) // release drag
        {
            isDragging = false;
            SetLightActive(true); // turn ON light again
        }

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            rb.MovePosition(mousePos);
        }
    }

    void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayClickSound();
            currentRotation += rotationStep;
            if (currentRotation >= 360f) currentRotation -= 360f;

            transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        }
    }*/

    void HandleLightToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lightOn = !lightOn;
            PlayClickSound();
            SetLightActive(lightOn);
        }
    }
    void MoveToCursor()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 newPos = Vector2.Lerp(transform.position, mouseWorld, Time.deltaTime * moveSpeed);
        rb.MovePosition(newPos);
    }

    void UpdateLightDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector2 direction = (mouseWorld - transform.position).normalized;

        // Rotate smoothly toward mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
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
        if (!lightOn) return;
        if (collision.CompareTag("Platforms"))
        {
            ShadowSource src = collision.GetComponent<ShadowSource>();
            if (src != null)
                /*src.ShowShadow(transform.up); // keep updating shadow direction*/
                src.ShowShadow(currentDirection);
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

    void SetLightActive(bool state)
    {
        if (light != null) light.enabled = state;
        if (collider != null) collider.enabled = state;
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
   /* private void PlayDragSound()
    {
        if (audioSource != null && lightDragSound != null)
        {
            audioSource.PlayOneShot(lightDragSound);
        }
    } */
}
