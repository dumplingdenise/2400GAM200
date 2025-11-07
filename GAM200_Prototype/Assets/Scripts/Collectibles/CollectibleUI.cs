using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectibleUI : MonoBehaviour
{
    [Header("References")]
    public Image collectibleIcon;
    public TextMeshProUGUI collectibleText;

    [Header("Settings")]
    public int totalRequired = 6;
    public bool showAfterTutorial = true;

    [Header("Glow Effect")]
    public float glowScale = 1.3f;
    public float glowDuration = 0.6f;

    private Vector3 originalScale;
    private CanvasGroup canvasGroup;  // for showing/hiding

    void Start()
    {
        if (collectibleText == null)
            collectibleText = GetComponentInChildren<TextMeshProUGUI>();
        if (collectibleIcon == null)
            collectibleIcon = GetComponentInChildren<Image>();

        originalScale = collectibleIcon.transform.localScale;

        // Add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Hide at start if set
        if (showAfterTutorial)
            canvasGroup.alpha = 0;
        else
            canvasGroup.alpha = 1;

        UpdateUI();

        // Subscribe to event
        CollectibleManager.OnCollectiblePicked += OnCollectiblePicked;
    }

    void OnDestroy()
    {
        CollectibleManager.OnCollectiblePicked -= OnCollectiblePicked;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (CollectibleManager.instance == null || collectibleText == null) return;
        collectibleText.text = $"{CollectibleManager.instance.totalCollected} / {totalRequired}";
    }

    void OnCollectiblePicked()
    {
        StopAllCoroutines();
        StartCoroutine(GlowEffect());
    }

    IEnumerator GlowEffect()
    {
        // Slight glow / shine animation
        float elapsed = 0f;
        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, glowScale, elapsed / glowDuration);
            collectibleIcon.transform.localScale = originalScale * scale;
            yield return null;
        }

        // Back to normal
        collectibleIcon.transform.localScale = originalScale;
    }

    public void ShowUI()
    {
        if (canvasGroup == null) return;
        StartCoroutine(FadeUI(1f));
    }

    public void HideUI()
    {
        if (canvasGroup == null) return;
        StartCoroutine(FadeUI(0f));
    }

    IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}

