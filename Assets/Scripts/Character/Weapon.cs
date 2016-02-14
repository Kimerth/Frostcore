using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour 
{
    [SerializeField] private float m_FireRate;
    [SerializeField] private float m_Damage;
    [SerializeField] private LayerMask m_WhatToHit;

    [SerializeField] private Transform k_BulletTrailPrefab;
    [SerializeField] private Transform k_HitPrefab;
    [SerializeField] private Transform k_MuzzleFlashPrefab;
    float timeToSpawnEffect;
    [SerializeField] private float effectSpawnRate = 10;

    private float camShakeAmt = 0.05f;
    private float camShakeLength = 0.1f;

    float timeToFire = 0;
    Transform firePoint;

    void Awake()
    {
        firePoint = transform.FindChild("FirePoint");

        if (firePoint == null)
        {
            Debug.LogError("No FirePoint child found on this GameObject");
        }
    }

	void Update() 
    {
        if (m_FireRate == 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }

        else
        {
            if(Input.GetButton("Fire1") && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / m_FireRate;
                Shoot();
            }
        }
	}

    void Shoot()
    {
        Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, mousePosition - firePointPosition, 100, m_WhatToHit);

        if (hit.collider != null)
        {
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyDamage(m_Damage);
            }
        }

        if (Time.time > timeToSpawnEffect)
        {
            Vector3 hitPos;
            Vector3 hitNormal;

            if(hit.collider == null)
            {
                hitPos = (mousePosition - firePointPosition) * 30;
                hitNormal = new Vector3(9999, 9999, 9999);
            }
            else
            {
                hitPos = hit.point;
                hitNormal = hit.normal;
            }

            Effect(hitPos, hitNormal);
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

    void Effect(Vector3 hitPos, Vector3 hitNormal)
    {
        Transform trail = Instantiate(k_BulletTrailPrefab, firePoint.position, firePoint.rotation) as Transform;
        LineRenderer lr = trail.GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, hitPos);
        }

        Destroy(trail.gameObject, 0.04f);

        if (hitNormal != new Vector3(9999, 9999, 9999))
        {
            Transform hitParticle = Instantiate(k_HitPrefab, hitPos, Quaternion.FromToRotation(Vector3.right, hitNormal)) as Transform;
            Destroy(hitParticle.gameObject, 1f);
        }

        Transform clone = Instantiate(k_MuzzleFlashPrefab, firePoint.position, firePoint.rotation) as Transform;
        clone.parent = firePoint;

        float size = Random.Range(0.6f, 0.9f);
        clone.localScale = new Vector3(size, size, size);

        Destroy(clone.gameObject, 0.02f);

        // Shake the camera
        GameMaster.gm.camShake.Shake(camShakeAmt, camShakeLength);
    }
}
