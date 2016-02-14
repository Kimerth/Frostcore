using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemIcon : MonoBehaviour
{
    public Vector2 InitialPos = new Vector2(-81.6f, 3.0518e-05f);

    public Transform CorrespondingItem;
    public int CorrespondingItemPosInContents;

    public void DragItem()
    {
        if (InventoryDisplay.Instance.itemBeingDragged.rectTransform != null)
            return;

        InventoryDisplay.Instance.itemBeingDragged.rectTransform = (Instantiate(transform.GetChild(0).gameObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<RectTransform>();
        transform.GetChild(0).GetComponent<Image>().sprite = InventoryDisplay.Instance.NullItemIcon;
        InventoryDisplay.Instance.itemBeingDragged.copyOf = transform.GetChild(0).GetComponent<RectTransform>();
        InventoryDisplay.Instance.itemBeingDragged.posInContents = CorrespondingItemPosInContents;
        InventoryDisplay.Instance.itemBeingDragged.rectTransform.parent = Inventory.Instance.InventoryContents;

        InventoryDisplay.Instance.k_ItemBeingDragged = CorrespondingItem;
    }

    public void SwapItems()
    {
        if (InventoryDisplay.Instance.itemBeingDragged.rectTransform == null)
            return;

        if(InventoryDisplay.Instance.itemBeingDragged.posInContents == -1)
        {
            Inventory.Instance.AddItem(InventoryDisplay.Instance.k_ItemBeingDragged);

            InventoryDisplay.Instance.itemBeingDragged.rectTransform.transform.parent.gameObject.SendMessage("ClearSlot", SendMessageOptions.DontRequireReceiver);

            InventoryDisplay.Instance.ClearDraggedItem();
            return;
        }
        
        Transform aux;
        aux = Inventory.Instance.Contents[CorrespondingItemPosInContents];
        Inventory.Instance.Contents[CorrespondingItemPosInContents] = Inventory.Instance.Contents[InventoryDisplay.Instance.itemBeingDragged.posInContents];
        Inventory.Instance.Contents[InventoryDisplay.Instance.itemBeingDragged.posInContents] = aux;

        InventoryDisplay.Instance.ClearDraggedItem();
        InventoryDisplay.Instance.UpdateInventoryList();
    }

    public void ClearDraggedItem()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = InventoryDisplay.Instance.itemBeingDragged.rectTransform.GetComponent<Image>().sprite;
    }
}
