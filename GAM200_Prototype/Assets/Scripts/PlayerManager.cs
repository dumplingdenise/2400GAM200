using UnityEngine;
using Unity.Cinemachine;
using static GameController;

public enum EntityLinkState { Joined, Split }
public enum EntityControlState { Physical, Shadow }

public class PlayerManager : MonoBehaviour
{
    [Header("Entity References")]
    public PlayerMovement physical;
    public PlayerMovement shadow;
    public CameraController cameraController;

    [Header("State")]
    public EntityLinkState linkState = EntityLinkState.Joined;
    public EntityControlState controlState = EntityControlState.Physical;

    [SerializeField] CinemachineCamera vCam;

    void Awake()
    {
        // ✅ Prevent duplicates and persist across scenes
        if (FindObjectsOfType<PlayerManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        // initial state
        UpdateControlContext();
    }

    void Update()
    {
        if (GameController.IsPaused) return;

        HandleInputs();
    }

    void HandleInputs()
    {
        /*if (Input.GetKeyDown(KeyCode.E))*/
        if (InputLockManager.instance.canSplit && Input.GetKeyDown(KeyCode.E))
            ToggleLinkState();

        /*if (Input.GetKeyDown(KeyCode.Q) && linkState == EntityLinkState.Split)*/
        if (InputLockManager.instance.canMerge && linkState == EntityLinkState.Split && Input.GetKeyDown(KeyCode.Q))
            SwitchControl();
    }

    void ToggleLinkState()
    {
        if (linkState == EntityLinkState.Joined)
        {
            // --- Split ---
            linkState = EntityLinkState.Split;
            shadow.gameObject.SetActive(true);
            /*shadow.transform.position = physical.transform.position + Vector3.right * 1.5f;*/
        }
        else
        {
            // --- Join ---
            linkState = EntityLinkState.Joined;

            if (controlState == EntityControlState.Physical)
                shadow.transform.position = physical.transform.position;
            else
                physical.transform.position = shadow.transform.position;
        }

        UpdateControlContext();
    }

    void SwitchControl()
    {
        controlState = (controlState == EntityControlState.Physical)
            ? EntityControlState.Shadow
            : EntityControlState.Physical;

        UpdateControlContext();
    }

    public void UpdateControlContext()
    {
        bool controllingPhysical = controlState == EntityControlState.Physical;

        physical.SetActiveControl(controllingPhysical);
        shadow.SetActiveControl(!controllingPhysical);

        // always make sure both are visible when split
        if (linkState == EntityLinkState.Split)
        {
            physical.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);

            // disable shadow follow
            ShadowFollower follower = shadow.GetComponent<ShadowFollower>();
            if (follower != null)
            {
                follower.target = null;
                // Disable the follower script while shadow is directly controlled
                follower.enabled = false;
                follower.StopAnimation();
            }                
        }
        else // Joined
        {
            physical.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);

            if (controlState == EntityControlState.Shadow)
            {
                // hide physical only when controlling shadow in joined mode
                physical.gameObject.SetActive(false);
            }
            else
            {
                // shadow follows physical
                ShadowFollower follower = shadow.GetComponent<ShadowFollower>();
                if (follower != null)
                {
                    follower.enabled = true;
                    follower.target = physical.transform;
                }                 
            }
        }

        // update camera target
        if (vCam != null)
            vCam.Follow = controllingPhysical ? physical.transform : shadow.transform;

        // new 
        UpdateCollisionLayers();
    }

    void UpdateCollisionLayers()
    {
        int realWorldLayer = LayerMask.NameToLayer("RealWorld");
        int realLayer = LayerMask.NameToLayer("Real");
        int shadowWorldLayer = LayerMask.NameToLayer("ShadowWorld");

        bool controllingPhysical = controlState == EntityControlState.Physical;

        // Physical player → only collide with real world
        Physics2D.IgnoreLayerCollision(realWorldLayer, realLayer, !controllingPhysical);
        Physics2D.IgnoreLayerCollision(realWorldLayer, shadowWorldLayer, true);

        // Shadow player → only collide with shadow world
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realLayer, controllingPhysical);
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realWorldLayer, controllingPhysical);

        /*int realWorldLayer = LayerMask.NameToLayer("RealWorld");
        int realLayer = LayerMask.NameToLayer("Real");
        int shadowWorldLayer = LayerMask.NameToLayer("ShadowWorld");
        int realWorldDoorLayer = LayerMask.NameToLayer("RealWorldDoor"); // 👈 add this

        bool controllingPhysical = controlState == EntityControlState.Physical;

        // Physical player → only collide with real world
        Physics2D.IgnoreLayerCollision(realWorldLayer, realLayer, !controllingPhysical);
        Physics2D.IgnoreLayerCollision(realWorldLayer, shadowWorldLayer, true);

        // Shadow player → only collide with shadow world
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realLayer, controllingPhysical);
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realWorldLayer, controllingPhysical);

        // 👇 ADD THIS: allow shadow ↔ real-world-door collision always
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realWorldDoorLayer, false);*/
    }
}