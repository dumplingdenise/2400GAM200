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

    [Header("Audio References")]
    [SerializeField] private AudioSource sfxSource;   // the AudioSource that will play the sounds
    [SerializeField] private AudioClip splitSFX;
    [SerializeField] private AudioClip mergeSFX;
    [SerializeField] private AudioClip switchSFX;

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
        if (Input.GetKeyDown(KeyCode.E))
            if (InputLockManager.instance.canSplit && Input.GetKeyDown(KeyCode.E))
                ToggleLinkState();

        if (Input.GetKeyDown(KeyCode.Q) && linkState == EntityLinkState.Split)
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

            PlaySFX(splitSFX);
            shadow.transform.position = physical.transform.position + Vector3.right * 1.5f;
        }
        else
        {
            // --- Join ---
            linkState = EntityLinkState.Joined;

            // play merge sound
            PlaySFX(mergeSFX);


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

        // play switch sound
        PlaySFX(switchSFX);

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
        /*int realWorldLayer = LayerMask.NameToLayer("RealWorld");
        int realLayer = LayerMask.NameToLayer("Real");
        int shadowWorldLayer = LayerMask.NameToLayer("ShadowWorld");

        bool controllingPhysical = controlState == EntityControlState.Physical;

        // Physical player → only collide with real world
        Physics2D.IgnoreLayerCollision(realWorldLayer, realLayer, !controllingPhysical);
        Physics2D.IgnoreLayerCollision(realWorldLayer, shadowWorldLayer, true);

        // Shadow player → only collide with shadow world
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realLayer, controllingPhysical);
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realWorldLayer, controllingPhysical);*/

        int realWorldLayer = LayerMask.NameToLayer("RealWorld");
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
        Physics2D.IgnoreLayerCollision(shadowWorldLayer, realWorldDoorLayer, false);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}

// TESTING for shadow rise

/*using UnityEngine;
using Unity.Cinemachine;
using static GameController;
using System.Collections;

public enum EntityLinkState { Joined, Split }
public enum EntityControlState { Physical, Shadow }

public class PlayerManager : MonoBehaviour
{
    public PlayerMovement physical;
    public PlayerMovement shadow;
    public CameraController cameraController;

    public EntityLinkState linkState = EntityLinkState.Joined;
    public EntityControlState controlState = EntityControlState.Physical;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip splitSFX;
    [SerializeField] private AudioClip mergeSFX;
    [SerializeField] private AudioClip switchSFX;

    [SerializeField] CinemachineCamera vCam;

    private ShadowFollower follower;

    void Awake()
    {
        follower = shadow.GetComponent<ShadowFollower>();
    }

    void Start()
    {
        UpdateControlContext();
    }

    void Update()
    {
        if (GameController.IsPaused) return;

        if (InputLockManager.instance.canSplit && Input.GetKeyDown(KeyCode.E))
            ToggleLinkState();

        if (InputLockManager.instance.canMerge && linkState == EntityLinkState.Split && Input.GetKeyDown(KeyCode.Q))
            SwitchControl();
    }

    void ToggleLinkState()
    {
        if (linkState == EntityLinkState.Joined)
        {
            // Split
            linkState = EntityLinkState.Split;

            physical.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);

            PlaySFX(splitSFX);

            // Spawn behind physical
            bool facingRight = physical.GetComponent<Animator>().GetBool("isFacingRight");
            float dir = facingRight ? -1f : 1f;
            shadow.transform.position =
                physical.transform.position + new Vector3(dir * 0.5f, 0f, 0f);

            // Shadow upright
            follower.SetFlatMode(false);
            StartCoroutine(RiseShadow());
        }
        else
        {
            // Merge
            linkState = EntityLinkState.Joined;

            PlaySFX(mergeSFX);

            // Snap to same position
            if (controlState == EntityControlState.Physical)
                shadow.transform.position = physical.transform.position;
            else
                physical.transform.position = shadow.transform.position;

            // Shadow flat
            follower.SetFlatMode(true);
        }

        UpdateControlContext();
    }

    void SwitchControl()
    {
        controlState =
            (controlState == EntityControlState.Physical) ?
            EntityControlState.Shadow :
            EntityControlState.Physical;

        PlaySFX(switchSFX);
        UpdateControlContext();
    }

    public void UpdateControlContext()
    {
        bool controllingPhysical = controlState == EntityControlState.Physical;

        physical.SetActiveControl(controllingPhysical);
        shadow.SetActiveControl(!controllingPhysical);

        if (linkState == EntityLinkState.Split)
        {
            // Split mode: both upright
            physical.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);

            follower.enabled = false;
            follower.isFlatMode = false;

            follower.StopAnimation();
        }
        else
        {
            // Joined mode
            if (controlState == EntityControlState.Shadow)
            {
                // Shadow active, upright
                physical.gameObject.SetActive(false);

                follower.enabled = false;
                follower.SetFlatMode(false);
            }
            else
            {
                // Physical active, shadow flat + following
                physical.gameObject.SetActive(true);

                follower.enabled = true;
                follower.target = physical.transform;
                follower.SetFlatMode(true);
                *//*follower.isFlatMode = false;*//*
            }
        }

        if (vCam != null)
            vCam.Follow = controllingPhysical ? physical.transform : shadow.transform;

        UpdateCollisionLayers();
    }

    void UpdateCollisionLayers()
    {
        int realWorld = LayerMask.NameToLayer("RealWorld");
        int real = LayerMask.NameToLayer("Real");
        int shadowWorld = LayerMask.NameToLayer("ShadowWorld");

        bool isPhysical = controlState == EntityControlState.Physical;

        Physics2D.IgnoreLayerCollision(realWorld, real, !isPhysical);
        Physics2D.IgnoreLayerCollision(realWorld, shadowWorld, true);

        Physics2D.IgnoreLayerCollision(shadowWorld, real, isPhysical);
        Physics2D.IgnoreLayerCollision(shadowWorld, realWorld, isPhysical);
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip);
    }

    IEnumerator RiseShadow()
    {
        Vector3 start = follower.flatLocalScale;
        Vector3 end = follower.uprightLocalScale;

        float t = 0;
        while (t < 0.25f)
        {
            shadow.transform.localScale = Vector3.Lerp(start, end, t / 0.25f);
            t += Time.deltaTime;
            yield return null;
        }

        shadow.transform.localScale = end;
    }
}*/



