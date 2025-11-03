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

        Vector2[] projectedPoints = new Vector2[worldPoints.Length];
        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 dir = (worldPoints[i] - lightPos).normalized; // direction from light to this vertex
            projectedPoints[i] = worldPoints[i] + dir * length;   // extend along that direction

            Debug.DrawLine(worldPoints[i], projectedPoints[i], Color.yellow, 0.05f);
        }

        /*        // Determine flat 2D projection direction
                Vector2 lightDir = ((Vector2)source.transform.position - lightPos).normalized;

                // Projected (extruded) points in same direction
                Vector2[] projectedPoints = new Vector2[worldPoints.Length];
                for (int i = 0; i < worldPoints.Length; i++)
                {
                    projectedPoints[i] = worldPoints[i] + lightDir * length;

                    // Debug: show connection from source → shadow
                    Debug.DrawLine(worldPoints[i], projectedPoints[i], Color.yellow, 0.05f);
                }*/

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

    /*public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        // 1️⃣ Get collider vertices in world space
        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // 2️⃣ Determine light direction for each vertex
        Vector2 objectCenter = source.bounds.center;
        Vector2 lightDir = ((Vector2)objectCenter - lightPos).normalized;

        // 3️⃣ Filter out back-facing vertices (so we don’t cast behind the light)
        List<Vector2> visibleVerts = new List<Vector2>();
        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 edgeDir = (worldPoints[i] - objectCenter).normalized;
            float facing = Vector2.Dot(edgeDir, lightDir);
            if (facing < 0.3f) // tweakable threshold
                visibleVerts.Add(worldPoints[i]);
        }

        if (visibleVerts.Count < 3)
            return;

        // 4️⃣ Project these vertices outward from the light
        Vector2[] projectedPoints = new Vector2[visibleVerts.Count];
        for (int i = 0; i < visibleVerts.Count; i++)
        {
            Vector2 dir = (visibleVerts[i] - lightPos).normalized;
            projectedPoints[i] = visibleVerts[i] + dir * length;
            Debug.DrawLine(visibleVerts[i], projectedPoints[i], Color.yellow, 0.05f);
        }

        // 5️⃣ Build combined shape (original + projected)
        List<Vector2> combined = new List<Vector2>();
        combined.AddRange(visibleVerts);
        for (int i = projectedPoints.Length - 1; i >= 0; i--)
            combined.Add(projectedPoints[i]);

        // 6️⃣ Convert world → local for shadow prefab
        Vector2[] localCombined = ToLocal(shadowCollider.transform, combined.ToArray());

        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);
    }*/

    // Helper: convert world → local points for shadow object
    private Vector2[] ToLocal(Transform t, Vector2[] world)
    {
        Vector2[] local = new Vector2[world.Length];
        for (int i = 0; i < world.Length; i++)
            local[i] = t.InverseTransformPoint(world[i]);
        return local;
    }
}