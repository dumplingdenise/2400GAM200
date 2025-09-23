using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Android.LowLevel;

public class gameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    /* This script is for handling of the game session.
        Use for game state -> running, paused, gameOver etc.
        Stores & Restore checkpoints (**TBC if script separated or not yet)
        Handles respawning when player die (**TBC if script separated or not yet)
        Control UI Menus (**TBC if script separated or not yet)    
     */
    public enum GameState
    {
        Real,
        Shadow
    }

    [SerializeField] private GameObject mainDoll, shadowDoll;

    public GameState currentMode;

    void Start()
    {
        currentMode = GameState.Real;
    }

    // Update is called once per frame
    void Update()
    {
        ShadowSwitchMode();
    }

    void ShadowSwitchMode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentMode == GameState.Real)
            {
                currentMode = GameState.Shadow;
                Debug.Log("Now controlling: " + currentMode);
            }
            else
            {
                currentMode = GameState.Real;
                Debug.Log("Now controlling: " + currentMode);
            }

        }
    }

}
