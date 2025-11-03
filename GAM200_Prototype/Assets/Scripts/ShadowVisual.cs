using UnityEngine;
using System.Collections.Generic;

public class ShadowVisual : MonoBehaviour
{
    private PolygonCollider2D collider;
    private MeshFilter meshFilter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        collider = GetComponent<PolygonCollider2D>();
        meshFilter = GetComponent<MeshFilter>();
    }

    void LateUpdate()
    {
        UpdateMesh();
    }

    /*void UpdateMesh()
    {
        if (collider.pathCount == 0) return;

        Vector2[] pts = collider.GetPath(0);
        if (pts.Length < 3) return; // need 3+ points for a polygon

        // Convert collider points to Vector3
        Vector3[] verts = new Vector3[pts.Length];
        for (int i = 0; i < pts.Length; i++)
            verts[i] = pts[i];

        // Create triangle fan (connect 0 -> i -> i+1)
        List<int> tris = new List<int>();
        for (int i = 1; i < pts.Length - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        // Build and assign the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }*/

    void UpdateMesh()
    {
        if (collider.pathCount == 0)
            return;

        Vector2[] pts = collider.GetPath(0);
        if (pts.Length < 3)
            return;

        // Build vertices
        Vector3[] verts = new Vector3[pts.Length];
        for (int i = 0; i < pts.Length; i++)
            verts[i] = new Vector3(pts[i].x, pts[i].y, 0.01f); // slight Z offset

        // Triangulate (simple fan)
        List<int> tris = new List<int>();
        for (int i = 1; i < pts.Length - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
}
