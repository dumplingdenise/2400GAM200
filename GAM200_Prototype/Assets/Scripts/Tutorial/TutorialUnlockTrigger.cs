using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class TutorialPointUnlock : MonoBehaviour
{
    public enum UnlockType { None, Jump, SplitMergeLight }

    [Header("Tutorial Unlock Type")]
    public UnlockType unlockType;

    [Header("Prompt Settings")]
    [TextArea] public string message;       // text displayed beside icon
    public Sprite iconSprite;               // 🆕 specific icon for this tutorial
    public float fadeDuration = 0.5f;
    public float visibleDuration = 3f;

    // Shared UI (auto-found)
    private Image iconImage;
    private TextMeshProUGUI tutorialText;
    private CanvasGroup canvasGroup;

    private bool triggered = false;

    void Start()
    {
        // Find the shared TutorialPrompt UI
        GameObject promptObj = GameObject.FindWithTag("TutorialPrompt");
        if (promptObj != null)
        {
            iconImage = promptObj.GetComponentInChildren<Image>();
            tutorialText = promptObj.GetComponentInChildren<TextMeshProUGUI>();
            canvasGroup = promptObj.GetComponent<CanvasGroup>();
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || triggered) return;
        triggered = true;

        UnlockControls();
        ShowPrompt();

        GetComponent<Collider2D>().enabled = false;
    }

    void UnlockControls()
    {
        switch (unlockType)
        {
            case UnlockType.Jump:
                InputLockManager.instance.canJump = true;
                break;
            case UnlockType.SplitMergeLight:
                InputLockManager.instance.canSplit = true;
                InputLockManager.instance.canMerge = true;
                InputLockManager.instance.canControlLight = true;
                break;
        }
    }

    void ShowPrompt()
    {
        if (iconImage == null || tutorialText == null || canvasGroup == null) return;

        // 🆕 Update icon and text
        iconImage.sprite = iconSprite;
        iconImage.enabled = (iconSprite != null);  // hide if no icon
        tutorialText.text = message;

        StopAllCoroutines();
        StartCoroutine(FadePrompt());
    }

    IEnumerator FadePrompt()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(visibleDuration);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
    }
}
