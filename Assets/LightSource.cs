using UnityEngine;
using System.Collections;

public class LightSource : MonoBehaviour
{
    public float Power = 1;

    public int MapPosX;
    public int MapPosY;

    public int antMapPosX;
    public int antMapPosY;

    public bool Stationary = true;
    public bool Added = false;

    public bool markedAsFar = false;

    public Unmanaged2DFloatMatrix mobileLight;

    public GameMaster.PreRenderedLight preRenderedLight;

    public void UpdateMapPos()
    {
        MapPosX = (int)(transform.position.x * 2);
        if ((int)(transform.position.y * 2) >= 0)
            MapPosY = GameMaster.gm.mapGen.height - (int)(transform.position.y * 2);
        else
            MapPosY = ((int)(transform.position.y * 2) * -1) + GameMaster.gm.mapGen.height;
    }

    public void UpdateantMapPos()
    {
        antMapPosX = MapPosX;
        antMapPosY = MapPosY;
    }

    void Start()
    {
        if (!Stationary)
        {
            if ((int)(Power % 0.05f) % 2 == 0)
                mobileLight = new Unmanaged2DFloatMatrix((int)(Power * 40) + 5, (int)(Power * 40) + 5);
            else
                mobileLight = new Unmanaged2DFloatMatrix((int)(Power * 40) + 4, (int)(Power * 40) + 4);
        }

        GameMaster.gm.lightSources.Add(this);
        GameMaster.gm.PreRenderLight(Power, this);
        StartCoroutine(GameMaster.gm.UpdateLight(this));
    }

    void OnDestroy()
    {
        if (!Stationary)
            GameMaster.gm.mapLight.ResetDinamicMatrix(this);

        GameMaster.gm.lightSources.Remove(this);
    }

    void OnDisable()
    {
        OnDestroy();
    }

    void OnEnable()
    {
        Start();
    }
}
