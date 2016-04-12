using UnityEngine;
using System.Collections;

public class ArmRotation : MonoBehaviour 
{
    public static ArmRotation Instance;

    public float rotationOffset = 45;

    public float rotZ;
    private bool updateRotation;

    void Awake()
    {
        Instance = this;
    }

	void LateUpdate()
    {
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();

        rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (!P2D_Motor.Instance.FacingRight)
        {
            rotZ = (rotZ < 0 ? rotZ - (2 * (rotZ + 90)) : rotZ - (2 *( rotZ - 90)));
        }

        if(updateRotation)
            transform.localRotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
	}

    public void UpdateArmRotation(bool reset = false)
    {
        if (reset)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            updateRotation = false;
        }
        else
        {
            updateRotation = true;
        }
    }
}
