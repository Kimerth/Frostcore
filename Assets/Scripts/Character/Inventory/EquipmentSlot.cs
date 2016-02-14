using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    public InventoryEquipSlot SlotType;

    public int slotNumber;
    public Transform CorrespondingItem = null;

    public void EquipDraggedItem()
    {
        if (CorrespondingItem != null)
            return;

        Item itemBeingDragged = null;

        try
        {
            itemBeingDragged = InventoryDisplay.Instance.k_ItemBeingDragged.GetComponent<Item>();
        }
        catch { };

        if (itemBeingDragged == null)
            return;

        if (itemBeingDragged.EquipSlot != SlotType)
            return;

        if (SlotType == InventoryEquipSlot.MeleeWeapon || SlotType == InventoryEquipSlot.RangedWeapon)
        {
            Character.Instance.WeaponSlot[slotNumber] = itemBeingDragged;
        }
        else
        {
            Character.Instance.ArmorSlot[slotNumber] = itemBeingDragged;
        }

        CorrespondingItem = itemBeingDragged.transform;

        transform.GetChild(0).GetComponent<Image>().sprite = itemBeingDragged.itemIcon;
        transform.GetChild(0).GetComponent<Image>().color = Color.white;

        Inventory.Instance.RemoveItem(itemBeingDragged.transform);

        InventoryDisplay.Instance.ClearDraggedItem();
        InventoryDisplay.Instance.UpdateInventoryList();

        Character.Instance.UpdateEquipment();
    }

    public void UnequipItem()
    {
        if (CorrespondingItem == null)
            return;

        if (InventoryDisplay.Instance.itemBeingDragged.rectTransform != null)
            return;

        InventoryDisplay.Instance.itemBeingDragged.rectTransform = (Instantiate(transform.GetChild(0).gameObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<RectTransform>();
        transform.GetChild(0).GetComponent<Image>().sprite = InventoryDisplay.Instance.NullItemIcon;
        InventoryDisplay.Instance.itemBeingDragged.copyOf = transform.GetChild(0).GetComponent<RectTransform>();
        InventoryDisplay.Instance.k_ItemBeingDragged = CorrespondingItem;
        InventoryDisplay.Instance.itemBeingDragged.rectTransform.parent = Inventory.Instance.InventoryContents;
    }

    public void PlaceInHotbar()
    {
        if (Inventory.Instance.HotbarContents[slotNumber] != null)
            return;

        if (InventoryDisplay.Instance.itemBeingDragged.rectTransform == null)
            return;

        Item itemBeingDragged = null;

        try
        {
            itemBeingDragged = InventoryDisplay.Instance.k_ItemBeingDragged.GetComponent<Item>();
        }
        catch { };

        if (itemBeingDragged.EquipSlot != SlotType)
            return;

        Inventory.Instance.HotbarContents[slotNumber] = itemBeingDragged.transform;

        transform.GetChild(0).GetComponent<Image>().sprite = itemBeingDragged.itemIcon;
        transform.GetChild(0).GetComponent<Image>().color = Color.white;

        CorrespondingItem = InventoryDisplay.Instance.k_ItemBeingDragged;

        Inventory.Instance.RemoveItem(itemBeingDragged.transform);

        InventoryDisplay.Instance.ClearDraggedItem();
        InventoryDisplay.Instance.UpdateInventoryList();
    }

    public void RemoveFromHotbar()
    {
        if (Inventory.Instance.HotbarContents[slotNumber] == null)
            return;

        if (InventoryDisplay.Instance.itemBeingDragged.rectTransform != null)
            return;

        InventoryDisplay.Instance.itemBeingDragged.rectTransform = (Instantiate(transform.GetChild(0).gameObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<RectTransform>();
        transform.GetChild(0).GetComponent<Image>().sprite = InventoryDisplay.Instance.NullItemIcon;
        InventoryDisplay.Instance.itemBeingDragged.copyOf = transform.GetChild(0).GetComponent<RectTransform>();
        InventoryDisplay.Instance.k_ItemBeingDragged = CorrespondingItem;
        InventoryDisplay.Instance.itemBeingDragged.rectTransform.parent = Inventory.Instance.InventoryContents;
    }

    public void ClearSlot()
    {
        CorrespondingItem = null;
        transform.GetChild(0).GetComponent<Image>().color = Color.clear;

        ClearDraggedItem();
    }

    public void ClearDraggedItem()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = InventoryDisplay.Instance.itemBeingDragged.rectTransform.GetComponent<Image>().sprite;
    }
}
