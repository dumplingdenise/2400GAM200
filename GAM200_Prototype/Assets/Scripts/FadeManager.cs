using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public Image fadePanelImage;  // Reference to the Fade Panel's Image component
    public float fadeSpeed = 1f;  // Speed of the fade effect

    private void Start()
    {
        // Ensure the fade panel is initially invisible
        fadePanelImage.canvasRenderer.SetAlpha(1f);
    }

    // Call this function to fade to black
    public void FadeOut()
    {
        StartCoroutine(Fade(1));  // Fade to black (alpha 1)
    }

    // Call this function to fade back in
    public void FadeIn()
    {
        StartCoroutine(Fade(0));  // Fade back in (alpha 0)
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanelImage.color.a;
        float elapsedTime = 0f;

        // Loop to smoothly transition the alpha value
        while (elapsedTime < fadeSpeed)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeSpeed);
            Color newColor = fadePanelImage.color;
            newColor.a = alpha;  // Update the alpha value
            fadePanelImage.color = newColor;  // Set the new color with updated alpha

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final alpha is set
        Color finalColor = fadePanelImage.color;
        finalColor.a = targetAlpha;
        fadePanelImage.color = finalColor;
    }
}
