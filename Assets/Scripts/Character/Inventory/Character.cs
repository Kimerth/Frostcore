using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Character : MonoBehaviour
{
    /// <summary>
    /// Instance for quick access
    /// </summary>
    public static Character Instance;

    void Start()
    {
        Instance = this;

        ItemsInWeaponSlots = new Item[4];
    }

    public Item[] ItemsInWeaponSlots;
    public Item[] ItemsInArmorSlots;

    public RectTransform[] WeaponSlots;
    public RectTransform[] ArmorSlots;

    public int WeaponInUseIndex;

    public Transform WeaponHolder;

    void Update()
    {
        if (!InventoryDisplay.Instance.displayInventory)
            return;

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(Inventory.Instance.draggedItem.isAnItemDragged)
            {
                if(EventSystem.current.currentSelectedGameObject != null)
                {
                    var draggedItemIndex = Inventory.Instance.draggedItem.indexInContents;

                    if (EventSystem.current.currentSelectedGameObject.name == "Melee1" && IsRightType(Inventory.Instance.Contents[draggedItemIndex], Item.ItemType.Tool, Item.ItemType.Weapon))
                    {
                        ItemsInWeaponSlots[0] = Inventory.Instance.Contents[draggedItemIndex];
                        Inventory.Instance.Contents.Remove(Inventory.Instance.Contents[draggedItemIndex]);
                        Inventory.Instance.draggedItem.ClearDraggedItem();

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Melee2" && IsRightType(Inventory.Instance.Contents[draggedItemIndex], Item.ItemType.Tool, Item.ItemType.Weapon))
                    {
                        ItemsInWeaponSlots[1] = Inventory.Instance.Contents[draggedItemIndex];
                        Inventory.Instance.Contents.Remove(Inventory.Instance.Contents[draggedItemIndex]);
                        Inventory.Instance.draggedItem.ClearDraggedItem();

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Ranged1" && IsRightType(Inventory.Instance.Contents[draggedItemIndex], Item.ItemType.Gun))
                    {
                        ItemsInWeaponSlots[2] = Inventory.Instance.Contents[draggedItemIndex];
                        Inventory.Instance.Contents.Remove(Inventory.Instance.Contents[draggedItemIndex]);
                        Inventory.Instance.draggedItem.ClearDraggedItem();

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Ranged2" && IsRightType(Inventory.Instance.Contents[draggedItemIndex], Item.ItemType.Gun))
                    {
                        ItemsInWeaponSlots[3] = Inventory.Instance.Contents[draggedItemIndex];
                        Inventory.Instance.Contents.Remove(Inventory.Instance.Contents[draggedItemIndex]);
                        Inventory.Instance.draggedItem.ClearDraggedItem();

                        UpdateEquipment();
                    }

                    InventoryDisplay.Instance.UpdateInventoryList();
                }
            }
            else
            {
                if(EventSystem.current.currentSelectedGameObject != null)
                {
                    if (EventSystem.current.currentSelectedGameObject.name == "Melee1")
                    {
                        if (ItemsInWeaponSlots[0] != null)
                            Inventory.Instance.Contents.Add(ItemsInWeaponSlots[0]);

                        ItemsInWeaponSlots[0] = null;

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Melee2")
                    {
                        if (ItemsInWeaponSlots[1] != null)
                            Inventory.Instance.Contents.Add(ItemsInWeaponSlots[1]);

                        ItemsInWeaponSlots[1] = null;

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Ranged1")
                    {
                        if (ItemsInWeaponSlots[2] != null)
                            Inventory.Instance.Contents.Add(ItemsInWeaponSlots[2]);

                        ItemsInWeaponSlots[2] = null;

                        UpdateEquipment();
                    }
                    else if (EventSystem.current.currentSelectedGameObject.name == "Ranged2")
                    {
                        if (ItemsInWeaponSlots[3] != null)
                            Inventory.Instance.Contents.Add(ItemsInWeaponSlots[3]);

                        ItemsInWeaponSlots[3] = null;

                        UpdateEquipment();
                    }
                }

                InventoryDisplay.Instance.UpdateInventoryList();
            }
        }
    }

    /// <summary>
    /// Checks if the item has the searched type
    /// </summary>
    /// <param name="item"> The item that is checked </param>
    /// <param name="acceptedTypes"> The accepted types </param>
    /// <returns> true || false </returns>
    public bool IsRightType(Item item, params Item.ItemType[] acceptedTypes)
    {
        IEnumerable<Item.ItemType> fitTypes = from value in acceptedTypes
                                              where value == item.itemType
                                              select value;

        return fitTypes.Count<Item.ItemType>() > 0;
    }

    /// <summary>
    /// Updates the equipped weapon and armor versions of the currently used items
    /// </summary>
    public void UpdateEquipment()
    {
        for(int index = 0; index < 4; index++)
            if (ItemsInWeaponSlots[index] != null)
            {
                WeaponSlots[index].GetChild(0).GetComponent<Image>().sprite = ItemsInWeaponSlots[index].Icon;
                WeaponSlots[index].GetChild(0).GetComponent<Image>().color = Color.white;
            }
            else
                WeaponSlots[index].GetChild(0).GetComponent<Image>().color = Color.clear;

        for (int index = 0; index < WeaponHolder.childCount; index++)
            Destroy(WeaponHolder.GetChild(index).gameObject);

        if (WeaponInUseIndex != -1)
            if (ItemsInWeaponSlots[WeaponInUseIndex] != null)
                InstantiateWeapon(ItemsInWeaponSlots[WeaponInUseIndex]);

        //TODO: Armors
    }

    public void InstantiateWeapon(Item item)
    {
        GameObject clone = Instantiate(item.EquippedPrefab) as GameObject;

        clone.transform.parent = WeaponHolder;
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localScale = item.EquippedPrefabScale;
    }
}