using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class gameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    /* This script is for handling of the game session.
        Use for game state -> running, paused, gameOver etc.
        Stores & Restore checkpoints (**TBC if script separated or not yet)
        Handles respawning when player die (**TBC if script separated or not yet)
        Control UI Menus (**TBC if script separated or not yet)    
     */

    //world state to switch
    public enum WorldState
    {
        Real,
        Shadow
    }

    // game state
    public enum GameState
    {
        Playing,
        Paused
    }

    public GameState currentGameState;
    public static gameController Instance;
    public GameObject PausedPanel;

    public GameObject CheckpointPanel;

    // button sfx
    public AudioSource audioSource;
    public AudioClip btnClickSound;

    [SerializeField] private GameObject mainDoll, shadowDoll;
    public WorldState currentMode;

    // switching for player movement control
    [SerializeField] playerController playerMove;
    [SerializeField] ShadowMovement shadowMove;

    // rigidbody referennces for physics control
    private Rigidbody2D playerrb;
    private Rigidbody2D shadowRb;

    //components of the main player (to disable/enable visuals & colliders)
    SpriteRenderer[] mainRenderers;
    Collider2D[] mainColliders;
    Animator mainAnimator;
    bool wasSimulated; // rmb if physics was acive before hiding

    // for deferred reappearance
    bool pendingReappear;
    Vector2 pendingPos;

    Collider2D[] shadowColliders;

    //camera movement
    [SerializeField] CinemachineCamera vcam;

    //for respawn/checkpoint
    [SerializeField] Transform startPoint; //where level starts (set in inspector)
    Transform currentCheckpoint; // last activated checkpoint
    [SerializeField] Vector2 respawnOffset = new Vector2 (0f, 0.5f); // spawn abit above ground

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        currentGameState = GameState.Playing;

        // start with main player state
        currentMode = WorldState.Real;

        //cache references to main doll's rigidbody and components
        playerrb = mainDoll.GetComponent<Rigidbody2D>();
        mainRenderers = mainDoll.GetComponentsInChildren<SpriteRenderer>(true);
        mainColliders = mainDoll.GetComponentsInChildren<Collider2D>(true);
        mainAnimator = mainDoll.GetComponentInChildren<Animator>();
        wasSimulated = playerrb.simulated;

        //cache shadow rigidbody
        shadowRb = shadowDoll.GetComponent<Rigidbody2D>();

        //use interpolation so moements looks smooth
        playerrb.interpolation = RigidbodyInterpolation2D.Interpolate;

        shadowColliders = shadowDoll.GetComponentsInChildren<Collider2D>(true);

        currentCheckpoint = (startPoint != null) ? startPoint : mainDoll.transform;

        SetControlForMode();
    }

    // Update is called once per frame
    void Update()
    {
        ShadowSwitchMode();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (currentGameState == GameState.Playing)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        if (PausedPanel != null)
        {
            PausedPanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        PlayClickSound();
        currentGameState = GameState.Playing;
        if (PausedPanel != null)
        {
            PausedPanel.SetActive(false);
        }
    }

    public void mainMenu()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneWithDelay("Menu", 0.3f)); // 0.3s delay
    }
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Game Closed");
    }

    void ShadowSwitchMode()
    {
        //check if previous mode is shadow to decide for the recall function
        // bool wasShadow = (currentMode == GameState.Shadow);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentMode == WorldState.Real)
            {
                // switch to shadow mode
                currentMode = WorldState.Shadow;
                Debug.Log("Now controlling: " + currentMode);

                // position shadow at main player's current position
                shadowRb.position = mainDoll.transform.position;
                HideMain(); //hide main player
                SetControlForMode();
            }
            else
            {
               // currentMode = GameState.Real;
                Debug.Log("Now controlling: " + currentMode);
                // isRecalling = wasShadow;

                //mark the real  body shoudl reappear in physics update
                pendingReappear = true;
                pendingPos = shadowDoll.transform.position;

                // zero out player's x velocity so recall doesnt fight momentum
                var v = playerrb.linearVelocity;
                v.x = 0f;
                playerrb.linearVelocity = v;

            }

        }
    }

    void SetControlForMode()
    {

        int shadowPlayerLayer = LayerMask.NameToLayer("Shadow");
        int shadowWorldLayer = LayerMask.NameToLayer("ShadowWorld");

        if (currentMode == WorldState.Real)
        {
            playerMove.enabled = true;
            shadowMove.enabled = false;

            vcam.Follow = mainDoll.transform;

            foreach (var sc in shadowColliders)
            {
                sc.enabled = false;
            }
            shadowRb.bodyType = RigidbodyType2D.Kinematic;
            shadowRb.linearVelocity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(shadowPlayerLayer, shadowWorldLayer, true);
           
        }
        else if (currentMode == WorldState.Shadow)
        {
            playerMove.enabled = false;
            shadowMove.enabled = true;

            vcam.Follow = shadowDoll.transform;

            shadowRb.bodyType = RigidbodyType2D.Dynamic;
            foreach (var sc in shadowColliders)
            {
                sc.enabled = true;
            }

            Physics2D.IgnoreLayerCollision(shadowPlayerLayer, shadowWorldLayer, false);
        }
    }

    //hide main doll 
    void HideMain()
    {
        wasSimulated = playerrb.simulated;
        playerrb.linearVelocity = Vector2.zero;

        //disable all renderes and colliders
        foreach (var r in mainRenderers)
        {
            r.enabled = false;
        }

        foreach (var c in mainColliders)
        {
            c.enabled = false;
        }

        if (mainAnimator != null)
        {
            mainAnimator.enabled = false;
        }

        //turn off physics simulation
        playerrb.simulated = false;

    }

    //show main body at the given position
    void ShowMainAt(Vector2 pos)
    {
        var oldPos = playerrb.position;

        //temporarily disbale interpolation to avoid 'lerp from old spot'
        var oldInterp = playerrb.interpolation;
        playerrb.interpolation = RigidbodyInterpolation2D.None;

        //set both the Transform (visuals) and Rigidbody (physics)
        mainDoll.transform.position = pos;
        playerrb.position = pos;

        vcam.OnTargetObjectWarped(mainDoll.transform, (Vector3)pos - (Vector3)oldPos);

        // Reset velocity & re-enable physics
        playerrb.linearVelocity = Vector2.zero;
        playerrb.simulated = wasSimulated;

        // Restore interpolation
        playerrb.interpolation = oldInterp;

        // Re-enable all renderers and colliders
        foreach (var r in mainRenderers)
        {
            r.enabled = true;
        }

        foreach (var c in mainColliders)
        {
            c.enabled = true;
        }

        if (mainAnimator != null)
        {
            mainAnimator.enabled = true;
        }
    }

    void FixedUpdate()
    {
        // Reappear is done here so physics + visuals are in sync
        if (pendingReappear)
        {
            ShowMainAt(pendingPos);
            //FindAnyObjectByType<CameraController>().SnapToTarget();
            var sc = FindAnyObjectByType<ShadowController>();
            sc.ArmFollowCooldown(4);
            sc.needInitialAlign = true;
            currentMode = WorldState.Real;
            SetControlForMode();
            pendingReappear = false;
        }
    }

    public void SetCheckpoint(Transform cp)
    {
        if (cp == null)
        {
            // if nothing pass ignore
            return;
        }
    
        //rmb the most recent checkpoint
        currentCheckpoint = cp;
        Debug.Log("Checkpoint set:" + cp.name);

        if (cp.name == "CheckPoint_3")
        {
            currentGameState = GameState.Paused;
            CheckpointPanel.SetActive(true);
        }

    }

    public void Respawn()
    {
        /*        // freeze control and physics so we can safely teleport
                playerMove.enabled = false;
                playerrb.simulated = false;
                playerrb.linearVelocity = Vector2.zero;

                // compute where to respawn and the camera warp delta
                Vector2 oldPos = playerrb.position;
                Vector2 cp = (currentCheckpoint != null ? (Vector2)currentCheckpoint.position : (Vector2)startPoint.position);
                Vector2 target = cp + respawnOffset;
                Vector3 delta = (Vector3)target - (Vector3)oldPos;

                // ensure in real mode -> camera follow is correct
                currentMode = WorldState.Real;
                SetControlForMode();
                Debug.Log("Respawn to:" + target);

                // move real body(Transform + Rigidbody).
                mainDoll.transform.position = target;
                playerrb.position = target;

                //Prevent a camera whip by informing Cinemachine of the teleport.
                vcam.OnTargetObjectWarped(mainDoll.transform, delta);

                // Reset the follower so it doesn’t tug the player on the next frame.
                var sc = FindAnyObjectByType<ShadowController>();
                if (sc != null)
                {
                    sc.ArmFollowCooldown(4);
                    sc.needInitialAlign = true;
                }

                // resume physics and input
                playerrb.simulated = true;
                playerMove.enabled = true;*/

        // compute where to respawn and the camera warp delta
        Vector2 cp = (currentCheckpoint != null ? (Vector2)currentCheckpoint.position : (Vector2)startPoint.position);
        Vector2 target = cp + respawnOffset;

        if (currentMode == WorldState.Real)
        {
            // freeze control and physics so we can safely teleport
            playerMove.enabled = false;
            playerrb.simulated = false;
            playerrb.linearVelocity = Vector2.zero;

            Vector2 oldPos = playerrb.position;
            Vector3 delta = (Vector3)target - (Vector3)oldPos;

            // ensure in real mode -> camera follow is correct
            currentMode = WorldState.Real;
            SetControlForMode();
            Debug.Log("Respawn to:" + target);

            // move real body(Transform + Rigidbody).
            mainDoll.transform.position = target;
            playerrb.position = target;

            //Prevent a camera whip by informing Cinemachine of the teleport.
            vcam.OnTargetObjectWarped(mainDoll.transform, delta);

            // resume physics and input
            playerrb.simulated = true;
            playerMove.enabled = true;
        }
        else if (currentMode == WorldState.Shadow) 
        { 
            shadowMove.enabled = false;
            shadowRb.simulated = false;
            shadowRb.linearVelocity = Vector2.zero;

            Vector2 oldPos = shadowRb.position;
            Vector3 delta = (Vector3)target - (Vector3)oldPos;

            // move shadow and real body(Transform + Rigidbody).
            shadowDoll.transform.position = target;
            shadowRb.position = target;

            mainDoll.transform.position = target;
            playerrb.position = target;

            //Prevent a camera whip by informing Cinemachine of the teleport.
            vcam.OnTargetObjectWarped(shadowDoll.transform, delta);

            // resume physics and input
            shadowRb.simulated = true;
            shadowMove.enabled = true;
        }

        // Reset the follower so it doesn’t tug the player on the next frame.
        var sc = FindAnyObjectByType<ShadowController>();
        if (sc != null)
        {
            sc.ArmFollowCooldown(4);
            sc.needInitialAlign = true;
        }
    }
    private void PlayClickSound()
    {
        if (audioSource != null && btnClickSound != null)
        {
            audioSource.PlayOneShot(btnClickSound);
        }
    }
}