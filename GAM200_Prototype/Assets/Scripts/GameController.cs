using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused
    }

    public static GameController Instance;
    public GameState currentGameState;

    [Header("Paused")]
    public GameObject PausedPanel;

    [Header("Settings panel & Btns")]
    public GameObject settingsPanel;
    /*public Button settingsBtn;*/
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

    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private Color activeColor;
    private Color inactiveColor;

    public static bool IsPaused
    {
        get { return Instance != null && Instance.currentGameState == GameState.Paused; }
    }

    void Awake()
    {
        // singleton pattern for global access
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ColorUtility.TryParseHtmlString("#323232", out activeColor);
        ColorUtility.TryParseHtmlString("#FFFFFF", out inactiveColor);

        settingsPanel.SetActive(false);
        ShowControlsTab();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGameState = GameState.Playing;

        // Sync sliders with current saved volume values
        if (AudioManager.instance != null)
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.8f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.8f);

            musicSlider.value = musicVol;
            sfxSlider.value = sfxVol;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (currentGameState == GameState.Playing)
            PauseGame();
        else
            ResumeGame();
    }

    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        /*Time.timeScale = 0f;*/
        if (PausedPanel)
            PausedPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        PlayClickSound();
        currentGameState = GameState.Playing;
        /*Time.timeScale = 1f;*/
        if (PausedPanel)
            PausedPanel.SetActive(false);
    }

    public void MainMenu()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneWithDelay("Menu", 0.25f));
    }

    /*public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Game Closed");
    }*/

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

    IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }

    void PlayClickSound()
    {
        if (audioSource && btnClickSound)
            audioSource.PlayOneShot(btnClickSound);
    }

    public void OnMusicSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSFXVolume(value);
    }
}
