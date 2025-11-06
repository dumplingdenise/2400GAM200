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
    public float rotationSpeed = 1000f; // speed of scroll wheel rotation (degrees per second)
    public float moveSpeed = 5f;       // how fast light follows cursor

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

    private bool isFrozen = false; // if true → light stops following cursor

    void Start()
    {
        lightRB = GetComponent<Rigidbody2D>();
        if (lightRB == null)
            Debug.LogError("No RigidBody2D found on Light");
        lightRB.freezeRotation = true;

        urpLight = GetComponentInChildren<Light2D>();
        if (urpLight == null)
            Debug.LogError("No Light2D found");

        collider = GetComponentInChildren<PolygonCollider2D>();
        if (collider == null)
            Debug.LogError("Collider not found");

        SetLightActive(false);
    }

    void Update()
    {
        if (GameController.IsPaused) return;

        HandleLightToggle();
        HandleFreezeToggle();
        HandleRotationScroll();
        HandleLightMovement();

        if (lightOn)
        {
            UpdateAllShadows();

            SetShadowsGhostMode(!isFrozen);
        }  
    }

    // -------------------------------
    // Left click → Toggle light ON/OFF
    // -------------------------------
    void HandleLightToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lightOn = !lightOn;
            PlayLightToggleSound();
            SetLightActive(lightOn);
            SetShadowsGhostMode(true);

            if (!lightOn)
                ClearAllShadows();
        }
    }

    // -------------------------------
    // Right click → Freeze/unfreeze
    // -------------------------------
    void HandleFreezeToggle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // Toggle frozen state
            isFrozen = !isFrozen;
        }
    }

    // -------------------------------
    // Scroll wheel → Rotate while frozen
    // -------------------------------
    void HandleRotationScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Scroll up = positive, scroll down = negative
            float angleChange = scroll * rotationSpeed * Time.deltaTime;

            transform.Rotate(0, 0, -angleChange); // negative to make scroll up = anti-clockwise
        }
    }

    // -------------------------------
    // Move light (follow cursor)
    // -------------------------------
    void HandleLightMovement()
    {
        if (isFrozen) return; // don’t move when frozen

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // instant or smooth follow (choose)
        lightRB.MovePosition(mouseWorld);
    }

    // -------------------------------
    // Utilities
    // -------------------------------
    void SetLightActive(bool state)
    {
        if (urpLight != null)
            urpLight.enabled = state;
    }

    void PlayLightToggleSound()
    {
        if (lightAudioSource != null && lightAudioClip != null)
            lightAudioSource.PlayOneShot(lightAudioClip);
    }

    // -------------------------------
    // Shadow system
    // -------------------------------
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!lightOn) return;

        PolygonCollider2D poly = col as PolygonCollider2D;
        if (poly != null && ((1 << col.gameObject.layer) & shadowableLayer) != 0)
        {
            var shadow = new ShadowCaster(poly, Instantiate(shadowPrefab), baseShadowLength);
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
        Vector2 lightPos = urpLight ? urpLight.transform.position : transform.position;

        foreach (var s in activeShadows)
        {
            if (s.source == null) continue;

            float dist = Vector2.Distance(lightPos, s.source.transform.position);
            float dynamicLength = Mathf.Clamp(baseShadowLength * (1.5f / (dist * 0.25f)), minShadowLength, maxShadowLength);

            s.SetLength(dynamicLength);
            s.UpdateShape(lightPos);
        }
    }

    void ClearAllShadows()
    {
        foreach (var s in activeShadows)
            GameObject.Destroy(s.shadowObj);
        activeShadows.Clear();
    }

    void SetShadowsGhostMode(bool ghost)
    {
        foreach (var s in activeShadows)
        {
            var col = s.shadowObj.GetComponent<PolygonCollider2D>();
            if (col != null)
                col.isTrigger = ghost;
        }
    }
}
