using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class AutoColliderUpdater : MonoBehaviour
{
    private SpriteRenderer sr;
    private PolygonCollider2D poly;
    private Sprite lastSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
        UpdateColliderShape();
    }

    void Update()
    {
        // Run both in play mode and editor
        if (sr == null || poly == null) return;
        if (sr.sprite != lastSprite)
            UpdateColliderShape();
    }

    void UpdateColliderShape()
    {
        if (sr.sprite == null) return;

        lastSprite = sr.sprite;

        // Clear old paths
        poly.pathCount = 0;

        // Use the sprite's physics shape, which matches "Auto Generate" button behavior
        int shapeCount = sr.sprite.GetPhysicsShapeCount();
        if (shapeCount == 0) return;

        poly.pathCount = shapeCount;

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            path.Clear();
            sr.sprite.GetPhysicsShape(i, path);
            poly.SetPath(i, path.ToArray());
        }
    }
}
