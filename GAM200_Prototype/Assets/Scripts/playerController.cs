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

    //animation
    private Animator animator;
    //private bool isWalking = false;

    /*
    public LayerMask Ground;
    public Vector2 GroundCheckOffset = new Vector2(0f ,- 0.1f);
    public float groundCheckDistance = 0.12f;
    */

    void Start()
    {
       rb = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
       boxCollider = GetComponent<BoxCollider2D>();
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
        }

        var s = transform.localScale;

      /*  if (moveInput > 0)
        {
            s.x = +1f;
        }
        else if (moveInput < 0)
        {
            s.x = -1f;
        }
        else
        {
          
        } */

        transform.localScale = s;

       // animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    /*
    private void FixedUpdate()
    {
        Physics2D.Raycast(rb.position, GroundCheckOffset);
        float distance = groundCheckDistance;
        //LayerMask = Groundmask;
    }
     */
    
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
    
}
