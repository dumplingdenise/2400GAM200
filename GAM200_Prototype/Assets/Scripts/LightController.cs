using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using static gameController;

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
    public float shadowLength = 0f;
    public LayerMask shadowableLayer;
    public GameObject shadowPrefab;

    private List<ShadowCaster> activeShadows = new List<ShadowCaster>();


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
        HandleLightToggle();
        HandleLightMovement();
        HandleLightDirection();

        if (lightOn)
        {
            UpdateAllShadows();
        }
    }

    void HandleLightToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lightOn = !lightOn;
            PlayLightToggleSound();
            SetLightActive(lightOn);

            if (!lightOn)
            {
                ClearAllShadows();
            }
        }
    }

    void HandleLightMovement()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 newPos = Vector2.Lerp(transform.position, mouseWorld, Time.deltaTime * moveSpeed);
        lightRB.MovePosition(newPos);
    }

    void HandleLightDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector2 direction = (mouseWorld - transform.position).normalized;

        // Rotate smoothly toward mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
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
        /*if (((1 << col.gameObject.layer) & shadowableLayer) != 0)
        {
            var shadow = new ShadowCaster(poly, Instantiate(shadowPrefab), shadowLength);
            activeShadows.Add(shadow);
        }*/

        // test
        if (poly != null && ((1 << col.gameObject.layer) & shadowableLayer) != 0)
        {
            var shadow = new ShadowCaster(poly, Instantiate(shadowPrefab), shadowLength);

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
        Vector2 lightPos = urpLight ? urpLight.transform.position : transform.position;
        foreach (var s in activeShadows)
        {
            s.UpdateShape(lightPos);
        }
    }

    void ClearAllShadows()
    {
        foreach (var s in activeShadows)
            GameObject.Destroy(s.shadowObj);
        activeShadows.Clear();
    }
}
