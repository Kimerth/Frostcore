using UnityEngine;
using System.Collections;

public class P2DI_PlaceBlock : MonoBehaviour
{

    private Camera _camera;
    public Vector2 blockDimension;
    public LayerMask layerMask;

    GameObject thePreview;

    public float Range = 10f;

    void Start()
    {
        _camera = Camera.main; 
    }

    public void Preview(GameObject block)
    {
        Destroy(thePreview);

        Vector3 positionClick = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 placePosition;
        if (canPlace() == true)
        {
            placePosition.x = (int)((positionClick.x + 0.25f) * 2);
            placePosition.y = (int)((positionClick.y + 0.25f) * 2);

            if (placePosition.y >= 0)
                placePosition.y = GameMaster.gm.mapGen.height - placePosition.y;
            else
            {
                placePosition.y *= -1;
                placePosition.y += GameMaster.gm.mapGen.height;
            }

            if (block.GetComponent<BlockBreak>() == null)
                return;

            if (GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id != 0)
                return;

            GameObject clone;

            if (placePosition.y > GameMaster.gm.mapGen.height)
                clone = Instantiate(Resources.Load("Prefabs/" + block.GetComponent<BlockBreak>().id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, (float)-((int)placePosition.y - GameMaster.gm.mapGen.height) / 2), Quaternion.identity) as GameObject;
            else
                clone = Instantiate(Resources.Load("Prefabs/" + block.GetComponent<BlockBreak>().id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, GameMaster.gm.mapGen.height / 2 - (float)placePosition.y / 2), Quaternion.identity) as GameObject;

            clone.GetComponent<SpriteRenderer>().color = Color.yellow;
            clone.name = "Preview";
            thePreview = clone;
        }
    }

    public bool Place(GameObject block)
    {
        Vector3 positionClick = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 placePosition;
        if (canPlace() == true)
        {
            placePosition.x = (int)((positionClick.x + 0.25f) * 2);
            placePosition.y = (int)((positionClick.y + 0.25f) * 2);

            if (placePosition.y >= 0)
                placePosition.y = GameMaster.gm.mapGen.height - placePosition.y;
            else
            {
                placePosition.y *= -1;
                placePosition.y += GameMaster.gm.mapGen.height;
            }

            if (block.GetComponent<BlockBreak>() == null)
                return false;

            if (GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id != 0)
                return false;

            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id = block.GetComponent<BlockBreak>().id;

            GameObject clone;

            if (placePosition.y > GameMaster.gm.mapGen.height)
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, (float)-((int)placePosition.y - GameMaster.gm.mapGen.height) / 2), Quaternion.identity) as GameObject;
            else
                clone = Instantiate(Resources.Load("Prefabs/" + GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].id.ToString(), typeof(GameObject)), new Vector3((float)placePosition.x / 2, GameMaster.gm.mapGen.height / 2 - (float)placePosition.y / 2), Quaternion.identity) as GameObject;

            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].clone = clone;
            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].blockingAmount = 0.10f;
            GameMaster.gm.Map[(int)placePosition.x][(int)placePosition.y].spriteRenderer = clone.GetComponent<SpriteRenderer>();

            return true;
        }

        return false;
    }

    private bool canPlace()
    {
        Collider2D[] collHit = null;
        Vector3 pos = _camera.ScreenToWorldPoint(Input.mousePosition);

        try
        {
            collHit = Physics2D.OverlapPointAll(pos, layerMask);
            if (collHit[0] != null && collHit[0].gameObject.name != "Preview")
                return false;
        }
        catch { };

        return true;
    }
}
