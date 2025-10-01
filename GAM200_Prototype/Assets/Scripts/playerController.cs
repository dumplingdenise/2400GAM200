using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static gameController;

public class playerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    /* For player state, input & control
        Player movement
        Player input
        Player state (if have -> shadow or physical state, dead alive etc)

        ** See if want to handle player and shadow movement in 1 single script or separates it.
        ** See if want to handle the switching of merged or split state here or separated scripts
        ** See if want to handle player interaction with shadow/physical objects here or separated
     */

    public float speed = 5f;
    public float jumpspeed = 10f;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private bool isGrounded = false; // check for jumping
    private bool wasGrounded;

    //animation
    private Animator animator;
    //private bool isWalking = false;

    
    public LayerMask Ground;
    public Vector2 GroundCheckOffset = new Vector2(0f ,- 0.1f);
    public float groundCheckDistance = 0.12f;

    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip jumpClip, landClip;
    [SerializeField] public AudioSource footstepSource;
    private float walkThreshold = 0.05f; // min horizontal speed
    private float footstepMaxVol = 1f;
    private float footstepFade = 12f; // how fast to fade per second

    

    void Start()
    {
       rb = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
       boxCollider = GetComponent<BoxCollider2D>();

        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.Instance == null) return;
        if (gameController.Instance.currentGameState == GameState.Paused)
        {
            return;
        }
        PlayerMovement();

        bool walking = isGrounded && Mathf.Abs(rb.linearVelocity.x) > walkThreshold;

        if (footstepSource)  // null-guard
        {
            if (walking)
            {
                if (!footstepSource.isPlaying)
                    footstepSource.Play();

                // fade IN toward max
                footstepSource.volume = Mathf.MoveTowards(
                    footstepSource.volume, footstepMaxVol, footstepFade * Time.deltaTime);
            }
            else
            {
                // fade OUT toward zero
                footstepSource.volume = Mathf.MoveTowards(
                    footstepSource.volume, 0f, footstepFade * Time.deltaTime);

                // stop once silent
                if (footstepSource.isPlaying && footstepSource.volume <= 0.001f)
                    footstepSource.Stop();
            }
        }
    }
    
    void PlayerMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(moveInput) < 0.01f)
        {
            moveInput = 0f;
        }

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpspeed);

            if (sfx && jumpClip)
            {
                //sfx
                sfx.pitch = Random.Range(0.98f, 1.02f);
                sfx.PlayOneShot(jumpClip);
            }
      
        }

        var s = transform.localScale;

        if (moveInput > 0)
        {
            /*s.x = +1f;*/
            s.x = Mathf.Abs(s.x);
        }
        else if (moveInput < 0)
        {
            /*s.x = -1f;*/
            s.x = -Mathf.Abs(s.x);
        }
        else
        {

        }

        // walking animation when jump
        /*if (moveInput != 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }*/
        // no walking animation when jump

        if (!isGrounded)
        {
            // In the air → always idle for now
            animator.SetBool("isWalking", false);
        }
        else
        {
            // On the ground → walk only if moving
            animator.SetBool("isWalking", moveInput != 0);
        }

        transform.localScale = s;

       // animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    
    private void FixedUpdate()
    {
        Vector2 origin = rb.position + GroundCheckOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, Ground);
        isGrounded = (hit.collider != null && hit.normal.y >= 0.7f);
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.yellow);

        if (!wasGrounded && isGrounded)
        {
            if (sfx && landClip)
            {
                sfx.pitch = Random.Range(0.98f, 1.02f);
                sfx.PlayOneShot(landClip);
            }
           
        }

        wasGrounded = isGrounded;
 
    }
     
   /* 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
       // animator.SetBool("Grounded", isGrounded);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
       // animator.SetBool("Grounded", isGrounded);
    }
    */
}
