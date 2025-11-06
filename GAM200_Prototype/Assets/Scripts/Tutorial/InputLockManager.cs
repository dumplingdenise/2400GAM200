using UnityEngine;

public class InputLockManager : MonoBehaviour
{
    public static InputLockManager instance;

    [Header("Unlocked Inputs")]
    public bool canMove = true;      // ✅ Movement always allowed from start
    public bool canJump = false;
    public bool canSplit = false;
    public bool canMerge = false;
    public bool canControlLight = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}
