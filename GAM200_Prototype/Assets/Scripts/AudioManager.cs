using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip menuBGM;
    [SerializeField] AudioClip mainBGM;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.8f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.8f);

        float musicDB = Mathf.Log10(musicVol) * 20;
        float sfxDB = Mathf.Log10(sfxVol) * 20;

        mixer.SetFloat("MusicVolume", musicDB);
        mixer.SetFloat("SFXVolume", sfxDB);

        musicSource.clip = menuBGM;
        musicSource.Play();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMusicVolume(float linear01)
    {
        linear01 = Mathf.Max(linear01, 0.0001f);
        float db = Mathf.Log10(linear01) * 20f;
        mixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat("MusicVol", linear01);
    }
    public void SetSFXVolume(float linear01)
    {
        linear01 = Mathf.Max(linear01, 0.0001f);
        float db = Mathf.Log10(linear01) * 20f;
        mixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SFXVol", linear01);
    }

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

   public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }
}
