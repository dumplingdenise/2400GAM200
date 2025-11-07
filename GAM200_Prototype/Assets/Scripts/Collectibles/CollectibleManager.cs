using UnityEngine;
using System;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager instance;
    public static event Action OnCollectiblePicked; // 🔔 Notify others when collected

    [Header("Collectible Count")]
    public int totalCollected = 0;

    void Awake()
    {
        // Make sure only one instance exists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Load collectible count when the game starts
        LoadCollectibles();
    }

    void Start()
    {
        ResetCollectibles(); // 🔥 Clears all collectible data at game start
    }

    /// <summary>
    /// Add collectibles to the total count and save immediately.
    /// </summary>
    public void AddCollectible(int amount)
    {
        totalCollected += amount;
        Debug.Log($"[CollectibleManager] Collected: {totalCollected}");
        SaveCollectibles();

        // 🔔 Trigger event so UI can animate
        OnCollectiblePicked?.Invoke();
    }

    /// <summary>
    /// Save current collectible count to PlayerPrefs.
    /// </summary>
    public void SaveCollectibles()
    {
        PlayerPrefs.SetInt("TotalCollectibles", totalCollected);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load collectible count when game starts.
    /// </summary>
    public void LoadCollectibles()
    {
        totalCollected = PlayerPrefs.GetInt("TotalCollectibles", 0);
        Debug.Log($"[CollectibleManager] Loaded Collectibles: {totalCollected}");
    }

    /// <summary>
    /// Reset all collectibles (useful for new game).
    /// </summary>
    public void ResetCollectibles()
    {
        totalCollected = 0;
        SaveCollectibles();
        Debug.Log("[CollectibleManager] Collectibles reset to 0");
    }
}
