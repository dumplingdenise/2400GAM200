using UnityEngine;
using System.Collections.Generic;

public class ShadowCaster
{
    public PolygonCollider2D source;          // the shape casting shadow
    public GameObject shadowObj;              // the shadow prefab instance
    private PolygonCollider2D shadowCollider; // the collider of shadow
    private float length;                     // how far shadow extends

    public ShadowCaster(PolygonCollider2D src, GameObject obj, float len)
    {
        source = src;
        shadowObj = obj;
        shadowCollider = obj.GetComponent<PolygonCollider2D>();
        length = len;

        shadowObj.transform.SetParent(null);
        shadowObj.name = $"Shadow_{src.name}";
    }

    public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        // Get collider vertices (local → world space)
        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // Determine flat 2D projection direction
        Vector2 lightDir = ((Vector2)source.transform.position - lightPos).normalized;

        // Projected (extruded) points in same direction
        Vector2[] projectedPoints = new Vector2[worldPoints.Length];
        for (int i = 0; i < worldPoints.Length; i++)
        {
            projectedPoints[i] = worldPoints[i] + lightDir * length;

            // Debug: show connection from source → shadow
            Debug.DrawLine(worldPoints[i], projectedPoints[i], Color.yellow, 0.05f);
        }

        // Combine original + projected vertices to form solid extrusion
        List<Vector2> combined = new List<Vector2>();
        combined.AddRange(worldPoints);
        for (int i = projectedPoints.Length - 1; i >= 0; i--)
            combined.Add(projectedPoints[i]);

        // Convert world-space points → local-space points for shadow object
        Vector2[] localCombined = ToLocal(shadowCollider.transform, combined.ToArray());

        // Apply to the shadow collider
        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);

        /*// Keep shadow slightly above background (for render sorting)
        shadowCollider.transform.position = new Vector3(
            source.transform.position.x,
            source.transform.position.y,
            0.01f
        );*/
    }

    // Helper: convert world → local points for shadow object
    private Vector2[] ToLocal(Transform t, Vector2[] world)
    {
        Vector2[] local = new Vector2[world.Length];
        for (int i = 0; i < world.Length; i++)
            local[i] = t.InverseTransformPoint(world[i]);
        return local;
    }
}