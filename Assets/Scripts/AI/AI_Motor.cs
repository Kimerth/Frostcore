using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class AI_Motor : MonoBehaviour 
{
    [SerializeField] private float m_MaxSpeed = 10f;            // Player max move speed on X axis.
    [SerializeField] private float m_JumpForce = 400;           // Amount of force added when the player jumps.
    [SerializeField] private float m_MaxDistanceToAWall = 1.5f; // Stop hitting yourself...
    [SerializeField] private LayerMask m_WhatIsGround;          // A mask determining what is ground to the character.

    private Transform GroundCheck;      // A position marking where to check if the player is grounded.
    const float GroundedRadius = .4f;   // Radius of the overlap circle to determine if grounded.
    private bool Grounded;              // Whether or not the player is grounded.
    private Transform CeilingCheck;     // A position marking where to check for ceilings.
    const float CeilingRadius = .01f;   // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D k_Rigidbody2D;  // The Rigidbody2D component attached to the player GameObject.
    bool FacingRight = true;            // For determining which way the player is curently facing.

    public float Damage = 10f;
    public float attackRepeatTime = 1f;
    public float attackTime;

    Animator k_Animator;

	public void ImposedAwake() 
    {
        // Setting up references
        GroundCheck = transform.Find("GroundCheck");
        CeilingCheck = transform.Find("CeilingCheck");
        k_Rigidbody2D = GetComponent<Rigidbody2D>();
        k_Animator = GetComponent<Animator>();
	}
	
	public void ImposedFixedUpdate() 
    {
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                Grounded = true;
        }

        k_Animator.SetBool("Grounded", Grounded);
	}

    void Start()
    {
        attackTime = Time.time;
    }

    public void LookAt(Vector2 target)
    {
        if(transform.position.x > target.x && !FacingRight)
        {
            Flip();
        }
        else if(transform.position.x < target.x && FacingRight)
        {
            Flip();
        }
    }

    public void Move(Vector3 move)
    {

        float moveHorizontal = move.magnitude;
        moveHorizontal = (move.x > 0 ? moveHorizontal : moveHorizontal * -1);

        k_Rigidbody2D.velocity = new Vector2(moveHorizontal * m_MaxSpeed, k_Rigidbody2D.velocity.y);

        if (moveHorizontal > 0)
            k_Animator.SetFloat("Speed", moveHorizontal);
        else
            k_Animator.SetFloat("Speed", -moveHorizontal);
       
        if (Grounded)
        {
            if (move.y > 0)
            {
                k_Animator.SetBool("Jump", true);
                if (move.x > 0)
                {
                    if (move.y >= move.x)
                    {
                        k_Rigidbody2D.velocity = Vector2.zero;
                        k_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    }
                }
                else if (move.x < 0)
                {
                    if (move.y >= -move.x)
                    {
                        k_Rigidbody2D.velocity = Vector2.zero;
                        k_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    }
                }
            }
            else
            {
                k_Animator.SetBool("Jump", false);
            }
        }
        else
        {
            StopHittingTheWall(); // You dummie
        }
    }

    void StopHittingTheWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(GroundCheck.position, transform.forward - transform.position, m_MaxDistanceToAWall, m_WhatIsGround);

        if(hit.collider != null)
        {
            k_Rigidbody2D.velocity = new Vector2(0f, k_Rigidbody2D.velocity.y);
        }
    }

    public void Attack(Transform target)
    {
        if (Time.time > attackTime)
        {
            target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
            attackTime = Time.time + attackRepeatTime;
        }
    }

    void Flip()
    {
        FacingRight = !FacingRight;

        Vector2 tempLocalScale = transform.localScale;
        tempLocalScale.x *= -1;
        transform.localScale = tempLocalScale;
    }
}
