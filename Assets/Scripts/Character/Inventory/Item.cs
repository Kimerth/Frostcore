using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public Sprite Icon;

    public string Name;
    
    /// <summary>
    /// The mass of each unit of this item in KG.
    /// </summary>
    public float Weight;

    public enum ItemType
    {
        Tool, Weapon, Gun, Itemblock
    }

    public ItemType itemType;

    public enum ItemSubType
    {
        Pistol, Pickaxe, None
    }

    public ItemSubType itemSubType;

    /// <summary>
    /// Corresponding ID of this Item. Must be assigned manually
    /// </summary>
    public int ID;
    
    /// <summary>
    /// How many of this item are stacked together
    /// </summary>
    public int Count;

    /// <summary>
    /// The limit of how many this item can be stacked together
    /// </summary>
    public int MaxStack;

    /// <summary>
    /// The equipped object prefab; to make sure it looks right in-game
    /// </summary>
    public GameObject EquippedPrefab;
    public Vector3 EquippedPrefabScale;

    /// <summary>
    /// If the item is placeble, it must have a placement prefab; to make sure it looks right in-game
    /// </summary>
    public GameObject ItemPlacement;

    /// <summary>
    /// Some lore.
    /// </summary>
    public string Description;
}