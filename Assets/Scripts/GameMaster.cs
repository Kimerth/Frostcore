using UnityEngine;
using System.Collections;
using System.Linq;

public class GameMaster : MonoBehaviour {

    [HideInInspector]
    public static GameMaster gm;

    void Awake()
    {
        gm = this;
    }

    [SerializeField] private GameObject m_PlayerPrefab;

    public Transform m_Player;
    public Transform spawnPoint;
    public float spawnDelay = 2f;
    public Transform spawnPrefab;

    public CameraShake camShake;

    MapGenerator mapGen;

	void Start() 
    {
        if (camShake == null)
        {
            camShake = GetComponent<CameraShake>();
            if (camShake == null)
            {
                Debug.LogError("No CameraShake component on _GameMaster");
            }
        }

        mapGen = GetComponent<MapGenerator>();
        mapGen.GenerateMap();
        ShowThyMap(mapGen.width, mapGen.height);
        mapGen.GenerateMap();
        MakeSureDONTDefyTheGravity();

        for (int i = 0; i < 5; i++)
            mapGen.SmoothMap();
        
        ShowThyMap(mapGen.width, mapGen.height, -1, true);
	}

    void ShowThyMap(int width, int height, int AddToHeight = 0, bool negative = false)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!negative)
                    Instantiate(Resources.Load("Prefabs/" + mapGen.map[i, j].ToString(), typeof(GameObject)), new Vector3((float)i / 2, (float)-j / 2 + AddToHeight), Quaternion.identity);
                else
                {
                    Instantiate(Resources.Load("Prefabs/" + mapGen.map[i, j].ToString(), typeof(GameObject)), new Vector3((float)i / 2, (float)j / 2 + AddToHeight), Quaternion.identity);
                }
            }
        }
    }

    void MakeSureDONTDefyTheGravity()
    {
        for (int x = 0; x < mapGen.width; x++)
        {
            bool fly = false;
            for (int y = 0; y < mapGen.height; y++)
            {
                if (mapGen.map[x, y] == 0)
                    fly = true;

                if (fly == true)
                    mapGen.map[x, y] = 0;
            }
        }
    }

    IEnumerator _RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);
        Transform clonePlayer = Instantiate(m_PlayerPrefab, spawnPoint.position, spawnPoint.rotation) as Transform;

        m_Player.GetComponent<P2D_Motor>().Reset();

        GameObject cloneEffect = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        Destroy(cloneEffect, 3f);
    }

    public static void KillPlayer(Player player)
    {
        Destroy(player.gameObject);
        gm.m_Player = null;
        gm.StartCoroutine(gm._RespawnPlayer());
    }

    public static void KillEnemy(Enemy enemy)
    {
        gm._KillEnemy(enemy);
    }

    void _KillEnemy(Enemy _enemy)
    {
        Destroy(_enemy.gameObject);
    }
}
