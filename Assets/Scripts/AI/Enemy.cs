using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour 
{
    [System.Serializable]
    public class EnemyStats
    {
        public float maxHP;
        private float _curHP;
        public float curHP
        {
            get { return _curHP; }
            set { _curHP = Mathf.Clamp(value, 0, maxHP); }
        }

        public void ResetHP()
        {
            curHP = maxHP;
        }
    }

    public EnemyStats eStats;

	void Start () 
    {
        eStats.ResetHP();
	}

    public void AttachCollisionBasedDamage(CollisionBasedDamage theSource)
    {
        theSource.thisEnemys = this;
    }

    public RectTransform HpBar;

	void Update () 
    {
        HpBar.localScale = new Vector3(eStats.curHP / eStats.maxHP, 1, 1);
	}

    List<CollisionBasedDamage> ignoreSources = new List<CollisionBasedDamage>();

    public void ApplyDamage(object[] tempStorage)
    {
        float damage = (float)tempStorage[0];
        CollisionBasedDamage source = (CollisionBasedDamage)tempStorage[1];

        if (source != null)
        {
            ignoreSources.Add(source);
            StartCoroutine(resetIgnoreSource(source));
        }

        eStats.curHP -= damage;

        if (eStats.curHP == 0)
        {
            GameMaster.KillEnemy(this);
        }
    }

    IEnumerator resetIgnoreSource(CollisionBasedDamage source)
    {
        yield return new WaitForSeconds(0.5f);

        ignoreSources.Remove(source);
    }
}
