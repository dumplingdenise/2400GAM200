using UnityEngine;
using System.Collections.Generic;

public class ShadowCaster
{
    public PolygonCollider2D source;          // the shape casting shadow
    public GameObject shadowObj;              // the shadow prefab instance
    private PolygonCollider2D shadowCollider; // the collider of shadow

    public PolygonCollider2D lightCollider; // testing

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

    // test version 4
    public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        // 1️⃣ Convert collider points to world space
        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // 2️⃣ Identify edges whose outward normal faces away from the light
        List<(Vector2 a, Vector2 b)> silhouetteEdges = new List<(Vector2, Vector2)>();
        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 a = worldPoints[i];
            Vector2 b = worldPoints[(i + 1) % worldPoints.Length];

            Vector2 edge = b - a;
            Vector2 normal = new Vector2(edge.y, -edge.x).normalized;
            Vector2 toLight = (lightPos - ((a + b) / 2f)).normalized;

            // keep only edges whose normal points away from the light
            if (Vector2.Dot(normal, toLight) < 0)
                silhouetteEdges.Add((a, b));
        }

        if (silhouetteEdges.Count == 0)
        {
            shadowCollider.pathCount = 0;
            return;
        }

        // 3️⃣ Gather unique silhouette vertices in order
        List<Vector2> silhouetteVerts = new List<Vector2>();
        foreach (var e in silhouetteEdges)
        {
            if (!silhouetteVerts.Contains(e.a)) silhouetteVerts.Add(e.a);
            if (!silhouetteVerts.Contains(e.b)) silhouetteVerts.Add(e.b);
        }

        // 4️⃣ Sort silhouette vertices by angle around the light
        silhouetteVerts.Sort((p1, p2) =>
        {
            float a1 = Mathf.Atan2(p1.y - lightPos.y, p1.x - lightPos.x);
            float a2 = Mathf.Atan2(p2.y - lightPos.y, p2.x - lightPos.x);
            return a1.CompareTo(a2);
        });

        // 5️⃣ Project those vertices outward (radially from the light)
        Vector2[] projected = new Vector2[silhouetteVerts.Count];
        for (int i = 0; i < silhouetteVerts.Count; i++)
        {
            Vector2 dir = (silhouetteVerts[i] - lightPos).normalized;
            projected[i] = silhouetteVerts[i] + dir * length;

            // visualize projection
            Debug.DrawLine(silhouetteVerts[i], projected[i], Color.yellow, 0.05f);
        }

        // 6️⃣ Combine into one closed polygon: original + reversed projected
        List<Vector2> combined = new List<Vector2>();
        combined.AddRange(silhouetteVerts);
        for (int i = projected.Length - 1; i >= 0; i--)
            combined.Add(projected[i]);

        // 7️⃣ Ensure correct winding order (clockwise)
        if (!IsClockwise(combined))
            combined.Reverse();

        // 8️⃣ Apply polygon to collider
        Vector2[] localCombined = ToLocal(shadowCollider.transform, combined.ToArray());
        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);
    }

    private bool IsClockwise(List<Vector2> pts)
    {
        float sum = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 v1 = pts[i];
            Vector2 v2 = pts[(i + 1) % pts.Count];
            sum += (v2.x - v1.x) * (v2.y + v1.y);
        }
        return sum > 0;
    }

    // Helper: convert world → local points for shadow object
    private Vector2[] ToLocal(Transform t, Vector2[] world)
    {
        Vector2[] local = new Vector2[world.Length];
        for (int i = 0; i < world.Length; i++)
            local[i] = t.InverseTransformPoint(world[i]);
        return local;
    }
    // test for shadow length base on light distance
    public void SetLength(float newLength)
    {
        length = newLength;
    }
}