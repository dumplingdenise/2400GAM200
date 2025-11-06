using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using static GameController;

public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    public bool lightOn = false;
    public float followSpeed = 5f; // speed of light direction rotation following the cursor
    public float moveSpeed = 5f; // speed of light movement following cursor

    private Light2D urpLight;
    private Rigidbody2D lightRB;
    private PolygonCollider2D collider;

    public AudioSource lightAudioSource;
    public AudioClip lightAudioClip;

    [Header("Shadow Settings")]
    public LayerMask shadowableLayer;
    public GameObject shadowPrefab;

    [Header("Dynamic Shadow Length")]
    public float baseShadowLength = 5f;
    public float maxShadowLength = 10f;
    public float minShadowLength = 1f;

    private List<ShadowCaster> activeShadows = new List<ShadowCaster>();

    private bool isSnapped = false;

    private bool isRotating = false;
    private bool isFrozen = false;        // NEW: stays true after releasing RMB
    private float lastRightClickTime = 0; // NEW: used to detect double click
    private float doubleClickThreshold = 0.3f;

    private Vector3 lastMousePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        lightRB = GetComponent<Rigidbody2D>();
        if (lightRB == null)
        {
            Debug.LogError("no RigidBody2D found on Light");
        }
        lightRB.freezeRotation = true;

        urpLight = GetComponentInChildren<Light2D>();
        if (urpLight == null)
        {
            Debug.LogError("no Light2D found");
        }

        collider = GetComponentInChildren<PolygonCollider2D>();
        if (collider == null)
        {
            Debug.LogError("collider not found");
        }

        SetLightActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.IsPaused) return;

        HandleLightToggle();
        /*HandleSnapToggle();*/ // right click to snap light in-place to prevent movement from cursor
        HandleRotationMode();
        HandleLightMovement();
        HandleLightDirection();

        if (lightOn)
        {
            UpdateAllShadows();

            // test for light angle -> shadow logic
            if (isSnapped)
                UpdateShadowSolidityByAngle();
        }
    }

    void HandleRotationMode()
    {
        // --- Enter rotation mode when holding right mouse ---
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        // --- While rotating, update light angle based on mouse movement ---
        if (isRotating)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = currentMousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
        }
    }

    void HandleLightMovement()
    {
        if (isRotating) return; // don’t move when rotating

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        lightRB.MovePosition(mouseWorld);
    }


    void HandleLightDirection()
    {
        // When rotating, rotation handled by HandleRotationMode()
        if (isRotating) return;
    }

    void HandleLightToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lightOn = !lightOn;
            PlayLightToggleSound();
            SetLightActive(lightOn);

            SetShadowsGhostMode(true); // for ghost shadow

            if (!lightOn)
            {
                ClearAllShadows();
            }
        }
    }

    void SetLightActive(bool state)
    {
        if (urpLight != null)
        {
            urpLight.enabled = state;
        }
    }

    private void PlayLightToggleSound()
    {
        if (lightAudioSource != null && lightAudioClip != null)
            lightAudioSource.PlayOneShot(lightAudioClip);
    }

    // shadow system
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!lightOn)
        {
            return;
        }

        PolygonCollider2D poly = col as PolygonCollider2D;

        // test
        if (poly != null && ((1 << col.gameObject.layer) & shadowableLayer) != 0)
        {
            var shadow = new ShadowCaster(poly, Instantiate(shadowPrefab), baseShadowLength);

            // ✅ NEW LINE: assign the light’s cone collider to this shadow
            shadow.lightCollider = collider;

            activeShadows.Add(shadow);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        activeShadows.RemoveAll(s =>
        {
            if (s.source == col)
            {
                Destroy(s.shadowObj);
                return true;
            }
            return false;
        });
    }

    void UpdateAllShadows()
    {
        // test for dynamic shadow length base on light distance
        Vector2 lightPos = urpLight ? urpLight.transform.position : transform.position;

        foreach (var s in activeShadows)
        {
            if (s.source == null) continue;

            // distance from light to the platform casting this shadow
            float dist = Vector2.Distance(lightPos, s.source.transform.position);

            // closer light = longer shadow, further = shorter
            float dynamicLength = Mathf.Clamp(baseShadowLength * (1.5f / (dist * 0.25f)), minShadowLength, maxShadowLength);

            s.SetLength(dynamicLength);   // new helper we'll add next
            s.UpdateShape(lightPos);
        }
    }

    void ClearAllShadows()
    {
        foreach (var s in activeShadows)
            GameObject.Destroy(s.shadowObj);
        activeShadows.Clear();
    }

    // testing for ghost shadow when light is rotating and moving while on
    void SetShadowsGhostMode(bool ghost)
    {
        foreach (var s in activeShadows)
        {
            var col = s.shadowObj.GetComponent<PolygonCollider2D>();
            if (col != null)
            {
                // ghost → trigger; solid → normal collider
                col.isTrigger = ghost;
            }
        }
    }

    // testing for light angle for different shadow behavior (inside or on top)
    void UpdateShadowSolidityByAngle()
    {
        if (!lightOn || !isSnapped) return;

        // Measure how far the light is from pointing straight up
        float angle = Mathf.Abs(Vector2.Angle(Vector2.up, transform.right));

        // 0° = pointing up, 90° = sideways, 180° = down
        bool makeSolid = (angle < 60f || angle > 120f);   // mostly up or down → solid
                                                          // between 60°–120° (sideways) → pass-through

        // --- Debug Visual ---
        // Green = solid (walkable)
        // Cyan = pass-through (walk inside)
        Color debugColor = makeSolid ? Color.green : Color.cyan;
        Debug.DrawRay(transform.position, transform.right * 2f, debugColor);

        foreach (var s in activeShadows)
        {
            var col = s.shadowObj.GetComponent<PolygonCollider2D>();
            if (col != null)
                col.isTrigger = !makeSolid;  // solid=false, pass-through=true
        }
    }
}


