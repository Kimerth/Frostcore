using UnityEngine;
using System.Collections;

public class P2DI_PlaceBlock : MonoBehaviour
{

    private Camera _camera;
    public Vector2 blockDimension;
    public LayerMask layerMask;

    public float Range = 10f;

    private LayerMask airMask;

    void Start()
    {
        _camera = Camera.main;

        airMask = LayerMask.GetMask("Air"); 
    }

    public void Place(GameObject block)
    {
        Vector3 pos2 = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 positionClick = new Vector2(pos2.x, pos2.y);
        Vector2 placePosition;
        if (canPlace() == true)
        {
            placePosition.x = ((int)(positionClick.x / blockDimension.x) * blockDimension.x);
            placePosition.y = ((int)(positionClick.y / blockDimension.y) * blockDimension.y);
            Instantiate(block, placePosition, Quaternion.identity);

            GameObject objectHit = null;

            try
            {
                objectHit = Physics2D.OverlapPoint(placePosition, airMask).gameObject;
                Destroy(objectHit);
            }
            catch { }
        }
    }

    private bool canPlace()
    {
        GameObject objectHit = null;
        Vector3 pos2 = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new Vector2(pos2.x, pos2.y);

        try
        {
            objectHit = Physics2D.OverlapPoint(pos, layerMask).gameObject;
            if (objectHit != null)
                return false;
        }
        catch { };

        return true;
    }
}
