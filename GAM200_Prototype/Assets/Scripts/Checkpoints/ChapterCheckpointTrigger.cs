using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChapterCheckpointTrigger : MonoBehaviour
{
    private GameController gc;
    private bool triggered = false;

    [Header("Chapter Info")]
    public int chapterIndex = 1;             // Example: 1 = Chapter 1
    public string chapterName = "Chapter 1"; // Optional name for debug or UI

    void Start()
    {
        gc = FindAnyObjectByType<GameController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || triggered) return;

        triggered = true;

        if (gc != null)
        {
            gc.SetChapterCheckpoint(transform, chapterIndex, chapterName);
            Debug.Log($"Entered {chapterName} checkpoint!");
        }

        // Optional: disable collider if you only want it to trigger once
        GetComponent<Collider2D>().enabled = false;
    }
}
