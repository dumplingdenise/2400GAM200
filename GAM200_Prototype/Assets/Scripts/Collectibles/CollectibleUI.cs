/*using TMPro;
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
        if (FindObjectsOfType<CollectibleUI>().Length > 1)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (collectibleText == null)
            collectibleText = GetComponentInChildren<TextMeshProUGUI>();
        if (collectibleIcon == null)
            collectibleIcon = GetComponentInChildren<Image>();

        originalScale = collectibleIcon.transform.localScale;

        // Add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        *//*// Hide at start if set
        if (showAfterTutorial)
            canvasGroup.alpha = 0;
        else
            canvasGroup.alpha = 1;*//*

        // 🕶 Start hidden (for tutorial)
        canvasGroup.alpha = 0;

        // store icon original size for glow animation
        if (collectibleIcon != null)
            originalScale = collectibleIcon.transform.localScale;

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


    *//* public void ShowUI()
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
     void OnEnable()
     {
         StartCoroutine(ReappearAfterSceneLoad());
     }

     IEnumerator ReappearAfterSceneLoad()
     {
         yield return new WaitForSeconds(0.5f);
         if (canvasGroup != null && canvasGroup.alpha == 0f)
             StartCoroutine(FadeUI(1f));
     }*//*

    public void ShowPermanently()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}

*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleUI : MonoBehaviour
{
    [Header("References")]
    public Image collectibleIcon;
    public TextMeshProUGUI collectibleText;

    [Header("Settings")]
    public int totalRequired = 6;

    [Header("Glow Effect")]
    public float glowScale = 1.3f;
    public float glowDuration = 0.6f;

    private Vector3 originalScale;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (collectibleText == null)
            collectibleText = GetComponentInChildren<TextMeshProUGUI>();
        if (collectibleIcon == null)
            collectibleIcon = GetComponentInChildren<Image>();

        // store original scale for glow
        originalScale = collectibleIcon.transform.localScale;

        // make sure there's a CanvasGroup for visibility
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // hidden by default (tutorial)
        canvasGroup.alpha = 0;

        UpdateUI();

        // subscribe for pickup effects
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

    System.Collections.IEnumerator GlowEffect()
    {
        float elapsed = 0f;
        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, glowScale, elapsed / glowDuration);
            collectibleIcon.transform.localScale = originalScale * scale;
            yield return null;
        }
        collectibleIcon.transform.localScale = originalScale;
    }

    // called once player reaches Chapter 1
    public void ShowUI()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    // optional: hide again (used automatically when entering boss level)
    public void HideUI()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
