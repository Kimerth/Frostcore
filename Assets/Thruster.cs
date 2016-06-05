using UnityEngine;
using System.Collections;

public class Thruster : MonoBehaviour
{
    public float EnergyConsumptionPerSecond = 100;
    public float MaxUpwardsSpeed = 5f;

    void Start()
    {
        P2D_Motor.Instance.thruster = this;
    }

    void FixedUpdate()
    {
        if (P2D_Animator.Instance._isFlying)
            GetComponent<ParticleSystem>().Play();
        else
            GetComponent<ParticleSystem>().Stop();
    }
    
    //TODO: to be continued...
}
