﻿using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] public Animator animator;
    [SerializeField] public float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, 1)] [SerializeField] public float m_CrouchSpeed = .36f;
    [Range(0, 1)] [SerializeField] public float m_JumpSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] public float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] public bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] public LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] public Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] public Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] public Collider2D m_CrouchDisableCollider2;                // A collider that will be disabled when crouching

    const float k_GroundedRadius = .3f; // Radius of the overlap circle to determine if grounded
    public bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    public Rigidbody2D m_Rigidbody2D;
    public bool m_FacingRight = true;  // For determining which way the player is currently facing.
    public Vector3 m_Velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    public BoolEvent OnAttackEvent;
    public bool m_wasCrouching = false;
    private bool m_wasAttacking = false;
    public PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine)
        {
            //   m_CrouchDisableCollider.enabled = false;
            //    m_CrouchDisableCollider2.enabled = false;
            this.gameObject.layer =9;

        }
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnAttackEvent == null)
            OnAttackEvent = new BoolEvent();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.gameObject.layer == 10)
        {
            print("hi" + collision.name);
              m_wasCrouching = true;
            animator.SetTrigger("2To4");
            animator.ResetTrigger("4To2");

        }
        if(collision.gameObject.name=="e")
        {
            m_CrouchDisableCollider.isTrigger = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
       // m_CrouchDisableCollider.isTrigger = false;
        if (collision.gameObject.layer == 10)
        {
         //   m_CrouchDisableCollider.isTrigger = false;
        }
        }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapBoxAll(m_GroundCheck.position, new Vector2(.3f, 1), k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }


    public float fallmult=200.5f;
    public float lowMult = 2f;
    
    public void Move(float move, bool crouch, bool jump)
    {
        

        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;


                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                {
                    //    m_CrouchDisableCollider.enabled = false;
                    //   animator.SetTrigger("2To4");
                    //   animator.ResetTrigger("4To2");

                }

            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                {
                    //  m_CrouchDisableCollider.enabled = true;
                    //  animator.SetTrigger("4To2");
                    // animator.ResetTrigger("2To4");
                }


                if (m_wasCrouching)
                {
                    m_wasCrouching = false;

                    OnCrouchEvent.Invoke(false);
                }
            }
            if (GetComponent<PlayerMovement>().res.Length == 0)
            {
                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                // And then smoothing it out and applying it to the character
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
                //   print(Time.deltaTime);

            }
            else
            {

                // m_Rigidbody2D.AddForce(new Vector2(move * 10f, m_WallJumpForce) * Time.deltaTime, ForceMode2D.Impulse);
            }


        }
        // If the player should jump...

        //   print(jump);
        if (jump)
        {
            // Add a vertical force to the player.

            if (m_Grounded)
            {
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, GetComponent<PlayerMovement>().controlData.m_JumpForce), ForceMode2D.Impulse);
                animator.SetTrigger("Jump");
                move *= 0.8f;

                m_Grounded = false;
            }
        }
        else
        {

            animator.ResetTrigger("Jump");
        }
    }
    private void Update()
    {
        //if (m_Rigidbody2D.velocity.y < 0)
        //{
        //    m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * 2 * Time.deltaTime;

        //}
        //else
        //{
        //    m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * 2.5f* Time.deltaTime;

        //}
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}