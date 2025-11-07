using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;   // Assign your Video Player here
    [SerializeField] private string nextSceneName = "Main"; // Scene to load after cutscene
    [SerializeField] private bool allowSkip = true;     // Allow player to skip?

    private bool isPrepared = false;
    private bool hasEnded = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Prepare and subscribe to end event
        videoPlayer.loopPointReached += OnVideoEnd;
        StartCoroutine(PrepareAndPlay());
    }

    private System.Collections.IEnumerator PrepareAndPlay()
    {
        videoPlayer.Prepare();

        // Wait until prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        isPrepared = true;
        videoPlayer.Play();
    }

    void Update()
    {
        // Skip option
        if (allowSkip && isPrepared && !hasEnded)
        {
            if (Input.anyKeyDown)
            {
                SkipCutscene();
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (hasEnded) return;
        hasEnded = true;

        LoadNextScene();
    }

    void SkipCutscene()
    {
        if (hasEnded) return;
        hasEnded = true;

        videoPlayer.Stop();
        LoadNextScene();
    }

    void LoadNextScene()
    {
        // Load your main gameplay scene
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}