using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    //Reference to all audio
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip menuBGM;
    [SerializeField] AudioClip mainBGM;

    [SerializeField] AudioSource ambienceSource;    // second AudioSource for background SFX
    [SerializeField] AudioClip ambienceClip;        // e.g. wind, machinery, crowd noise

    // Singleton instance so there is only one AudioManager in the game
    public static AudioManager instance;

    private void Awake()
    {
        // If there is no existing AudioManager, set this one as the main instance
        if (instance == null)
        {
            instance = this;

            // Keep this AudioManager alive when switching scenes
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this) // If another AudioManager already exists, destroy this duplicate
        {
            Destroy(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load saved player preferences for music and SFX volume.
        // If there are no saved values yet, use 0.8f as a safe default.
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.8f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.8f);

        // Convert the linear 0–1 values to decibel (dB) scale.
        float musicDB = Mathf.Log10(musicVol) * 20;
        float sfxDB = Mathf.Log10(sfxVol) * 20;

        // Apply those dB values to the mixer.
        // These names must match the exposed parameters in the Audio Mixer.
        mixer.SetFloat("MusicVolume", musicDB);
        mixer.SetFloat("SFXVolume", sfxDB);

        //Start playing the menu background music when the game begins.
        musicSource.clip = menuBGM;
        musicSource.Play();

        // Subscribe to Unity’s sceneLoaded event
        // so we can change music automatically when scenes change.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Adjusts the MUSIC volume when the Music slider moves.
    public void SetMusicVolume(float linear01)
    {
        // Prevent math error from log10(0)
        linear01 = Mathf.Max(linear01, 0.0001f);

        // Convert to decibels for the mixer
        float db = Mathf.Log10(linear01) * 20f;

        // Apply to mixer parameter "MusicVolume"
        mixer.SetFloat("MusicVolume", db);

        // Save the new linear value so it’s remembered next time
        PlayerPrefs.SetFloat("MusicVol", linear01);
    }

    // Adjusts the SFX volume when the SFX slider moves.
    public void SetSFXVolume(float linear01)
    {
        // Prevent math error from log10(0)
        linear01 = Mathf.Max(linear01, 0.0001f);

        // Convert to decibels for the mixer
        float db = Mathf.Log10(linear01) * 20f;

        // Apply to mixer parameter "SFXVolume"
        mixer.SetFloat("SFXVolume", db);

        // Save the new linear value so it’s remembered next time
        PlayerPrefs.SetFloat("SFXVol", linear01);
    }

    // This method sets the BGM for each scene.
    public void PlayBGMForScene(string sceneName)
    {
        // create a temporary variable to decide which song to play
        AudioClip desired = null;

        // decide which BGM to use based on the scene name
        if (sceneName == "Menu")
        {
            desired = menuBGM;
        }
       else if (sceneName == "Main")
        {
            desired = mainBGM;
        }

        // if no matching clip found, do nothing
        if (desired == null)
        {
            return;
        }

        // if it's already playing the right clip, do nothing
        if (musicSource.clip == desired)
        {
            return;
        }

        // assign the new clip and play it
        musicSource.clip = desired;
        musicSource.Play();
    }

    public void PlayAmbienceForScene(string sceneName)
    {
        if (ambienceSource == null) return;

        // stop ambience for menus
        if (sceneName == "Menu")
        {
            ambienceSource.Stop();
            return;
        }

        // play ambience for gameplay scenes only
        if (sceneName == "Main" && ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            ambienceSource.Play();
        }
    }

    // Called automatically whenever a new scene finishes loading.
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Change BGM depending on the scene’s name.
        PlayBGMForScene(scene.name);
        PlayAmbienceForScene(scene.name);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid duplicate calls when exiting play mode
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
