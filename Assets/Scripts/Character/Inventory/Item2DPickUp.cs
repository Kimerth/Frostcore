using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Item2DPickUp : MonoBehaviour
{
    public float Range = 10;

    void Awake()
    {
        GetComponent<CircleCollider2D>().radius = Range;
    }

    void OnTriggerEnter2D(Collider2D theCollider)
    {
        if (theCollider.gameObject.tag == "Player")
        {
            transform.parent.GetComponent<Item>().PickUpItem();
            gameObject.SetActive(false);
        }
    }
}
