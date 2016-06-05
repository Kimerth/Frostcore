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
    const float GroundedRadius = 1f;   // Radius of the overlap circle to determine if grounded.
    private bool Grounded;              // Whether or not the player is grounded.
    private Transform CeilingCheck;     // A position marking where to check for ceilings.
    const float CeilingRadius = .01f;   // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D k_Rigidbody2D;  // The Rigidbody2D component attached to the player GameObject.
    bool FacingRight = true;            // For determining which way the player is curently facing.

    public float Damage = 10f;
    public float attackRepeatTime = 1f;
    public float attackTime;

    public Transform[] AttackPoints;

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

    public bool IsStaggered;

    public void Move(Vector3 move)
    {
        if (IsStaggered)
            return;

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

    public void Stagger(object[] tempStorage)
    {
        float duration = (float)tempStorage[0];
        bool ragdoll = (bool)tempStorage[1];

        StartCoroutine(Attack(0, duration));

        StartCoroutine(_Stagger(duration, ragdoll));
    }

    IEnumerator _Stagger(float duration, bool ragdoll)
    {
        IsStaggered = true;

        yield return null;

        Collider2D[] colliders = null;
        if (ragdoll)
        {
            colliders = GetComponents<Collider2D>();
            foreach (Collider2D coll in colliders)
                coll.sharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicsMaterials/Slippery");
        }

        var stopTime = Time.time + duration;

        while (stopTime > Time.time)
            yield return null;

        if (ragdoll)
        {
            foreach (Collider2D coll in colliders)
                coll.sharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicsMaterials/Slippery");
        }

        IsStaggered = false;
    }

    public void MakeItBouncy()
    {
        StartCoroutine(_MakeItBouncy());
    }

    IEnumerator _MakeItBouncy()
    {
        Collider2D[] colliders = null;
        colliders = GetComponents<Collider2D>();
        foreach (Collider2D coll in colliders)
            coll.sharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicsMaterials/BouncyBox");

        yield return null;

        foreach (Collider2D coll in colliders)
            coll.sharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicsMaterials/Slippery");
    }

    public bool IsAttacking;
    public bool HasAttacked;
    public IEnumerator Attack(float timeAmount, float coolDown)
    {
        IsAttacking = true;

        foreach (Transform point in AttackPoints)
            point.gameObject.SetActive(true);

        yield return new WaitForSeconds(timeAmount);

        IsAttacking = false;
        HasAttacked = true;

        foreach (Transform point in AttackPoints)
            point.gameObject.SetActive(false);

        yield return new WaitForSeconds(coolDown);
        HasAttacked = false;
    }

    void StopHittingTheWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(GroundCheck.position, transform.forward - transform.position, m_MaxDistanceToAWall, m_WhatIsGround);

        if(hit.collider != null)
        {
            k_Rigidbody2D.velocity = new Vector2(0f, k_Rigidbody2D.velocity.y);
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
