using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    void Awake()
    {
        // destroy duplicates when returning to menu
        if (FindObjectsOfType<PersistentUI>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}

