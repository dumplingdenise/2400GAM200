using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject controlsPanel;
    public Button controlsBtn;
    public Button backBtn;

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
        SceneManager.LoadScene("Main");
    }

    public void ExitBtn()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }

    public void openControls()
    {
        controlsPanel.SetActive(true);
    }

    public void closeControls()
    {
        controlsPanel.SetActive(false);
    }
}
