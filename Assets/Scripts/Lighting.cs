using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    public float Power; // Number of blocks that you can see thru
    public LayerMask discrimination;
    Vector2 dir;
    public float checkAngle = 2f;
    Quaternion angle;
    public float Distance = 20f;

    public float AirAffection = .1f;

    void Start()
    {
        angle = new Quaternion();
    }

    void Update()
    {
        angle.eulerAngles = Vector3.zero;

        float monotony = 1 / Power;
        for (int i = 0; i < 360 / checkAngle; i++)
        {
            try
            {
                RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, angle * Vector2.up, Distance, discrimination);
                float modifier = 0;
                foreach (RaycastHit2D hitmark in hit)
                {
                    if (modifier <= Power)
                    {
                        if (hitmark.transform.tag == "Air")
                        {
                            if (hitmark.transform.GetComponent<SpriteRenderer>().color != new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, monotony * modifier))
                                hitmark.transform.GetComponent<SpriteRenderer>().color = new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, monotony * modifier);
                            modifier += AirAffection;
                        }
                        else if(hitmark.transform.tag == "Breakable")
                        {
                            if (hitmark.transform.GetComponent<SpriteRenderer>().color != new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, 1))
                                hitmark.transform.GetComponent<SpriteRenderer>().color = new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, 1);
                            modifier++;
                        }
                        else
                        {
                            if (hitmark.transform.GetComponent<SpriteRenderer>().color != Color.white)
                                hitmark.transform.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    else
                    {
                        hitmark.transform.GetComponent<SpriteRenderer>().color = Color.black;
                    }
                }
            }
            catch { }

            angle.eulerAngles = new Vector3(0, 0, angle.eulerAngles.z + checkAngle);
        }
    }

    void OnTriggerExit2D(Collider2D theCollider)
    {
        theCollider.GetComponent<SpriteRenderer>().color = Color.black;
    }
}
