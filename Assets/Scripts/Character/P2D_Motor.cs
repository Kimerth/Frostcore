using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class P2D_Motor : MonoBehaviour 
{
    public static P2D_Motor Instance;

    /// <summary>
    /// Player max move speed on X axis
    /// </summary>
    [SerializeField] private float m_Speed = 10f;                    
    /// <summary>
    /// Player speed multiplier applied when sprinting
    /// </summary>
    [Range(1, 10)][SerializeField] private float m_SprintSpeed = 1.5f;
    /// <summary>
    /// How much stamina will be lost each second while sprint is active
    /// </summary>
    [SerializeField] private float m_SprintStaminaCost;
    /// <summary>
    /// How many times per second stamina will be lost
    /// </summary>
    [SerializeField] private int sprintStaminaCostRate;
    /// <summary>
    /// Amount of force added when the player jumps
    /// </summary>
    [SerializeField] private float m_JumpForce = 400f;
    /// <summary>
    /// How much stamina will be lost when you jump
    /// </summary>
    [SerializeField] private float m_JumpStaminaCost;
    /// <summary>
    /// How fast will the player move while dashing
    /// </summary>
    [SerializeField] private float m_DashSpeed = 40f;                   
    /// <summary>
    /// How long will the player dash
    /// </summary>
    [SerializeField] private float m_DashLenght = 0.02f;  
    /// <summary>
    /// How much stamina will be lost at dash
    /// </summary>
    [SerializeField] private float m_DashStaminaCost;
    /// <summary>
    /// Amount of maxSpeed applied to crouching movement. 1 = 100%
    /// </summary>
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;   
    /// <summary>
    /// Whether or not a player can steer while jumping
    /// </summary>
    [SerializeField] private bool m_AirControl = false;                 
    /// <summary>
    /// A mask determining what is ground to the character
    /// </summary>
    [SerializeField] private LayerMask m_WhatIsGround;                  

    /// <summary>
    /// A position marking where to check if the player is grounded
    /// </summary>
    public Transform GroundCheck;      
    /// <summary>
    ///  Radius of the overlap circle to determine if grounded
    /// </summary>
    const float GroundedRadius = .3f;   
    /// <summary>
    /// Whether or not the player is grounded
    /// </summary>
    public bool Grounded;
    private bool LastCheckGrounded;
    /// <summary>
    /// A position marking where to check for ceilings
    /// </summary>
    public Transform CeilingCheck;     
    /// <summary>
    /// Radius of the overlap circle to determine if the player can stand up
    /// </summary>
    const float CeilingRadius = .01f;   
    /// <summary>
    /// The Rigidbody2D component attached to the player GameObject
    /// </summary>
    private Rigidbody2D k_Rigidbody2D; 
    /// <summary>
    /// For determining which way the player is curently facing
    /// </summary>
    public bool FacingRight = true;            
    /// <summary>
    /// For determining wheather or not the player is dashing
    /// </summary>
    public bool IsDashing = false;             
    /// <summary>
    /// To check for how long should the player dash
    /// </summary>
    float dashTime;
    float timeToDrainSprintStamina;
    
    /// <summary>
    /// Stores the Transform component of the Arm attached to the player
    /// </summary>
    private Transform Graphics;                         

    /// <summary>
    /// Setting up references
    /// </summary>
	public void Awake() 
    {
        Instance = this;

        k_Rigidbody2D = GetComponent<Rigidbody2D>();
	}

    void Start()
    {
        Graphics = transform.FindChild("Graphics");
        timeToDrainSprintStamina = Time.time;
    }

    public void ImposedUpdate()
    {
        if (P2D_Animator.Instance._HoldGun)
        {
            if (ArmRotation.Instance.rotZ > 90 || ArmRotation.Instance.rotZ < -90)
                Flip();
        }

        if (P2D_Animator.Instance._isSprinting)
        {
            if (timeToDrainSprintStamina < Time.time)
            {
                Player.Instance.DrainStamina(m_SprintStaminaCost / sprintStaminaCostRate);
                timeToDrainSprintStamina = Time.time + 1 / sprintStaminaCostRate;
            }
            Player.Instance.DrainStamina(0);
        }

        if (Time.time > dashTime && IsDashing)
        {
            k_Rigidbody2D.velocity = Vector2.zero;
            IsDashing = false;
            gameObject.layer = 8;
            return;
        }
    }
	
	public void ImposedFixedUpdate() 
    {
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject != gameObject)
            {
                Grounded = true;
            }
        }

        if(IsDashing)
            k_Rigidbody2D.velocity = new Vector2(Mathf.Clamp(k_Rigidbody2D.velocity.x, -1f, 1f) * m_DashSpeed, k_Rigidbody2D.velocity.y);
    }

    private bool IsStaggered = false;

    public void Move(float move, bool jump, bool crouch, bool sprint)
    {
        // If dashing, don't do anything
        if (IsDashing)
            return;

        if (IsStaggered)
            return;

        // If crouching, check to see if the player can stand up.
        if (!crouch)
        {
            if (Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        P2D_Animator.Instance.SetStateCrouch(crouch);

        // Only control the player if grounded or air control is true.
        if (Grounded || m_AirControl)
        {
            // Reduce the speed if crouching by the crouchSpeed multiplier
            move = (crouch ? move * m_CrouchSpeed : move);

            // Multiply the speed if sprinting by the sprintSpeed multiplier
            move = (sprint ? move * m_SprintSpeed : move);

            P2D_Animator.Instance.SetStateSprint(sprint);

            // Multiply the speed by MaxSpeed
            move *= m_Speed;

            // Move the character
            if (!LastCheckGrounded)
                k_Rigidbody2D.velocity = new Vector2(move, k_Rigidbody2D.velocity.y);
            else
                k_Rigidbody2D.velocity = new Vector2(move, k_Rigidbody2D.velocity.y / 10);

            if (!P2D_Animator.Instance._HoldGun)
                if (move == 0)
                { }
                else if (Mathf.Sign(move) == 1 && !FacingRight)
                    Flip();
                else if (Mathf.Sign(move) == -1 && FacingRight)
                    Flip();

            P2D_Animator.Instance.moveSpeed = move;
        }

        // If the player should jump...
        if (Grounded && jump)
        {
            // add a vertical force to the player.
            Grounded = false;
            k_Rigidbody2D.velocity = new Vector2(k_Rigidbody2D.velocity.x, 0);
            k_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            Player.Instance.DrainStamina(m_JumpStaminaCost);
            P2D_Animator.Instance.SetStateJump(jump);
        }

        LastCheckGrounded = Grounded;
    }

    public IEnumerator Stagger(float duration)
    {
        IsStaggered = true;

        k_Rigidbody2D.velocity = new Vector2(k_Rigidbody2D.velocity.x / 10, k_Rigidbody2D.velocity.y / 10);

        var stopTime = Time.time + duration;

        while(stopTime > Time.time)
        {
            yield return null;
        }

        IsStaggered = false;
    }

    public void Dash(bool dash)
    {
        if (!dash || IsDashing)
            return;

        IsDashing = true;
        gameObject.layer = 11;
        dashTime = Time.time + m_DashLenght;

        Player.Instance.DrainStamina(m_DashStaminaCost);
    }

    void Flip()
    {
        // Switch the way the player is labeled as facing
        FacingRight = !FacingRight;

        Vector3 tempPlayerLocalScale = transform.localScale;
        tempPlayerLocalScale.x *= -1;
        transform.localScale = tempPlayerLocalScale;
    }

    public void Reset()
    {
        if (transform.localScale.x < 0)
        {
            Vector3 tempPlayerLocalScale = transform.localScale;
            tempPlayerLocalScale.x *= -1;
            transform.localScale = tempPlayerLocalScale;
        }

        FacingRight = true;
    }
}
