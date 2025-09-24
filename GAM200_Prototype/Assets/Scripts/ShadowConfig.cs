using UnityEngine;

[CreateAssetMenu(fileName = "shadowConfig", menuName = "Scriptable Objects/shadowConfig")]
public class ShadowConfig : ScriptableObject
{
    public Vector2 offsetUp;
    public Vector2 offsetDown;
    public Vector2 offsetLeft;
    public Vector2 offsetRight;

    public Vector2 scale = Vector2.one;
}
