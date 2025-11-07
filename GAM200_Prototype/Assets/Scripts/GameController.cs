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
    public FadeManager fadeManager;  // Reference to the FadeManager

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

    // ------------------------------
    //  Gameplay Respawn Checkpoints & Respawn logic
    // ------------------------------
    [Header("Gameplay References")]
    [SerializeField] private PlayerManager playerManager;  // PlayerManager that manages both entities
    [SerializeField] private Transform startPoint;          // Level start
    private Transform currentCheckpoint;                    // Active checkpoint
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.5f);

    // Chapter Checkpoint & Autosave
    [SerializeField] private Transform currentChapterCheckpoint;
    [SerializeField] private int currentChapterIndex = 0;
    [SerializeField] private string currentChapterName = "Tutorial";

    void Awake()
    {
        // Singleton pattern — keep only one controller
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ✅ stays alive between scenes

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

        // --- Gameplay initialization ---
        if (playerManager != null)
        {
            currentCheckpoint = (startPoint != null) ? startPoint : playerManager.physical.transform;
        }


        if (currentChapterName == "Chapter 2")
        {
            Transform spawn = GameObject.Find("Chapter2_StartPoint")?.transform;
            if (spawn != null && playerManager != null)
            {
                playerManager.physical.transform.position = spawn.position;
                playerManager.shadow.transform.position = spawn.position + Vector3.left;
                Debug.Log("Spawned player at Chapter 2 start point");
            }
            else
            {
                Debug.LogWarning("No Chapter2_StartPoint or PlayerManager found in boss scene.");
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reconnect references
        if (fadeManager == null)
            fadeManager = FindAnyObjectByType<FadeManager>();
        if (playerManager == null)
            playerManager = FindAnyObjectByType<PlayerManager>();

        // Chapter-based spawn logic
        if (scene.name == "BossLevel" || scene.name == "Chapter 2")
        {
            Transform spawn = GameObject.Find("Chapter2_StartPoint")?.transform;
            if (spawn != null && playerManager != null)
            {
                playerManager.physical.transform.position = spawn.position;
                playerManager.shadow.transform.position = spawn.position + Vector3.left;
                Debug.Log("[GameController] Spawned player at Chapter 2 start point");
            }

            // Hide collectible UI if not needed in boss scene
            var collectibleUI = FindObjectOfType<CollectibleUI>(true);
            if (collectibleUI != null)
                collectibleUI.gameObject.SetActive(false);
        }
        else
        {
            // Normal chapters (show UI)
            var collectibleUI = FindObjectOfType<CollectibleUI>(true);
            if (collectibleUI != null)
                collectibleUI.gameObject.SetActive(true);
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

    // ------------------------------
    //  Checkpoint & Respawn Methods
    // ------------------------------
    public void SetCheckpoint(Transform cp)
    {
        if (cp == null) return;

        currentCheckpoint = cp;
        Debug.Log("Checkpoint set: " + cp.name);
    }
    public void SetChapterCheckpoint(Transform cp, int index, string name)
    {
        if (cp == null) return;

        currentChapterCheckpoint = cp;
        currentChapterIndex = index;
        currentChapterName = name;

        Debug.Log($"Chapter checkpoint set: {name} (Index {index}) at {cp.position}");
    }

    public void Respawn()
    {
        if (playerManager == null) return;

        // Start fading out to black
        fadeManager.FadeOut();

        // Wait for the fade to complete, then respawn the player
        StartCoroutine(RespawnAfterFade());
    }
    private IEnumerator RespawnAfterFade()
    {
        yield return new WaitForSeconds(fadeManager.fadeSpeed);

        // compute respawn position
        Vector2 spawnPos = (currentCheckpoint != null ? (Vector2)currentCheckpoint.position : (Vector2)startPoint.position) + respawnOffset;

        // move both entities back
        playerManager.physical.transform.position = spawnPos;
        // Determine the shadow offset based on the direction the player is facing
        float shadowOffsetX = playerManager.physical.transform.localScale.x > 0 ? -1f : 1f; // Negative for right-facing, positive for left-facing

        // Place the shadow behind the physical player by applying the offset
        playerManager.shadow.transform.position = new Vector2(spawnPos.x + shadowOffsetX, spawnPos.y);

        // reset physics
        Rigidbody2D rbPhys = playerManager.physical.GetComponent<Rigidbody2D>();
        Rigidbody2D rbShadow = playerManager.shadow.GetComponent<Rigidbody2D>();
        if (rbPhys) rbPhys.linearVelocity = Vector2.zero;
        if (rbShadow) rbShadow.linearVelocity = Vector2.zero;

        /*ShadowFollower shadowFollower = playerManager.shadow.GetComponent<ShadowFollower>();
        if (shadowFollower != null)
        {
            shadowFollower.StopAnimation();  // Ensure animation stops
            shadowFollower.target = null;  // Stop shadow from following any target
            shadowFollower.enabled = false;  // Disable follower temporarily until needed
        }*/

        // restore control to physical form
        playerManager.controlState = EntityControlState.Physical;
        playerManager.linkState = EntityLinkState.Joined;
        playerManager.UpdateControlContext();

        Debug.Log($"Respawned player(s) to {spawnPos}");

        // Fade back in
        fadeManager.FadeIn();  // Fade the screen back in
    }
}
