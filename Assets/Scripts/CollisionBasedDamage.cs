using UnityEngine;
using System.Collections;

public class CollisionBasedDamage : MonoBehaviour 
{
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_KnockbackSpeed;

    public Player thisPlayers = null;
    public Enemy thisEnemys = null;

    void Start()
    {
        SendMessageUpwards("AttachCollisionBasedDamage", this, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Enemy" || theCollider.gameObject.tag == "Player")
        {
            object[] tempStorage = new object[2];
            tempStorage[0] = m_Damage;
            tempStorage[1] = this;
            theCollider.gameObject.SendMessage("ApplyDamage", tempStorage, SendMessageOptions.DontRequireReceiver);

            tempStorage = new object[2];
            tempStorage[0] = 0.25f;
            tempStorage[1] = true;
            if (theCollider.gameObject.layer == LayerMask.NameToLayer("EnergyShield"))
            {
                if (thisPlayers != null)
                {
                    thisPlayers.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    thisPlayers.gameObject.GetComponent<Rigidbody2D>().velocity = ((transform.position - theCollider.transform.position + Vector3.up).normalized * m_KnockbackSpeed * 2);
                }
                else if (thisEnemys != null)
                {
                    thisEnemys.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    thisEnemys.gameObject.GetComponent<Rigidbody2D>().velocity = ((transform.position - theCollider.transform.position + Vector3.up).normalized * m_KnockbackSpeed * 2);
                }
                SendMessageUpwards("Stagger", tempStorage, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                theCollider.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                theCollider.GetComponent<Rigidbody2D>().velocity = ((theCollider.transform.position - transform.position + Vector3.up).normalized * m_KnockbackSpeed);
                theCollider.gameObject.SendMessage("Stagger", tempStorage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
