using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyShield : MonoBehaviour
{
    public float damageBlockEfficiency = 1f;
    public float TurnOnEnegy = 50f;

    List<CollisionBasedDamage> ignoreSources = new List<CollisionBasedDamage>();

    void Start()
    {
        Player.Instance.shield = this;
    }

    public void ApplyDamage(object[] tempStorage)
    {
        float damage = (float)tempStorage[0];
        CollisionBasedDamage source = tempStorage[1] as CollisionBasedDamage;

        if (source != null)
            if (ignoreSources.Contains(source))
                return;

        if (source != null)
        {
            ignoreSources.Add(source);
            StartCoroutine(resetIgnoreSource(source));
        }

        if(Player.Instance.pStats.energyCells.virtualTotalEnergy >= damage / damageBlockEfficiency)
        {
            StartCoroutine(Player.Instance.pStats.energyCells.DrainEnergy(damage / damageBlockEfficiency, 0.1f));
        }
        else
        {
            if(Player.Instance.pStats.energyCells.virtualTotalEnergy >= 1 / damageBlockEfficiency)
            {
                StartCoroutine(Player.Instance.pStats.energyCells.DrainEnergy(Player.Instance.pStats.energyCells.TotalEnergy, 0.1f));
            }

            gameObject.SetActive(false);
        }
    }

    IEnumerator resetIgnoreSource(CollisionBasedDamage source)
    {
        yield return new WaitForSeconds(0.5f);

        ignoreSources.Remove(source);
    }
}

