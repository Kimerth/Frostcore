using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
[Serializable]
[AddComponentMenu("Inventory/Inventory")]
public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    //This is the central piece of the Inventory System.
    public List<Transform> Contents; //The content of the Inventory

    /// <summary>
    /// Non 'Item' objects that can be accesed by the player without opening the 'Inventory'
    /// </summary>
    public Transform[] HotbarContents;
    public RectTransform[] HotbarIcons;
    
    public RectTransform InventoryContents;
    public GameObject ItemRectangle;

    bool DebugMode = false; //If this is turned on the Inventory script will output the base of what it's doing to the Console window.

    private InventoryDisplay playersInvDisplay; //Keep track of the InventoryDisplay script.

    public Transform itemHolderObject; //The object the unactive items are going to be parented to. In most cases this is going to be the Inventory object itself.

    //Handle components and assign the itemHolderObject.
    void Awake()
    {
        Instance = this;

        itemHolderObject = gameObject.transform;
        HotbarContents = new Transform[9];

        playersInvDisplay = GetComponent<InventoryDisplay>();
        if (playersInvDisplay == null)
        {
            Debug.LogError("No Inventory Display script was found on " + transform.name + " but an Inventory script was.");
            Debug.LogError("Unless a Inventory Display script is added the Inventory won't show. Add it to the same gameobject as the Inventory for maximum performance");
        }
    }

    //Add an item to the inventory.
    public void AddItem(Transform Item)
    {
        List<Transform> newContents = new List<Transform>(); // Create the array that will temporarily hold our inventory as we transfer it
        // Grabs the current contents and puts it into the array
        foreach (Transform i in Contents)
            // If the transform isn't empty in the old inventory list, add it to the new one
            if (i != null)
                newContents.Add(i);

        // If it's not in there add it in (regardless of if it stacks or not)
        if (!newContents.Contains(Item))
            newContents.Add(Item);

        Contents = newContents;

        if (DebugMode)
        {
            // Get the amount in use
            int invInUse = 0;
            foreach (Transform i in Contents)
                if (i != null)
                    invInUse += 1;
            // Display it
            Debug.Log(Item.name + " has been added to inventroy");
            Debug.Log("The Inventory contains " + invInUse + " items");
        }

        //Tell the InventoryDisplay to update the list.
        if (playersInvDisplay != null)
        {
            playersInvDisplay.UpdateInventoryList();
        }
    }

    //Removed an item from the inventory (IT DOESN'T DROP IT).
    public void RemoveItem(Transform Item)
    {
        List<Transform> newContents = new List<Transform>(Contents);
        int index = 0;
        bool shouldend = false;
        foreach (Transform i in newContents) //Loop through the Items in the Inventory:
        {
            if (i == Item) //When a match is found, remove the Item.
            {
                newContents.RemoveAt(index);
                shouldend = true;
                //No need to continue running through the loop since we found our item.
            }
            index++;

            if (shouldend) //Exit the loop
            {
                Contents = newContents;
                if (DebugMode)
                {
                    Debug.Log(Item.name + " has been removed from inventroy");
                }
                if (playersInvDisplay != null)
                {
                    playersInvDisplay.UpdateInventoryList();
                }
                return;
            }
        }
    }

    //Dropping an Item from the Inventory
    public void DropItem(Item item)
    {
        gameObject.SendMessage("PlayDropItemSound", SendMessageOptions.DontRequireReceiver); //Play sound
        bool makeDuplicate = false;
        if (item.stack == 1) //Drop item
        {
            RemoveItem(item.transform);
        }
        else //Drop from stack
        {
            item.stack -= 1;
            makeDuplicate = true;
        }

        item.DropMeFromThePlayer(makeDuplicate); //Calling the drop function + telling it if the object is stacked or not.

        InventoryDisplay.Instance.UpdateInventoryList();

        if (DebugMode)
        {
            Debug.Log(item.name + " has been dropped");
        }
    }
    
    public void EquipItem(Transform item, int index)
    {
        if (P2D_Controller.Instance.ItemBeingHeld != null)
        {
            Destroy(P2D_Controller.Instance.ItemBeingHeld.gameObject);
            P2D_Controller.Instance.ItemBeingHeld = null;
            HotbarContents[P2D_Controller.Instance.IndexOfItemBeingHeld].GetComponent<Image>().color = Color.white;
        }
        GameObject clone = new GameObject();
        clone = Instantiate(item.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
        clone.transform.SetParent(P2D_Controller.Instance.ItemHolderObject, false);
        clone.SetActive(true);
        P2D_Controller.Instance.ItemBeingHeld = item;

        HotbarContents[index].GetComponent<Image>().color = Color.red;
    }

    public void UseItem(Transform item)
    {
        Debug.Log("USE ITEM");
    }

    //This will tell you everything that is in the inventory.
    void DebugInfo()
    {
        Debug.Log("Inventory Debug - Contents");
        int items = 0;
        foreach (Transform i in Contents)
        {
            items++;
            // Debug.Log(i.name);
        }
        Debug.Log("Inventory contains " + items + " Item(s)");
    }

    //Drawing an 'S' in the scene view on top of the object the Inventory is attached to stay organized.
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(new Vector3(transform.position.x, transform.position.y + 2.3f, transform.position.z), "InventoryGizmo.png", true);
    }
}