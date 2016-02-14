using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
[AddComponentMenu("Inventory/Inventory Display")]
[RequireComponent(typeof(Inventory))]

public class InventoryDisplay : MonoBehaviour
{
    //Displaying the Inventory.

    public RectTransform Inventory;
    public Vector2 InitialPos = new Vector2(140, 180);

    public RectTransform HotBar;

    public static InventoryDisplay Instance;

    public struct DraggedItem
    {
        public RectTransform rectTransform;
        public RectTransform copyOf;
        public int posInContents;
    }
    
    //Variables for dragging:
    public DraggedItem itemBeingDragged; //This refers to the 'Icon'.
    public Transform k_ItemBeingDragged;    //This not.
    public Sprite NullItemIcon;
    private Vector2 draggedItemPosition; //Where on the screen we are dragging our Item.
    private Vector2 draggedItemSize;//The size of the item icon we are dragging.

    //Variables for updating the inventory
    private List<Transform> UpdatedList; //The updated inventory array.
    private RectTransform[] ItemIcons;
    private bool[] IconUsed;

    //More variables for the window:
    public static bool displayInventory = false; //If inv is opened.
    Vector2 Offset = new Vector2(7, 12); //This will leave so many pixels between the edge of the window (x = horizontal and y = vertical).

    public KeyCode onOffButton = KeyCode.I; //The button that turns the Inventory window on and off.

    public RectTransform CarryWeightBar;

    //Keeping track of components.
    private Inventory associatedInventory;
    private bool cSheetFound = false;
    private Character cSheet;

    public int CurrentPage = 1;
    public int MaxPage = 1;

    //Store components and adjust the window position.
    void Awake()
    {
        Instance = this;

        ItemIcons = new RectTransform[6];
        IconUsed = new bool[6];
        UpdatedList = new List<Transform>();

        associatedInventory = GetComponent<Inventory>();//keepin track of the inventory script

        itemBeingDragged = new DraggedItem();

        for (int i = 0; i < 6; i++)
        {
            ItemIcons[i] = associatedInventory.InventoryContents.GetChild(i) as RectTransform;
        }

        for (int i = 0; i < 6; i++)
        {
            ItemIcons[i].GetChild(0).GetComponent<Image>().sprite = NullItemIcon;
            ItemIcons[i].GetChild(1).GetComponent<Text>().text = " ";

            ItemIcons[i].gameObject.SetActive(false);
        }

        if (GetComponent<Character>() != null)
        {
            cSheetFound = true;
            cSheet = GetComponent<Character>();
        }
        else
        {
            Debug.LogError("No Character script was found on this object. Attaching one allows for functionality such as equipping items.");
            cSheetFound = false;
        }
    }

    //Update the inv list
    public void UpdateInventoryList()
    {
        int i;
        for (i = 0; i < 6; i++)
            IconUsed[i] = false;

        UpdatedList = associatedInventory.Contents;

        if (UpdatedList.Count > MaxPage * 6 - 1)
        {
            MaxPage++;
        }

        i = 0;
        foreach (Transform Content in UpdatedList)
        {
            if (i >= CurrentPage * 6 - 6 && i <= CurrentPage * 6 - 1)
            {
                ItemIcons[i % 6].GetChild(0).GetComponent<Image>().sprite = Content.GetComponent<Item>().itemIcon;
                ItemIcons[i % 6].GetChild(1).GetComponent<Text>().text = Content.name;

                if (Content.GetComponent<Item>().stackable)
                    ItemIcons[i % 6].GetChild(2).GetComponent<Text>().text = "x " + Content.GetComponent<Item>().stack;
                else
                    ItemIcons[i % 6].GetChild(2).GetComponent<Text>().text = null;
                ItemIcons[i % 6].GetComponent<ItemIcon>().CorrespondingItem = Content;
                ItemIcons[i % 6].GetComponent<ItemIcon>().CorrespondingItemPosInContents = i;

                IconUsed[i % 6] = true;
            }
            i++;
        }

        for (i = 0; i < 6; i++)
        {
            if (!IconUsed[i])
            {
                ItemIcons[i].gameObject.SetActive(false);
            }
            else
            {
                ItemIcons[i].gameObject.SetActive(true);
            }
        }

        Debug.Log("Inventory Updated");

    }

    void Update()
    {
        CarryWeightBar.GetComponent<Image>().fillAmount = Player.Instance.pStats.CarryWeight / Player.Instance.pStats.MaxCarryWeight;

        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (mousePos.x > Inventory.position.x && mousePos.y < Inventory.position.y &&
                mousePos.x < Inventory.position.x + Inventory.rect.width + HotBar.rect.width && mousePos.y > Inventory.position.y - Inventory.rect.height * 5 / 6)
            {

            }
            else
            {
                // DROP ITEM
                try
                {
                    associatedInventory.DropItem(itemBeingDragged.rectTransform.transform.parent.GetComponent<ItemIcon>().CorrespondingItem.GetComponent<Item>());
                }
                catch { };
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) //Pressed escape
        {
            ClearDraggedItem(); //Get rid of the dragged item.
        }
        if (Input.GetMouseButtonDown(1)) //Pressed right mouse
        {
            ClearDraggedItem(); //Get rid of the dragged item.
        }

        //Turn the Inventory on and off and handle audio + pausing the game.
        if (Input.GetKeyDown(onOffButton))
        {
            if (displayInventory)
            {
                displayInventory = false;
                ClearDraggedItem();
                Inventory.gameObject.SetActive(false);
                gameObject.SendMessage("ChangedState", false, SendMessageOptions.DontRequireReceiver);
                gameObject.SendMessage("PauseGame", false, SendMessageOptions.DontRequireReceiver); //StopPauseGame/EnableMouse/ShowMouse
            }
            else
            {
                displayInventory = true;
                Inventory.gameObject.SetActive(true);
                gameObject.SendMessage("ChangedState", true, SendMessageOptions.DontRequireReceiver);
                gameObject.SendMessage("PauseGame", true, SendMessageOptions.DontRequireReceiver); //PauseGame/DisableMouse/HideMouse
            }
        }

        //Making the dragged icon update its position
        if (itemBeingDragged.rectTransform != null)
        {
            //Give it a 20 pixel space from the mouse pointer to allow the Player to click stuff and not hit the button we are dragging.
            draggedItemPosition.y = Input.mousePosition.y + 20;
            draggedItemPosition.x = Input.mousePosition.x + 20;
            itemBeingDragged.rectTransform.position = draggedItemPosition;
        }
    }

    //If we are dragging an item, we will clear it.
    public void ClearDraggedItem()
    {
        try
        {
            itemBeingDragged.copyOf.parent.GetComponent<ItemIcon>().ClearDraggedItem();
        }
        catch { };

        if (itemBeingDragged.rectTransform == null)
            return;

        Destroy(itemBeingDragged.rectTransform.gameObject);

        itemBeingDragged.rectTransform = null;
        itemBeingDragged.copyOf = null;
        itemBeingDragged.posInContents = -1;
        k_ItemBeingDragged = null;
    }
}