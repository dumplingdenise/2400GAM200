using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinZoneTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject winPanel;               // The UI panel to show when player wins
    [SerializeField] BossLevelController bossController; // Optional: stop boss logic
    [SerializeField] GameController gameController;     // Optional: for fade / scene load

    [Header("Transition Settings")]
    [SerializeField] string menuSceneName = "Menu";     // Scene to load after a few seconds
    [SerializeField] float fadeDelay = 0.8f;            // Time before showing the win panel
    [SerializeField] float displayDuration = 3f;        // How long the win panel stays before returning
    [SerializeField] bool useFade = true;               // Fade-out visual transition

    private bool hasWon = false;

    private void Start()
    {
        if (gameController == null)
            gameController = FindAnyObjectByType<GameController>();

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasWon) return;
        if (!other.CompareTag("Player")) return;

        hasWon = true;
        StartCoroutine(HandleWinSequence());
    }

    private IEnumerator HandleWinSequence()
    {
        // Step 1 – Stop boss behaviour
        if (bossController != null)
            bossController.enabled = false;

        // Step 2 – Fade out (optional polish)
        if (useFade && gameController != null && gameController.fadeManager != null)
            gameController.fadeManager.FadeOut();

        // Step 3 – Short delay before showing UI
        yield return new WaitForSeconds(fadeDelay);

        // Step 4 – Show Win panel and pause gameplay
        if (winPanel != null)
            winPanel.SetActive(true);
        Time.timeScale = 0f;

        // Step 5 – Wait a few seconds while panel is visible
        yield return new WaitForSecondsRealtime(displayDuration);

        // Step 6 – Restore time & go back to menu
        Time.timeScale = 1f;
        if (useFade && gameController != null && gameController.fadeManager != null)
            gameController.fadeManager.FadeOut();

        yield return new WaitForSecondsRealtime(1f); // short fade buffer
        SceneManager.LoadScene(menuSceneName);
    }
}
