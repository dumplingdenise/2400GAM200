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

    /*public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        // 1️⃣ Get collider vertices (local → world space)
        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // 2️⃣ Collect silhouette edges (facing away from light)
        List<(Vector2 a, Vector2 b)> silhouetteEdges = new List<(Vector2, Vector2)>();
        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 a = worldPoints[i];
            Vector2 b = worldPoints[(i + 1) % worldPoints.Length];

            Vector2 edge = b - a;
            Vector2 normal = new Vector2(edge.y, -edge.x).normalized; // outward normal (right-hand)
            Vector2 toLight = (lightPos - ((a + b) / 2f)).normalized;

            // if edge faces *away* from the light, include it
            if (Vector2.Dot(normal, toLight) < 0)
                silhouetteEdges.Add((a, b));
        }

        if (silhouetteEdges.Count == 0)
        {
            shadowCollider.pathCount = 0;
            return;
        }

        // 3️⃣ Build shadow polygons for each silhouette edge
        List<Vector2> finalVerts = new List<Vector2>();

        foreach (var edge in silhouetteEdges)
        {
            Vector2 a = edge.a;
            Vector2 b = edge.b;

            // project both points outward
            Vector2 aProj = a + (a - lightPos).normalized * length;
            Vector2 bProj = b + (b - lightPos).normalized * length;

            // Debug visualize projection
            Debug.DrawLine(a, aProj, Color.yellow, 0.05f);
            Debug.DrawLine(b, bProj, Color.yellow, 0.05f);

            // build a quad (a → b → bProj → aProj)
            finalVerts.Add(a);
            finalVerts.Add(b);
            finalVerts.Add(bProj);
            finalVerts.Add(aProj);
        }

        // 4️⃣ Convert to local space
        Vector2[] localCombined = ToLocal(shadowCollider.transform, finalVerts.ToArray());

        // 5️⃣ Apply to shadow collider
        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);
    }*/

    // test version 2
    /*public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        // 1️⃣ Get collider vertices (local → world space)
        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // 2️⃣ Identify silhouette edges (those facing away from light)
        List<Vector2> silhouetteVerts = new List<Vector2>();
        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 a = worldPoints[i];
            Vector2 b = worldPoints[(i + 1) % worldPoints.Length];

            Vector2 edge = b - a;
            Vector2 normal = new Vector2(edge.y, -edge.x).normalized;
            Vector2 toLight = (lightPos - ((a + b) / 2f)).normalized;

            // keep edges facing away from the light
            if (Vector2.Dot(normal, toLight) < 0)
            {
                if (!silhouetteVerts.Contains(a)) silhouetteVerts.Add(a);
                if (!silhouetteVerts.Contains(b)) silhouetteVerts.Add(b);
            }
        }

        if (silhouetteVerts.Count < 2)
        {
            shadowCollider.pathCount = 0;
            return;
        }

        // 3️⃣ Sort silhouette vertices by angle around the light
        silhouetteVerts.Sort((p1, p2) =>
        {
            float a1 = Mathf.Atan2(p1.y - lightPos.y, p1.x - lightPos.x);
            float a2 = Mathf.Atan2(p2.y - lightPos.y, p2.x - lightPos.x);
            return a1.CompareTo(a2);
        });

        // 4️⃣ Project all silhouette vertices outward (radially)
        Vector2[] projected = new Vector2[silhouetteVerts.Count];
        for (int i = 0; i < silhouetteVerts.Count; i++)
        {
            Vector2 dir = (silhouetteVerts[i] - lightPos).normalized;
            projected[i] = silhouetteVerts[i] + dir * length;

            Debug.DrawLine(silhouetteVerts[i], projected[i], Color.yellow, 0.05f);
        }

        // 5️⃣ Combine original silhouette and projected silhouette (reversed)
        List<Vector2> final = new List<Vector2>();
        final.AddRange(silhouetteVerts);
        for (int i = projected.Length - 1; i >= 0; i--)
            final.Add(projected[i]);

        // 6️⃣ Convert to local space for shadow prefab
        Vector2[] localCombined = ToLocal(shadowCollider.transform, final.ToArray());

        // 7️⃣ Apply single continuous polygon to shadow collider
        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);
    }*/

    // test version 3
    /*public void UpdateShape(Vector2 lightPos)
    {
        if (source == null || shadowCollider == null)
            return;

        Vector2[] localPoints = source.GetPath(0);
        if (localPoints == null || localPoints.Length < 3)
            return;

        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = source.transform.TransformPoint(localPoints[i]);

        // 1️⃣ find the longest consecutive edge set facing away from the light
        List<Vector2> silhouette = new List<Vector2>();
        float bestDot = 0;
        int startIndex = 0;

        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 a = worldPoints[i];
            Vector2 b = worldPoints[(i + 1) % worldPoints.Length];
            Vector2 edge = b - a;
            Vector2 normal = new Vector2(edge.y, -edge.x).normalized;
            Vector2 toLight = (lightPos - ((a + b) / 2f)).normalized;
            float dot = Vector2.Dot(normal, toLight);

            // record the edge most directly opposite the light
            if (dot < bestDot)
            {
                bestDot = dot;
                startIndex = i;
            }
        }

        // build a list of edges opposite the light (half of the shape)
        int count = worldPoints.Length / 2;
        for (int i = 0; i <= count; i++)
            silhouette.Add(worldPoints[(startIndex + i) % worldPoints.Length]);

        // 2️⃣ project each silhouette vertex
        Vector2[] projected = new Vector2[silhouette.Count];
        for (int i = 0; i < silhouette.Count; i++)
        {
            Vector2 dir = (silhouette[i] - lightPos).normalized;
            projected[i] = silhouette[i] + dir * length;
            Debug.DrawLine(silhouette[i], projected[i], Color.yellow, 0.05f);
        }

        // 3️⃣ build one clean polygon (silhouette + reversed projection)
        List<Vector2> final = new List<Vector2>();
        final.AddRange(silhouette);
        for (int i = projected.Length - 1; i >= 0; i--)
            final.Add(projected[i]);

        // 4️⃣ ensure consistent winding (clockwise)
        if (IsClockwise(final) == false)
            final.Reverse();

        // 5️⃣ apply to collider
        Vector2[] localCombined = ToLocal(shadowCollider.transform, final.ToArray());
        shadowCollider.pathCount = 1;
        shadowCollider.SetPath(0, localCombined);
    }*/

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
}