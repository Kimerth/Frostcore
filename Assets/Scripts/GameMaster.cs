using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameMaster : MonoBehaviour {

    [HideInInspector]
    public static GameMaster gm;

    void Awake()
    {
        gm = this;
    }

    public bool HasGameStarted = false;

    [SerializeField] private GameObject m_PlayerPrefab;

    public Transform m_Player;
    public Transform spawnPoint;
    public float spawnDelay = 2f;
    public Transform spawnPrefab;

    public float mobSpawnDelay = 20f;
    float lastSpawn;

    public GameObject Enemy;

    public Transform mobSpawn1;
    public Transform mobSpawn2;
    public Transform mobSpawn3;
    public Transform mobSpawn4;

    public CameraShake camShake;

    public GameObject Hotbar;
    public GameObject PlayerInfo;
    public GameObject FPSCounter;
    public bool ShowFpsCounter;
    public GameObject MainMenu;
    public bool isMenuActive = false;

    public GameObject LoadingScreen;
    public bool gameIsStarting = false;
    public bool Generating;
    public int numberOfGens = 0;

    public MapGenerator mapGen;
    public Node[][] Map;

    int cameraCoordX;
    int cameraCoordY;

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
        MainMenu.GetComponent<Animator>().SetBool("Active", true);
        mapLight = new MapLight();

        mainCam = Camera.main;

        preRenderedLightOfSources = new List<PreRenderedLight>();
	}

    #region StartGame

    public void StartGame()
    {
        if (gameIsStarting)
            return;

        Map = new Node[mapGen.width][];
        for (int i = 0; i < Map.GetLength(0); i++)
            Map[i] = new Node[mapGen.height * 2];

        if (!LoadingScreen.active)
        {
            LoadingScreen.SetActive(true);
            MainMenu.SetActive(false);
            Invoke("StartGame", 0.01f);
            return;
        }

        gameIsStarting = true;
        StartCoroutine(StartingGame());
    }

    public struct Node
    {
        public int id;
        public string name;
        public bool shown;
        public GameObject clone;
        public SpriteRenderer spriteRenderer;
        public float blockingAmount;
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int cx, int cy)
        {
            x = cx;
            y = cy;
        }
    }

    IEnumerator StartingGame()
    {
        if (!gameIsStarting)
            yield break;

        if (numberOfGens == 0)
        {
            StartCoroutine(mapGen.GenerateMap());
            for (; ; )
                if (Generating)
                    yield return null;
                else break;

            for (int i = 0; i < mapGen.width; i++)
            {
                for (int j = mapGen.height; j < mapGen.height * 2; j++)
                {
                    Map[i][j].id = mapGen.map[i, j - mapGen.height];
                }
            }

            PlaceSpawn(mobSpawn3);
            PlaceSpawn(mobSpawn4);
        }

        if (numberOfGens == 1 && !Generating)
        {
            StartCoroutine(mapGen.GenerateMap());
            for (; ; )
                if (Generating)
                    yield return null;
                else break;

            MakeSureDONTDefyTheGravity();

            for (int i = 0; i < 5; i++)
                mapGen.SmoothMap();

            for (int i = 0; i < mapGen.width; i++)
            {
                for (int j = 0; j < mapGen.height; j++)
                {
                    Map[i][j].id = mapGen.map[i, j];
                }
            }
        }

        StartCoroutine(ShowThyMap());
        for (; ; )
            if (isShowingMap)
                yield return null;
            else break;

        lastSpawn = Time.time + spawnDelay;

        yield return new WaitForSeconds(1f);

        LightMesh.Instance.GenerateMesh(0.5f);
        colors = new Color[LightMesh.Instance.mesh.vertices.Length];

        LightMesh.Instance.mesh.colors = colors;

        yield return new WaitForSeconds(1f);

        PlaceSpawn(spawnPoint);
        PlaceSpawn(mobSpawn1);
        PlaceSpawn(mobSpawn2);
        StartCoroutine(_RespawnPlayer());


        gameIsStarting = false;
        HasGameStarted = true;

        Invoke("MakeMenuInactive", .1f);
        isMenuActive = false;

        LoadingScreen.SetActive(false);
        GameObject frostEff = GameObject.Find("FrostEffect");
        frostEff.SetActive(false);
        frostEff.SetActive(true);

        yield break;
    }

    void MakeSureDONTDefyTheGravity()
    {
        for (int x = 0; x < mapGen.width; x++)
        {
            bool fly = false;
            for (int y = mapGen.height - 1; y >= 0; y--)
            {
                if (mapGen.map[x, y] == 0)
                    fly = true;

                if (fly == true)
                    mapGen.map[x, y] = 0;
            }
        }
    }

    bool isShowingMap = false;

    IEnumerator ShowThyMap()
    {
        isShowingMap = true;
        for (int i = 0; i < mapGen.width; i++)
        {
            for (int j = 0; j < mapGen.height * 2; j++)
            {
                if (Map[i][j].id == 0)
                    continue;

                GameObject clone;

                if (j > mapGen.height)
                    clone = Instantiate(Resources.Load("Prefabs/" + Map[i][j].id.ToString(), typeof(GameObject)), new Vector3((float)i / 2, (float)-(j - mapGen.height) / 2), Quaternion.identity) as GameObject;
                else
                    clone = Instantiate(Resources.Load("Prefabs/" + Map[i][j].id.ToString(), typeof(GameObject)), new Vector3((float)i / 2, mapGen.height / 2 - (float)j / 2), Quaternion.identity) as GameObject;

                Map[i][j].clone = clone;
                Map[i][j].spriteRenderer = clone.GetComponent<SpriteRenderer>();
                Map[i][j].shown = true;
                if (Map[i][j].clone.tag != "Air")
                    Map[i][j].blockingAmount = 0.10f;
            }
            if (i % (int)(1000 / mapGen.height * 2) == 0)
                yield return null;
        }
        isShowingMap = false;
    }

    void PlaceSpawn(Transform spawn, bool negativeY = false)
    {
        int x = Random.Range(10, mapGen.width - 10);
        for (int y = 0; y < mapGen.height; y++)
        {
            if (mapGen.map[x, y] == 0)
            {
                if (!negativeY)
                    spawn.position = new Vector2(x / 4, (y + 8) / 4);
                else
                    spawn.position = new Vector2(x / 4, (-y + 8) / 4);

            }
        }
    }

    void SpawnMobs()
    {
        Instantiate(Enemy, mobSpawn1.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn2.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn3.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn4.position, mobSpawn1.rotation);

        lastSpawn = Time.time + mobSpawnDelay;
    }

    #endregion

    public Camera mainCam;

    void Update()
    {
        if (!HasGameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            if (isMenuActive)
            {
                Time.timeScale = 1f;
                MainMenu.GetComponent<Animator>().SetBool("Active", false);
                Invoke("MakeMenuInactive", .1f);
                isMenuActive = false;
            }
            else
            {
                MainMenu.SetActive(true);
                MainMenu.GetComponent<Animator>().SetBool("Active", true);
                isMenuActive = true;

                MainMenu.GetComponent<MainMenuButtons>().GoToMainPage();
                MainMenu.transform.FindChild("Background").GetComponent<Canvas>().sortingOrder = -2;

                PlayerInfo.SetActive(false);
                InventoryDisplay.Instance.InventoryUIReference.gameObject.SetActive(false);
                InventoryDisplay.Instance.displayInventory = false;
                Inventory.Instance.draggedItem.ClearDraggedItem();
                Hotbar.SetActive(false);

                if (ShowFpsCounter)
                    FPSCounter.SetActive(false);

                Time.timeScale = 0f;
            }

        if (isMenuActive)
            return;

        cameraCoordX = (int)(mainCam.transform.position.x * 2);
        cameraCoordY = (int)(mainCam.transform.position.y * 2);

        if (cameraCoordY >= 0)
            cameraCoordY = mapGen.height - cameraCoordY;
        else
        {
            cameraCoordY *= -1;
            cameraCoordY += mapGen.height;
        }

        mapLight.ApplyLight();

        if (Time.time > lastSpawn)
            SpawnMobs();
    }

    public bool isValidPosition(Coord coord)
    {
        return coord.x >= 0 && coord.x < mapGen.width && coord.y >= 0 && coord.y < mapGen.height * 2;
    }

    public bool isValidPositionNC(int coordX, int coordY)
    {
        return coordX >= 0 && coordX < mapGen.width && coordY >= 0 && coordY < mapGen.height * 2;
    }

    void MakeMenuInactive()
    {
        MainMenu.SetActive(false);

        PlayerInfo.SetActive(true);
        Hotbar.SetActive(true);

        if (ShowFpsCounter)
            FPSCounter.SetActive(true);
    }

    #region LightingSystem

    public IEnumerator UpdateLight(LightSource source)
    {
        int frame = 0;

        for (; ; )
        {
            if(source.markedAsFar)
            {
                if(Mathf.Sqrt(Mathf.Pow(source.MapPosX - cameraCoordX, 2) + Mathf.Pow(source.MapPosY - cameraCoordY, 2)) < 50)
                    source.markedAsFar = false;

                source.UpdateMapPos();

                yield return null;
                continue;
            }

            if (!source.markedAsFar)
            {
                if (Mathf.Sqrt(Mathf.Pow(source.MapPosX - cameraCoordX, 2) + Mathf.Pow(source.MapPosY - cameraCoordY, 2)) > 50)
                {
                    source.markedAsFar = true;

                    if (!source.Stationary)
                        mapLight.ResetDinamicMatrix(source);

                    continue;
                }
            }

            if (frame == 0)
            {
                if (source.Added && source.Stationary)
                    continue;

                source.UpdateMapPos();

                if (source.Stationary)
                {
                    applyLight(source.MapPosX, source.MapPosY, 0, source);
                    source.Added = true;
                }
                else
                {
                    mapLight.ResetDinamicMatrix(source);
                    applyLight(source.MapPosX, source.MapPosY, 0, source, false);
                }

                frame++;
                yield return null;
            }

            if (frame < 5)
            {
                frame++;
                yield return null;
            }

            if (frame == 5)
            {
                frame = 0;
                yield return null;
            }
        }
    }

    public float AirAffection = .1f;

    public List<LightSource> lightSources;
    public List<PreRenderedLight> preRenderedLightOfSources;

    Color[] colors;

    public class MapLight
    {
        public float[][] mapLightLevel;

        public MapLight()
        {
            mapLightLevel = new float[GameMaster.gm.mapGen.width][];

            for (int i = 0; i < mapLightLevel.GetLength(0); i++)
                mapLightLevel[i] = new float[GameMaster.gm.mapGen.height * 2];
        }

        public void SetLight(int coordX, int coordY, float newLight)
        {
            mapLightLevel[coordX][coordY] = newLight;

            if (GetLightBlockingAmountAt(coordX, coordY) == 0.10f)
                GameMaster.gm.Map[coordX][coordY].spriteRenderer.color = new Color(newLight, newLight, newLight);
            else if (GetLightBlockingAmountAt(coordX, coordY) == 0f)
                GameMaster.gm.Map[coordX][coordY].spriteRenderer.color = new Color(newLight, newLight, newLight, 1 - newLight);
        }

        public void DinamicSetLight(int coordX, int coordY, float newLight, LightSource source)
        {
            source.mobileLight[coordX - source.MapPosX + source.mobileLight.SizeX / 2,
                coordY - source.MapPosY + source.mobileLight.SizeY / 2] = newLight;
        }

        public void ResetDinamicMatrix(LightSource source)
        {
            source.mobileLight.Clear();
        }

        public float GetLight(int coordX, int coordY)
        {
            return mapLightLevel[coordX][coordY];
        }

        public float DinamicGetLight(int coordX, int coordY, LightSource source)
        {
            var X = coordX - source.MapPosX + source.mobileLight.SizeX / 2;
            var Y = coordY - source.MapPosY + source.mobileLight.SizeY / 2;

            if (X < 0 || X >= source.mobileLight.SizeX - 1 || Y < 0 || Y >= source.mobileLight.SizeY - 1)
                return 0;

            return source.mobileLight[X, Y];
        }

        public float DinamicGetLightAll(int coordX, int coordY)
        {
            var maxValue = GetLight(coordX, coordY);

            for (int i = 0; i < GameMaster.gm.lightSources.Count; i++) 
            {
                if (GameMaster.gm.lightSources[i].Stationary)
                    continue;

                if (GameMaster.gm.lightSources[i].markedAsFar)
                    continue;

                if (GameMaster.gm.lightSources[i].Power <= maxValue)
                    continue;

                if (maxValue >= 1)
                    return maxValue;

                if (DinamicGetLight(coordX, coordY, GameMaster.gm.lightSources[i]) > maxValue)
                    maxValue = DinamicGetLight(coordX, coordY, GameMaster.gm.lightSources[i]);
            }

            return maxValue;
        }

        public void ApplyLight()
        {
            LightMesh.Instance.UpdateMapPos();

            for (int x = GameMaster.gm.cameraCoordX - 45; x < GameMaster.gm.cameraCoordX + 45; x++)
            {
                for (int y = GameMaster.gm.cameraCoordY - 25; y < GameMaster.gm.cameraCoordY + 25; y++)
                {
                    if (!GameMaster.gm.isValidPositionNC(x ,y))
                        continue;

                    var newLight = DinamicGetLightAll(x, y);
                    GameMaster.gm.colors[(x - LightMesh.Instance.MapPosX + 50) * 50 + y - LightMesh.Instance.MapPosY + 25] = new Color(1 - newLight, 1 - newLight, 1 - newLight, 1 - newLight);
                }
            }

            LightMesh.Instance.mesh.colors = GameMaster.gm.colors;
        }

        public float GetLightBlockingAmountAt(int coordX, int coordY)
        {
            return GameMaster.gm.Map[coordX][coordY].blockingAmount;
        }
    }

    public class PreRenderedLight
    {
        public Unmanaged2DFloatMatrix preRenderedLight;
        public GameMaster.Coord initCoord;
        public float refPower;

        public void PreRender()
        {
            for (int i = 3; i < preRenderedLight.SizeX - 3; i++)
                for (int j = 3; j < preRenderedLight.SizeY - 3; j++)
                {
                    preRenderedLight[i, j] = refPower - Mathf.Sqrt(Mathf.Pow(i - initCoord.x, 2) + Mathf.Pow(j - initCoord.y, 2)) / 15;
                }
        }

        public float GetLightAt(int coordX, int coordY)
        {
            return preRenderedLight[coordX + initCoord.x, coordY + initCoord.y];
        }
    }

    public void PreRenderLight(float Power, LightSource source)
    {
        PreRenderedLight light = new PreRenderedLight();

        if ((int)(lightSources[lightSources.Count - 1].Power % 0.05f) % 2 == 0)
            light.preRenderedLight = new Unmanaged2DFloatMatrix((int)(Power * 40) + 1, (int)(Power * 40) + 1);
        else
            light.preRenderedLight = new Unmanaged2DFloatMatrix((int)(Power * 40), (int)(Power * 40));

        light.initCoord = new GameMaster.Coord((int)((light.preRenderedLight.SizeX - 1) / 2), (int)((light.preRenderedLight.SizeX - 1) / 2));
        light.refPower = Power;
        light.PreRender();

        preRenderedLightOfSources.Add(light);
        source.preRenderedLight = light;
    }

    public MapLight mapLight;
    
    void applyLight(int curCoordX, int curCoordY, float encounteredWallness, LightSource source, bool Stationary = true)
    {
        if (!isValidPositionNC(curCoordX, curCoordY)) return;

        encounteredWallness += mapLight.GetLightBlockingAmountAt(curCoordX, curCoordY);
        float newLight = source.preRenderedLight.GetLightAt(source.MapPosX - curCoordX, source.MapPosY - curCoordY) - encounteredWallness;

        if (newLight <= mapLight.GetLight(curCoordX, curCoordY)) return;

        if (Stationary)
            mapLight.SetLight(curCoordX, curCoordY, newLight);
        else
        {
            if (newLight <= mapLight.DinamicGetLight(curCoordX, curCoordY, source)) return;

            mapLight.DinamicSetLight(curCoordX, curCoordY, newLight, source);

            applyLight(curCoordX + 1, curCoordY, encounteredWallness, source, false);
            applyLight(curCoordX, curCoordY + 1, encounteredWallness, source, false);
            applyLight(curCoordX - 1, curCoordY, encounteredWallness, source, false);
            applyLight(curCoordX, curCoordY - 1, encounteredWallness, source, false);
            return;
        }

        applyLight(curCoordX + 1, curCoordY, encounteredWallness, source);
        applyLight(curCoordX, curCoordY + 1, encounteredWallness, source);
        applyLight(curCoordX - 1, curCoordY, encounteredWallness, source);
        applyLight(curCoordX, curCoordY - 1, encounteredWallness, source);
    }

    #endregion

    #region OldLightingSystem

    /*
    void Lighting(Coord startCoord, int dist, float powr)
    {
        float monotony = 1 / powr;

        float[,] mapFlags = new float[mapGen.width, mapGen.height * 2];
        float[,] tempMapLightLevel = new float[mapGen.width, mapGen.height * 2];

        int[] rowad = { -1, 0, 1, 0 };
        int[] linead = { 0, 1, 0, -1 };

        Queue<Coord> queue = new Queue<Coord>();

        Coord coord;
        Coord auxcoord;

        coord = startCoord;
        queue.Enqueue(coord);
        tempMapLightLevel[coord.x, coord.y] += powr * monotony;

        while (queue.Count != 0)
        {
            if (queue.Count > 0)
                coord = queue.Dequeue();
            if (mapFlags[coord.x, coord.y] > dist)
                continue;

            for (int i = 0; i < 4; i++)
            {
                auxcoord.x = coord.x + linead[i];
                auxcoord.y = coord.y + rowad[i];

                if (auxcoord.x < 0 || auxcoord.x > mapGen.width || auxcoord.y < 0 || auxcoord.y > mapGen.height * 2)
                    continue;

                if (mapFlags[auxcoord.x, auxcoord.y] != 0)
                    continue;

                if (Map[auxcoord.x, auxcoord.y].clone == null)
                    continue;

                mapFlags[auxcoord.x, auxcoord.y] = Mathf.Sqrt(Mathf.Pow(auxcoord.x - startCoord.x, 2) + Mathf.Pow(auxcoord.y - startCoord.y, 2));

                if (Map[auxcoord.x, auxcoord.y].clone.transform.tag == "Air")
                {
                    if (auxcoord.x <= startCoord.x + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.x >= startCoord.x - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) &&
                        auxcoord.y < startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x, auxcoord.y + 1] - monotony * AirAffection;
                    else if (auxcoord.x > startCoord.x &&
                        auxcoord.y <= startCoord.y + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.y >= startCoord.y - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16))
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y] - monotony * AirAffection;
                    else if (auxcoord.x <= startCoord.x + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.x >= startCoord.x - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) &&
                        auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x, auxcoord.y - 1] - monotony * AirAffection;
                    else if (auxcoord.x < startCoord.x &&
                        auxcoord.y <= startCoord.y + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.y >= startCoord.y - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16))
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y] - monotony * AirAffection;
                    else if (auxcoord.x > startCoord.x && auxcoord.y < startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y + 1] - monotony * AirAffection;
                    else if (auxcoord.x > startCoord.x && auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y - 1] - monotony * AirAffection;
                    else if (auxcoord.x < startCoord.x && auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y - 1] - monotony * AirAffection;
                    else
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y + 1] - monotony * AirAffection;
                }
                else if (Map[auxcoord.x, auxcoord.y].clone.transform.tag == "Breakable")
                {
                    if (auxcoord.x <= startCoord.x + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.x >= startCoord.x - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) &&
                        auxcoord.y < startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x, auxcoord.y + 1] - monotony;
                    else if (auxcoord.x > startCoord.x &&
                        auxcoord.y <= startCoord.y + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.y >= startCoord.y - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16))
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y] - monotony;
                    else if (auxcoord.x <= startCoord.x + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.x >= startCoord.x - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) &&
                        auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x, auxcoord.y - 1] - monotony;
                    else if (auxcoord.x < startCoord.x &&
                        auxcoord.y <= startCoord.y + 1 + (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16) && auxcoord.y >= startCoord.y - 1 - (int)(2 * 3.14f * mapFlags[auxcoord.x, auxcoord.y] / 16))
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y] - monotony * AirAffection;
                    else if (auxcoord.x > startCoord.x && auxcoord.y < startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y + 1] - monotony;
                    else if (auxcoord.x > startCoord.x && auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x - 1, auxcoord.y - 1] - monotony;
                    else if (auxcoord.x < startCoord.x && auxcoord.y > startCoord.y)
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y - 1] - monotony;
                    else
                        tempMapLightLevel[auxcoord.x, auxcoord.y] = tempMapLightLevel[auxcoord.x + 1, auxcoord.y + 1] - monotony;
                }

                queue.Enqueue(auxcoord);
            }
            mapLightLevel[coord.x, coord.y] += tempMapLightLevel[coord.x, coord.y];
        }
    }
    */

    #endregion

    IEnumerator _RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);
        m_Player.position = spawnPoint.position;
        m_Player.GetComponent<P2D_Motor>().Reset();

        m_Player.gameObject.SetActive(true);
        Player.Instance.Start();

        GameObject cloneEffect = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        Destroy(cloneEffect, 3f);
    }

    public static void KillPlayer(Player player)
    {
        gm.m_Player.gameObject.SetActive(false);
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
