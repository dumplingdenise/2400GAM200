using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowSource : MonoBehaviour
{
    public GameObject shadowObject;       // Assign the shadow child (platformShadow)

    [Header("Shadow setting")]
    public Vector2 offsetUp;
    public Vector2 offsetDown;
    public Vector2 offsetLeft;
    public Vector2 offsetRight;

    /*public Vector2 scale = Vector2.one;*/

    /*// new item
    private Vector3 targetOffset;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowShadow(Vector2 lightDir)
    {
        if (shadowObject == null) return;

        Vector3 newOffset = Vector3.zero;

        if (Vector2.Dot(lightDir, Vector2.up) > 0.9f)
            newOffset = offsetUp;
        else if (Vector2.Dot(lightDir, Vector2.down) > 0.9f)
            newOffset = offsetDown;
        else if (Vector2.Dot(lightDir, Vector2.left) > 0.9f)
            newOffset = offsetLeft;
        else if (Vector2.Dot(lightDir, Vector2.right) > 0.9f)
            newOffset = offsetRight;

        shadowObject.transform.localPosition = newOffset;
        shadowObject.transform.localPosition += new Vector3(0, 0, 1); // keep behind
        shadowObject.SetActive(true);
    }

    public void HideShadow()
    {
        if (shadowObject != null)
            shadowObject.SetActive(false);
    }
}
