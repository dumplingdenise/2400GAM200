using TMPro;
using UnityEngine;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [Header("Popup References")]
    public TextMeshProUGUI popupText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fadeDuration = 0.3f;
    public float displayDuration = 2f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
    }

    public void ShowMessage(string message)
    {
        // ✅ Make sure the popup is active before starting a coroutine
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ShowPopup(message));
    }


    IEnumerator ShowPopup(string message)
    {
        gameObject.SetActive(true);
        popupText.text = message;

        // fade in
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(displayDuration);

        // fade out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        HideInstant();
    }

    void HideInstant()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}