using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Diagnostics;

public class BossLevelController : MonoBehaviour
{
    //NPC Ref
    [SerializeField] NPCWorkerPattern boss;
    [SerializeField] NPCWorkerPattern[] followers;

    // Player and GC ref
    [SerializeField] Rigidbody2D playerRb;
    [SerializeField] GameController gc;
    [SerializeField] PlayerManager playerManager;

    // flag/timers
    private float moveThreshold = 0.12f;
    private float mismatchGrace = 1f;
    private float postRespawnCooldown = 0.5f;
    private float cooldownTimer;
    private float mismatchTimer;
    bool isActive;
    public bool started;
    float preRollDelay = 0.8f;

    // Start gate
    public Collider2D startZone;

    //UI Instruction
    public GameObject instructionPanel;
    public TextMeshProUGUI InstructionText;
    bool lockOnStart = true;
    bool showLockHint = true;

    // public FadeManager fadeManager;  // Reference to the FadeManager

    float resumeDelay = 1.2f;
    float suspendUntil = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = FindAnyObjectByType<Rigidbody2D>();

        if (gc == null) gc = GameController.Instance;

        //boss.enabled = false;
        PrepareBossLevel();

        /* playerManager.linkState = EntityLinkState.Joined;
         playerManager.controlState = EntityControlState.Physical;
         playerManager.UpdateControlContext();
         playerManager.enabled = false;
        */

        //isActive = true;

        //cooldownTimer = 0f;

        //mismatchTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started || !isActive)
        {
            return;
        }

        if (!isActive && Time.time >= suspendUntil)
        {
            mismatchTimer = 0f;
            cooldownTimer = 0f;
            isActive = true;   // resume the pattern check
        }

        if (Time.time < suspendUntil) return;

        bool bossMoving = boss.isMoving;

        float spd = Mathf.Abs(playerRb.linearVelocity.x);
        bool playerMoving = spd > moveThreshold;

        if (isActive && cooldownTimer <= 0)
        {
            if (bossMoving != playerMoving)
            {
                mismatchTimer += Time.deltaTime;
            }
            else
            {
                mismatchTimer = 0;
            }
        }

        if (mismatchTimer >= mismatchGrace)
        {
            //gc.Respawn();
            isActive = false;

            mismatchTimer = 0f;
            cooldownTimer = postRespawnCooldown;
            suspendUntil = Time.time + (postRespawnCooldown + resumeDelay);
            GameController.Instance?.Respawn();
            //mismatchTimer = 0;
            // cooldownTimer =postRespawnCooldown;
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }


    }

    void PrepareBossLevel()
    {
        playerManager.linkState = EntityLinkState.Joined;
        playerManager.controlState = EntityControlState.Physical;
        playerManager.UpdateControlContext();
        playerManager.enabled = false; // Disable Switching Input

        boss.enabled = false;

        foreach (NPCWorkerPattern follower in followers)
        {
            follower.enabled = false;
        }

        instructionPanel.SetActive(true);

        started = false;
        isActive = false;
        mismatchTimer = 0;
        cooldownTimer = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (started) return;

        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            StartCoroutine(BeginSequence());
        }
    }

    IEnumerator BeginSequence()
    {
        yield return new WaitForSeconds(preRollDelay);
        instructionPanel.SetActive(false);
        boss.enabled = true;

        foreach (NPCWorkerPattern follower in followers)
        {
            follower.enabled = true;
        }
        started = true;
        isActive = true;

    }
}
