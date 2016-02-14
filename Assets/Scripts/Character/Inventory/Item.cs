using UnityEngine;
using System.Collections;
using UnityEditor;

public enum InventoryEquipSlot
{
    MeleeWeapon, RangedWeapon, Head, Torso, Legs,
    NonEquippable
};

public enum MeleeTypes
{
    Pickaxe
};

public enum RangedTypes
{
    Pistol, Rifle
};

public class Item : MonoBehaviour
{
    public Sprite itemIcon; //The Icon.
    public bool canGet = true; //If we can pick up the Item.
    public bool isUsable;
    public float Weight = 0; // Mass of this (1 item, multiple stacks have this multiplied by stacks). (in kg damn 'muricans)
    public bool stackable = false; //Is it stackable? If yes then items with the same itemType will be stacked.
    public int maxStack= 0; //How many Items each stack can have before creating a new one. Remember that the Items that should be stacked should have the same itemType.
    public int stack = 0; //This is how many stack counts this Item will take up.
    
    public InventoryEquipSlot EquipSlot;

    public MeleeTypes MeleeType;

    public RangedTypes RangedType;
    
    //This is the object we will instantiate in the Players hand.
    //We use this so we can have two versions of the weapon. One for picking up and one for using.
    public GameObject equippedWeaponVersion;

    //These will store information about usefull components.
    public EquipmentEffect equipmentEffect;
    static Inventory playersinv;

    private bool FPPickUpFound = false;

    [AddComponentMenu("Inventory/Items/Item")]

    //Here we find the components we need.
    void Awake()
    {
        playersinv = FindObjectOfType(typeof(Inventory)) as Inventory; //finding the players inv.
        if (playersinv == null)
        {
            canGet = false;
            Debug.LogWarning("No 'Inventory' found in game. The Item " + transform.name + " has been disabled for pickup (canGet = false).");
        }
        else
        {
            gameObject.SendMessage("RetrievePlayer", playersinv, SendMessageOptions.DontRequireReceiver);
        }

        if (GetComponent<EquipmentEffect>())
        {
            equipmentEffect = GetComponent<EquipmentEffect>();
        }
    }

    void Start()
    {
        if (EquipSlot != InventoryEquipSlot.NonEquippable)
            if(equippedWeaponVersion == null)
            Debug.LogError("No equipped weapon version");
    }

    //Picking up the Item.
    public void PickUpItem()
    {
        bool getit = true;
        if (canGet)
        {//if its getable or hasnt been gotten.
            Item locatedit = null;
            playersinv.gameObject.SendMessage("PlayPickUpSound", SendMessageOptions.DontRequireReceiver); //Play sound
            if (stackable)
            {
                foreach (Transform t in playersinv.Contents)
                {
                    if (t != null)
                    {
                        if (t.name == this.transform.name)
                        {//if the item we wanna stack this on has the same name
                            Item i = t.GetComponent<Item>();
                            if (i.stack < i.maxStack)
                            {
                                locatedit = i;
                            }
                        }
                    }
                }
                if (locatedit != null)
                {//if we have a stack to stack it to!
                    getit = false;
                    if (locatedit.stack + stack <= locatedit.maxStack && Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        locatedit.stack += stack;
                        Player.Instance.pStats.CarryWeight += Weight * stack;
                        InventoryDisplay.Instance.UpdateInventoryList();
                        Destroy(this.gameObject);
                    }
                    else if (Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        stack -= locatedit.maxStack - locatedit.stack;
                        locatedit.stack = locatedit.maxStack;
                        playersinv.AddItem(this.transform);
                        MoveMeToThePlayer(playersinv.itemHolderObject);
                        Player.Instance.pStats.CarryWeight += Weight * stack;
                    }
                    else if (Player.Instance.pStats.CarryWeight + Weight <= Player.Instance.pStats.MaxCarryWeight)
                    {
                        if (locatedit.stack + (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight) <= locatedit.maxStack)
                        {
                            locatedit.stack += (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                            stack -= (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                        }
                        else
                        {
                            stack -= locatedit.maxStack - locatedit.stack;
                            locatedit.stack = locatedit.maxStack;
                            MoveMeToThePlayer(playersinv.itemHolderObject);
                            playersinv.AddItem(this.transform);
                        }
                        Player.Instance.pStats.CarryWeight += Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                    }
                    else
                    {
                        Debug.Log("Inventory is full");
                    }
                }
                else
                {
                    getit = true;
                }
            }
            //If we can get it and the inventory isn't full.
            if (getit && Player.Instance.pStats.CarryWeight + Weight * stack <= Player.Instance.pStats.MaxCarryWeight)
            {
                // Chuck it in the inventory
                playersinv.AddItem(this.transform);
                MoveMeToThePlayer(playersinv.itemHolderObject);//moves the object, to the player
            }
            else if (getit && Player.Instance.pStats.CarryWeight + Weight <= Player.Instance.pStats.MaxCarryWeight)
            {
                Transform clone = Instantiate(gameObject, transform.position, transform.rotation) as Transform;
                clone.GetComponent<Item>().stack -= (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                Player.Instance.pStats.CarryWeight += Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);
                stack -= (int) Weight * (int)((Player.Instance.pStats.MaxCarryWeight - Player.Instance.pStats.CarryWeight) / Weight);

                playersinv.AddItem(clone);
            }
            else if (getit)
            {
                Debug.Log("Inventory is full");
            }
        }
    }

    //Moves the item to the Players 'itemHolderObject' and disables it. In most cases this will just be the Inventory object.
    public void MoveMeToThePlayer(Transform itemHolderObject)
    {
        canGet = false;
        transform.gameObject.SetActive(false);
        transform.parent = itemHolderObject;
        transform.localPosition = Vector3.zero;
    }

    //Drops the Item from the Inventory.
    public void DropMeFromThePlayer(bool makeDuplicate)
    {
        GameObject clone;
        if (makeDuplicate == false) //We use this if the object is not stacked and so we can just drop it.
        {
            canGet = true;
            gameObject.SetActive(true);
            transform.parent = null;
            DelayPhysics();

            StartCoroutine(MakeItPickable());
        }
        else //If the object is stacked we need to make a clone of it and drop the clone instead.
        {
            canGet = true;
            clone = Instantiate(gameObject, transform.position, transform.rotation) as GameObject;
            canGet = false;
            clone.SetActive(true);
            clone.transform.parent = null;
            clone.name = gameObject.name;

            clone.GetComponent<Item>().stack = 1;

            clone.SendMessage("MakeItPickable", SendMessageOptions.DontRequireReceiver);
        }
    }

    public IEnumerator MakeItPickable()
    {
        yield return new WaitForSeconds(2f);

        transform.FindChild("ItemPickUp").gameObject.SetActive(true);
    }

    IEnumerator DelayPhysics()
    {
        if (playersinv.transform.parent.GetComponent<Collider>() != null && GetComponent<Collider>() != null)
        {
            Physics.IgnoreCollision(playersinv.transform.parent.GetComponent<Collider>(), GetComponent<Collider>(), true);
            yield return new WaitForSeconds(1);
            Physics.IgnoreCollision(playersinv.transform.parent.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
    }
}

[CustomEditor(typeof(Item))]
public class ItemEditor: Editor
{
    public override void OnInspectorGUI()
    {
        Item myTarget = (Item)target;

        myTarget.EquipSlot = (InventoryEquipSlot)EditorGUILayout.EnumPopup("EquipSlot: ", myTarget.EquipSlot);
        myTarget.itemIcon = (Sprite)EditorGUILayout.ObjectField("Item Icon", myTarget.itemIcon, typeof(Sprite));

        myTarget.canGet = EditorGUILayout.Toggle("Can Pick Up Item? ", myTarget.canGet);

        myTarget.Weight = EditorGUILayout.FloatField("Weight: ", myTarget.Weight);

        if (myTarget.EquipSlot == InventoryEquipSlot.NonEquippable)
        {
            myTarget.isUsable = EditorGUILayout.Toggle("Usable: ", myTarget.isUsable);

            myTarget.stackable = EditorGUILayout.Toggle("Stackable", myTarget.stackable);

            if (myTarget.stackable)
            {
                myTarget.maxStack = EditorGUILayout.IntField("Max Stack: ", myTarget.maxStack);
                myTarget.stack = EditorGUILayout.IntField("Stack: ", myTarget.stack);
            }
        }

        else
        {
            myTarget.equippedWeaponVersion = (GameObject)EditorGUILayout.ObjectField("Equipped Weapon Version: ", myTarget.equippedWeaponVersion, typeof(GameObject));
        }
    }

}