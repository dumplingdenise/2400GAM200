using UnityEngine;

public class ShadowSource : MonoBehaviour
{
    public GameObject shadowObject;       // Assign the shadow child (platformShadow)
    public ShadowConfig shadowConfig;     // Assign a ShadowConfig asset
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateShadow(Vector2 lightDir)
    {
        if (shadowObject == null || shadowConfig == null) return;

        if (lightDir == Vector2.up)
            shadowObject.transform.localPosition = shadowConfig.offsetUp;
        else if (lightDir == Vector2.down)
            shadowObject.transform.localPosition = shadowConfig.offsetDown;
        else if (lightDir == Vector2.left)
            shadowObject.transform.localPosition = shadowConfig.offsetLeft;
        else if (lightDir == Vector2.right)
            shadowObject.transform.localPosition = shadowConfig.offsetRight;

        shadowObject.transform.localScale = shadowConfig.scale;
        shadowObject.SetActive(true);
    }
}
