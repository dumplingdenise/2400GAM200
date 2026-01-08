using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Main menu button texts")]
    public TextMeshProUGUI playBtnText;
    public TextMeshProUGUI settingsBtnText;
    public TextMeshProUGUI exitBtnText;

    [Header("Settings panel & Btns")]
    public GameObject settingsPanel;
    public Button settingsBtn;
    public Button backBtn;

    [Header("Settings Tab")]
    public Button controlsBTN;
    public Button audioBTN;

    [Header("Tab Backgrounds")]
    public GameObject controlsBG;
    public GameObject audioBG;

    [Header("Tab Texts")]
    public TextMeshProUGUI controlsText;
    public TextMeshProUGUI audioText;

    [Header("Settings Pages")]
    public GameObject controlsPage;
    public GameObject audioPage;

    [Header("Audio")]
    public AudioSource audioSource;   
    public AudioClip btnClickSound;

    private Color activeColor;
    private Color inactiveColor;

    public Animator animator;

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        int screenW = 1920;
        int screenH = 1080;
        bool isFullScreen = true;
        Screen.SetResolution(screenW, screenH, isFullScreen);

        ColorUtility.TryParseHtmlString("#323232", out activeColor);
        ColorUtility.TryParseHtmlString("#FFFFFF", out inactiveColor);

        settingsPanel.SetActive(false);
        ShowControlsTab();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        settingsBtn.onClick.AddListener(openSettings);
        /*backBtn.onClick.AddListener(closeControls);*/

        controlsBTN.onClick.AddListener(ShowControlsTab);
        audioBTN.onClick.AddListener(ShowAudioTab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayBtn()
    {
        PlayClickSound();
        /*StartCoroutine(LoadSceneWithDelay("CutScene", 0.3f)); // 0.3s delay*/
        StartCoroutine(FadeAndLoad("CutScene"));
    }
    /*private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }*/

    private IEnumerator FadeAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true; // lock input
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float n = t / fadeDuration;
            fadeGroup.alpha = Mathf.SmoothStep(0f, 1f, n);
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }
    public void ExitBtn()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Game Closed");
    }

    public void openSettings()
    {
        PlayClickSound();
        settingsPanel.SetActive(true);
        ShowControlsTab(); // Default tab when opening
    }

    public void closeSettings()
    {
        PlayClickSound();
        settingsPanel.SetActive(false);
    }

    public void ShowControlsTab()
    {
        PlayClickSound();
        controlsPage.SetActive(true);
        audioPage.SetActive(false);

        controlsBG.SetActive(true);
        audioBG.SetActive(false);

        controlsText.color = activeColor;
        audioText.color = inactiveColor;
    }

    public void ShowAudioTab()
    {
        PlayClickSound();
        controlsPage.SetActive(false);
        audioPage.SetActive(true);

        controlsBG.SetActive(false);
        audioBG.SetActive(true);

        controlsText.color = inactiveColor;
        audioText.color = activeColor;
    }

    private void PlayClickSound()
    {
        if (audioSource != null && btnClickSound != null)
        {
            audioSource.PlayOneShot(btnClickSound);
        }
    }

    // Called from EventTrigger
    public void OnHoverEnter(string buttonName)
    {
        ColorUtility.TryParseHtmlString("#511B14", out Color newColor);
        switch (buttonName)
        {
            case "Play":
                animator.SetBool("isHover", true);               
                playBtnText.color = newColor;
                playBtnText.fontSize = 150;
                break;
            case "Settings":
                animator.SetBool("isHover", true);
                settingsBtnText.color = newColor;
                settingsBtnText.fontSize = 130;
                break;
            case "Exit":
                animator.SetBool("isHover", true);
                exitBtnText.color = newColor;
                exitBtnText.fontSize = 150;
                break;
        }
    }

    public void OnHoverExit(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                animator.SetBool("isHover", false);
                playBtnText.fontSize = 100;
                break;
            case "Settings":
                animator.SetBool("isHover", false);
                settingsBtnText.fontSize = 100;
                break;
            case "Exit":
                animator.SetBool("isHover", false);
                exitBtnText.fontSize = 100;
                break;
        }
    }
}
