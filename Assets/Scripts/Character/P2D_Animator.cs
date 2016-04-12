using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class P2D_Animator : MonoBehaviour 
{
    public static P2D_Animator Instance;

    private Animator m_Animator;

    public float moveSpeed;

    void Awake()
    {
        Instance = this;

        m_Animator = GetComponent<Animator>();
    }

	public void ImposedFixedUpdate() 
    {
        if (moveSpeed < 0)
            m_Animator.SetBool("GoingLeft", true);
        else
            m_Animator.SetBool("GoingLeft", false);

        moveSpeed = Mathf.Abs(moveSpeed);

        m_Animator.SetFloat("MoveSpeed", moveSpeed);
        m_Animator.SetBool("Grounded", P2D_Motor.Instance.Grounded);
        if (!P2D_Motor.Instance.Grounded)
        {
            SetStateJump(false);
        }

        if(moveSpeed == 0)
        {
            return;
        }

        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            m_Animator.speed = Mathf.Clamp(moveSpeed / 8, 1, moveSpeed);
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Sprint"))
        {
            m_Animator.speed = Mathf.Clamp(moveSpeed / 12, 1, moveSpeed);
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchMove"))
        {
            m_Animator.speed = Mathf.Clamp(moveSpeed / 10, 1, moveSpeed);
        }
	}

    public void SetStateCrouch(bool crouch)
    {
        m_Animator.SetBool("Crouch", crouch);
    }

    public void SetStateJump(bool jump)
    {
        m_Animator.SetBool("Jump", jump);
    }

    public void SetStateSprint(bool sprint)
    {
        m_Animator.SetBool("Sprint", sprint);
    }

    public void HoldGun(bool holdGun)
    {
        m_Animator.SetBool("HoldGun", holdGun);
        if (holdGun)
            ArmRotation.Instance.UpdateArmRotation();
        else
            ArmRotation.Instance.UpdateArmRotation(true);
    }

    public void FacingRight(bool Right)
    {
        m_Animator.SetBool("FacingRight", Right);
    }

    public void Attack(bool value = true)
    {
        m_Animator.SetBool("Attack", value);

        if(value)
            ArmRotation.Instance.UpdateArmRotation();
        else
        {
            ArmRotation.Instance.UpdateArmRotation(true);
        }
    }
}
