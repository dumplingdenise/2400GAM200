using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChapterCheckpointTrigger : MonoBehaviour
{
    private GameController gc;
    private bool triggered = false;

    [Header("Chapter Info")]
    public int chapterIndex = 1;             // Example: 1 = Chapter 1
    public string chapterName = "Chapter 1"; // Optional name for debug or UI

    [Header("Collectible Requirement")]
    public bool requireCollectibles = false;
    public int requiredCollectibles = 0;     // Coins needed to unlock
    public GameObject lockedVisual;          // Optional visual (e.g., closed gate)
    public GameObject unlockedVisual;        // Optional visual (e.g., glowing gate)
    /*public AudioClip lockedSound;            // Optional feedback
    public AudioClip unlockedSound;          // Optional feedback*/

    private bool isUnlocked = false;

    [Header("Scene Transition (Optional)")]
    public bool loadNextScene = false;      // Toggle to true if this checkpoint loads a new scene
    public string nextSceneName;            // The name of the scene to load
    public float loadDelay = 2f;

    private bool canOpen = false;

    void Start()
    {
        gc = FindAnyObjectByType<GameController>();
        CheckUnlockStatus();
    }
    void CheckUnlockStatus()
    {
        // --- Check if this checkpoint should be locked or unlocked ---
        if (!requireCollectibles)
        {
            // Chapters 0 & 1
            isUnlocked = true;
        }
        else
        {
            // Chapter 2 (requires collectibles)
            if (CollectibleManager.instance == null)
            {
                Debug.LogWarning("No CollectibleManager found — defaulting to unlocked.");
                isUnlocked = true;
                return;
            }

            int collected = CollectibleManager.instance.totalCollected;
            isUnlocked = collected >= requiredCollectibles;
        }

        // --- Update visuals only ---
        if (lockedVisual) lockedVisual.SetActive(!isUnlocked);
        if (unlockedVisual) unlockedVisual.SetActive(isUnlocked);

        // ✅ Do NOT mark triggered or disable collider here.
        // The checkpoint should only trigger when the player actually enters it.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || triggered) return;

        // Player reached the door
        if (requireCollectibles)
        {
            if (CollectibleManager.instance == null)
            {
                Debug.LogWarning("No CollectibleManager found — cannot verify collectibles.");
                return;
            }

            int current = CollectibleManager.instance.totalCollected;

            if (current < requiredCollectibles)
            {
                int remaining = requiredCollectibles - current;
                string msg = $"You need {remaining} more collectible{(remaining > 1 ? "s" : "")} to open this door!";
                Debug.Log(msg);

                if (PopupManager.instance != null)
                    PopupManager.instance.ShowMessage(msg);
                return;
            }

            // ✅ Player has enough collectibles now
            if (!canOpen)
            {
                canOpen = true;
                Debug.Log($"{chapterName} checkpoint now unlockable (triggered manually)");
            }
        }

        // Show the visual and trigger logic
        if (lockedVisual) lockedVisual.SetActive(false);
        if (unlockedVisual) unlockedVisual.SetActive(true);
        isUnlocked = true;
        triggered = true;

        if (gc != null)
        {
            gc.SetChapterCheckpoint(transform, chapterIndex, chapterName);
            Debug.Log($"Entered {chapterName} checkpoint!");
        }

        if (ChapterUIManager.instance != null)
        {
            ChapterUIManager.instance.ShowChapter(chapterName);
        }

        if (chapterName == "Chapter 1")
        {
            var collectibleUI = FindAnyObjectByType<CollectibleUI>();
            if (collectibleUI != null)
                collectibleUI.ShowUI();
        }

        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading next scene: {nextSceneName}");
            StartCoroutine(LoadNextSceneAfterDelay());
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }


    private IEnumerator LoadNextSceneAfterDelay()
    {
        if (gc != null && gc.fadeManager != null)
            gc.fadeManager.FadeOut(); // fade to black

        yield return new WaitForSeconds(loadDelay);

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // Optional: Real-time unlock check
    void Update()
    {
        // Only check collectible unlock readiness, don't change visuals yet
        if (requireCollectibles && !canOpen && CollectibleManager.instance != null)
        {
            if (CollectibleManager.instance.totalCollected >= requiredCollectibles)
            {
                canOpen = true; // ✅ Now ready to open, but keep visuals locked
                Debug.Log($"{chapterName} checkpoint now unlockable (waiting for player)");
            }
        }
    }
}
