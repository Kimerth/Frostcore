using UnityEngine;
using System.Collections;

public class CollisionBasedDamage : MonoBehaviour 
{
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_KnockbackForce;

    void OnCollisionEnter2D(Collision2D theCollider)
    {
        if (theCollider.gameObject.tag == "Enemy")
        {
            theCollider.gameObject.SendMessage("ApplyDamage", m_Damage, SendMessageOptions.DontRequireReceiver);

            theCollider.rigidbody.AddForce(theCollider.contacts[0].normal.normalized * m_KnockbackForce * -1);
        }
    }
}
