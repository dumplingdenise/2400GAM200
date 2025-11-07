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
    public float loadDelay = 0.5f;

    void Start()
    {
        gc = FindAnyObjectByType<GameController>();
        CheckUnlockStatus();
    }
    void CheckUnlockStatus()
    {
        if (!requireCollectibles)
        {
            isUnlocked = true;
        }
        else
        {
            if (CollectibleManager.instance == null)
            {
                Debug.LogWarning("No CollectibleManager found — defaulting to unlocked.");
                isUnlocked = true;
                return;
            }

            isUnlocked = CollectibleManager.instance.totalCollected >= requiredCollectibles;
        }

        // Update visuals
        if (lockedVisual) lockedVisual.SetActive(!isUnlocked);
        if (unlockedVisual) unlockedVisual.SetActive(isUnlocked);

        isUnlocked = true;

        // Trigger checkpoint logic
        triggered = true;

        if (gc != null)
        {
            gc.SetChapterCheckpoint(transform, chapterIndex, chapterName);
            Debug.Log($"Entered {chapterName} checkpoint (door unlocked)!");
        }

        // optional: disable collider if you only want it to trigger once
        GetComponent<Collider2D>().enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || triggered) return;

        // Player reached the door – check collectible requirement now
        if (requireCollectibles)
        {
            if (CollectibleManager.instance == null)
            {
                Debug.LogWarning("No CollectibleManager found — cannot verify collectibles.");
                return;
            }

            int current = CollectibleManager.instance.totalCollected;

            // 🔒 Not enough collectibles
            if (current < requiredCollectibles)
            {
                int remaining = requiredCollectibles - current;
                string msg = $"You need {remaining} more collectible{(remaining > 1 ? "s" : "")} to open this door!";
                Debug.Log(msg);

                if (PopupManager.instance != null)
                    PopupManager.instance.ShowMessage(msg);

                return; // stop here — door stays locked
            }
        }

        // ✅ If player has enough collectibles, unlock and trigger checkpoint
        if (lockedVisual) lockedVisual.SetActive(false);
        if (unlockedVisual) unlockedVisual.SetActive(true);
        isUnlocked = true;

        triggered = true;

        if (gc != null)
        {
            gc.SetChapterCheckpoint(transform, chapterIndex, chapterName);
            Debug.Log($"Entered {chapterName} checkpoint!");
        }

        // 🔓 Unlock collectible UI when reaching Chapter 1
        if (chapterName == "Chapter 1")
        {
            var collectibleUI = FindObjectOfType<CollectibleUI>(true);
            if (collectibleUI != null)
            {
                collectibleUI.gameObject.SetActive(true);
                collectibleUI.ShowUI();
            }
        }

        // ✅ NEW: load boss scene if this checkpoint transitions to another scene
        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading next scene: {nextSceneName}");
            StartCoroutine(LoadNextSceneAfterDelay());
        }
        else
        {
            // Optional: disable collider so it only triggers once
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
        if (requireCollectibles && !isUnlocked && CollectibleManager.instance != null)
        {
            if (CollectibleManager.instance.totalCollected >= requiredCollectibles)
            {
                isUnlocked = true;
                if (lockedVisual) lockedVisual.SetActive(false);
                if (unlockedVisual) unlockedVisual.SetActive(true);
                /*if (unlockedSound) AudioSource.PlayClipAtPoint(unlockedSound, transform.position);*/
                Debug.Log($"{chapterName} checkpoint unlocked in real-time!");
            }
        }
    }
}
