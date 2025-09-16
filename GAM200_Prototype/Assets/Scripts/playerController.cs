using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Animator animator;

    private bool isGrounded = false;
    private bool isWalking = false;

    void Start()
    {
       rb = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
       boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }
    
    void PlayerMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpspeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
