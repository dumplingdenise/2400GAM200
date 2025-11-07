using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
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

    [Header("Main Menu Spotlights")]
    public GameObject playUnlit;
    public GameObject playLit;
    public GameObject settingsUnlit;
    public GameObject settingsLit;
    public GameObject exitUnlit;
    public GameObject exitLit;

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

        SetSpotlightState(playUnlit, playLit, true);
        SetSpotlightState(settingsUnlit, settingsLit, true);
        SetSpotlightState(exitUnlit, exitLit, true);
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
        StartCoroutine(LoadSceneWithDelay("CutScene", 0.3f)); // 0.3s delay
    }
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
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

    private void SetSpotlightState(GameObject unlit, GameObject lit, bool showUnlit)
    {
        if (unlit) unlit.SetActive(showUnlit);
        if (lit) lit.SetActive(!showUnlit);
    }

    // Called from EventTrigger
    public void OnHoverEnter(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                SetSpotlightState(playUnlit, playLit, false);
                break;
            case "Settings":
                SetSpotlightState(settingsUnlit, settingsLit, false);
                break;
            case "Exit":
                SetSpotlightState(exitUnlit, exitLit, false);
                break;
        }
    }

    public void OnHoverExit(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                SetSpotlightState(playUnlit, playLit, true);
                break;
            case "Settings":
                SetSpotlightState(settingsUnlit, settingsLit, true);
                break;
            case "Exit":
                SetSpotlightState(exitUnlit, exitLit, true);
                break;
        }
    }
}
