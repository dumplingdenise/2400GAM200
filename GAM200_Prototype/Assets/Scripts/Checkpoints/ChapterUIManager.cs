using UnityEngine;
using TMPro;
using System.Collections;

public class ChapterUIManager : MonoBehaviour
{
    public static ChapterUIManager instance;
    public TextMeshProUGUI chapterText;
    public CanvasGroup canvasGroup;
    public float displayDuration = 3f;
    public float fadeSpeed = 2f;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ShowChapter(string chapterName)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(DisplayRoutine(chapterName));
    }

    private IEnumerator DisplayRoutine(string name)
    {
        chapterText.text = name;
        yield return StartCoroutine(Fade(1));  // fade in
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(Fade(0));  // fade out
    }

    private IEnumerator Fade(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
