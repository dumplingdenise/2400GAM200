using UnityEngine;

/// <summary>
/// Controls post-snap shadow collider behavior (walk-on, walk-into, diagonal, and same-level fixes)
/// Works with MeshRenderer-based shadows. Attach to your shadow prefab.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
[DisallowMultipleComponent]
public class ShadowColliderController : MonoBehaviour
{
    private PolygonCollider2D poly;
    private PlatformEffector2D effector;
    private Rigidbody2D shadowPlayerRb;

    private Vector2 currentLightDir;

    [Header("Effector Settings")]
    [Range(90f, 180f)] public float surfaceArc = 160f;
    [Tooltip("How flat the light direction must be before counting as horizontal")]
    public float sameLevelThreshold = 0.2f;
    [Tooltip("Minimum downward velocity required to stand on a flat shadow")]
    public float downwardEntryVelocity = -0.1f;
    [Tooltip("Small Y offset to prevent side collisions")]
    public float colliderYOffset = 0.05f;

    void Awake()
    {
        poly = GetComponent<PolygonCollider2D>();

        // Ensure PlatformEffector2D exists
        effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            effector = gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWayGrouping = true;
            effector.surfaceArc = surfaceArc;
        }

        // Find shadow player by layer instead of tag
        int shadowPlayerLayer = LayerMask.NameToLayer("ShadowPlayer");
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            if (rb.gameObject.layer == shadowPlayerLayer)
            {
                shadowPlayerRb = rb;
                break;
            }
        }
    }

    /// <summary>
    /// Called from LightController after the light is snapped/static.
    /// </summary>
    public void UpdateColliderState(Vector2 lightDir)
    {
        currentLightDir = lightDir.normalized;

        // 1️⃣ Rotate effector so its solid face opposes the light
        float angle = Mathf.Atan2(currentLightDir.y, currentLightDir.x) * Mathf.Rad2Deg;
        effector.rotationalOffset = angle + 180f;

        // 2️⃣ Decide Walk-On vs Walk-Into
        bool isWalkInto = currentLightDir.y > 0.5f;
        effector.enabled = !isWalkInto;

        // 3️⃣ Same-level fix
        if (!isWalkInto && Mathf.Abs(currentLightDir.y) < sameLevelThreshold && shadowPlayerRb != null)
        {
            if (shadowPlayerRb.linearVelocity.y > downwardEntryVelocity)
                effector.enabled = false;
        }

        // 4️⃣ Diagonal dot check (ramps)
        if (shadowPlayerRb != null)
        {
            Vector2 playerDir = shadowPlayerRb.linearVelocity.normalized;
            Vector2 shadowNormal = -currentLightDir;
            float dot = Vector2.Dot(playerDir, shadowNormal);

            // Player moving from behind or below → disable collision
            if (dot > -0.5f)
                effector.enabled = false;
        }

        // 5️⃣ Apply slight collider offset
        Vector2 offset = poly.offset;
        offset.y = Mathf.Abs(currentLightDir.y) < sameLevelThreshold ? -colliderYOffset : 0f;
        poly.offset = offset;
    }
}
