using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject controlsPanel;
    public Button controlsBtn;
    public Button backBtn;

    public AudioSource audioSource;   
    public AudioClip btnClickSound;      

    private void Awake()
    {
        int screenW = 1920;
        int screenH = 1080;
        bool isFullScreen = true;
        Screen.SetResolution(screenW, screenH, isFullScreen);


        controlsPanel.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controlsBtn.onClick.AddListener(openControls);
        /*backBtn.onClick.AddListener(closeControls);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayBtn()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneWithDelay("Main", 0.3f)); // 0.3s delay
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

    public void openControls()
    {
        PlayClickSound();
        controlsPanel.SetActive(true);
    }

    public void closeControls()
    {
        PlayClickSound();
        controlsPanel.SetActive(false);
    }

    private void PlayClickSound()
    {
        if (audioSource != null && btnClickSound != null)
        {
            audioSource.PlayOneShot(btnClickSound);
        }
    }
}
