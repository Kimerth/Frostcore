using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour
{
    /// <summary>
    /// Instance for quick access
    /// </summary>
    public static Inventory Instance;

    /// <summary>
    /// Reference to the UIOverlay, for canvas scaler purposes
    /// </summary>
    public RectTransform UIOverlay;

    void Start()
    {
        Instance = this;

        if (Contents == null)
            Contents = new List<Item>();

        HotbarContents = new int[9];
        Helper.Populate<int>(HotbarContents, -1);

        draggedItem = new DraggedItem();
    }

    /// <summary>
    /// The items held by the player
    /// </summary>
    public List<Item> Contents;

    /// <summary>
    /// The indexes of the items that are accessible from the hotbar
    /// </summary>
    public int[] HotbarContents;
    public RectTransform[] HotbarButtons;

    /// <summary>
    /// This has a separate class because it is used very often and by many components
    /// </summary>
    public class DraggedItem
    {
        public RectTransform itemIcon;
        public int indexInContents;
        public bool isAnItemDragged;

        /// <summary>
        /// Clones the icon on the itemIcon, adds an Canvas component with overrideSorting and starts the starts a coroutine that updates this clones position
        /// </summary>
        /// <param name="index"> The 0-based index of the item we want to drag in ItemIcons list </param>
        public void DragItem(int index)
        {
            indexInContents = index;
            isAnItemDragged = true;

            GameObject clone = Instantiate(InventoryDisplay.Instance.ItemIcons[index].GetChild(1).gameObject, Vector3.zero, Quaternion.identity) as GameObject;
            clone.transform.parent = InventoryDisplay.Instance.InventoryUIReference.transform;
            clone.AddComponent<Canvas>();
            clone.GetComponent<Canvas>().overrideSorting = true;
            clone.GetComponent<Canvas>().sortingLayerName = "UI";
            clone.GetComponent<Canvas>().sortingOrder = 20;
            clone.transform.localScale = clone.transform.localScale * (Inventory.Instance.UIOverlay.localScale.x * 2);
            itemIcon = clone.GetComponent<RectTransform>();

            Inventory.Instance.StartCoroutine(UpdateDraggedItemIconPos());
        }

        /// <summary>
        /// Destroys the currently dragged itemIcon
        /// </summary>
        public void ClearDraggedItem()
        {
            indexInContents = -1;
            isAnItemDragged = false;

            if (itemIcon == null)
                return;

            Destroy(itemIcon.gameObject);
            itemIcon = null;

            Inventory.Instance.StopCoroutine(UpdateDraggedItemIconPos());
        }

        /// <summary>
        /// Sets the dragged item's position to the mouse position, plus an offset
        /// </summary>
        /// <returns> null </returns>
        IEnumerator UpdateDraggedItemIconPos()
        {
            for(;;)
            {
                if (itemIcon == null)
                    yield break;

                var mousePos = GameMaster.gm.mainCam.ScreenToWorldPoint(Input.mousePosition);

                itemIcon.position = new Vector2(mousePos.x + 20 * (Inventory.Instance.UIOverlay.localScale.x * 1.5f), mousePos.y + 20 * (Inventory.Instance.UIOverlay.localScale.x * 1.5f));

                yield return null;
            }
        }
    }

    public DraggedItem draggedItem;

    /// <summary>
    /// Places an Item in Contents list.
    /// </summary>
    public void PickItem(Item item)
    {
        if (item == null)
            return;

        if (item.Count == 0)
        {
            Destroy(item.gameObject);
            return;
        }

        int fittingAmount = Mathf.Clamp((int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / item.Weight), 0, item.Count);

        // If the item is already in the Contents
        if (Contents.Find(x => x.Name == item.Name && x.Count < x.MaxStack) != null)
        {
            int index = Contents.FindIndex(x => x.Name == item.Name && x.Count < x.MaxStack);

            // If it cn fit wholy in one's stack
            if (fittingAmount + Contents[index].Count <= item.MaxStack)
            {
                // Just add it
                Contents[index].Count += fittingAmount;

                item.Count -= fittingAmount;
                fittingAmount = 0;

                Destroy(item.gameObject);
            }
            else
            {
                // Fill the first stack

                var auxCount = Contents[index].Count;
                Contents[index].Count = Mathf.Clamp(Contents[index].Count + fittingAmount, 0, Contents[index].MaxStack);
                item.Count = fittingAmount - (Mathf.Clamp(auxCount + fittingAmount, 0, item.MaxStack) - auxCount);
                fittingAmount = fittingAmount - (Mathf.Clamp(auxCount + fittingAmount, 0, item.MaxStack) - auxCount);
            }
        }
        
        // If there can still fit some of this item
        if(fittingAmount != 0)
        {
            // If it fits wholy
            if (item.Count == fittingAmount)
            {
                // Just add it
                Contents.Add(item);

                item.gameObject.SetActive(false);
                item.transform.parent = transform;
                item.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Add a clone, and uptade the item's remaining Count

                GameObject clone = Instantiate(item.gameObject) as GameObject;
                clone.name = item.Name;
                clone.transform.parent = transform;
                clone.transform.localPosition = Vector3.zero;
                clone.SetActive(false);

                clone.GetComponent<Item>().Count = fittingAmount;

                Contents.Add(clone.GetComponent<Item>());

                item.Count -= fittingAmount;
            }
        }
  
        // If there is nothing left of the item, destroy it
        if (item.Count == 0)
            Destroy(item.gameObject);

        InventoryDisplay.Instance.UpdateInventoryList();
    }

    /// <summary>
    /// Removes an item from Contents list.
    /// </summary>
    public void DropItem(Item item)
    {
        item.gameObject.SetActive(true);
        item.transform.parent = null;
        item.GetComponent<Collider2D>().enabled = true;
        item.gameObject.tag = "ItemIgnoreManeuver";
        item.transform.GetChild(0).gameObject.tag = "ItemIgnoreManeuver";
        StartCoroutine(ResetLayerOfItem(item));

        Contents.Remove(item);
    }

    public void UseItem(Item item)
    {

    }

    public void EquipItem(Item item)
    {
        Character.Instance.WeaponInUseIndex = -1;

        Character.Instance.UpdateEquipment();

        Character.Instance.InstantiateWeapon(item);
    }

    IEnumerator ResetLayerOfItem(Item item)
    {
        yield return new WaitForSeconds(4f);
        item.gameObject.tag = "Item";
        item.transform.GetChild(0).gameObject.tag = "Item";
    }
}