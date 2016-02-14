using UnityEngine;
using System.Collections;
[AddComponentMenu("Inventory/Character Sheet")]
[RequireComponent(typeof(Inventory))]
public class Character : MonoBehaviour
{
    public static Character Instance;

    public Item[] WeaponSlot; //This is where the Weapons are going to go (be parented too). In my case it's the "Melee" gameobject.
    public GameObject[] Weapons;

    public Item[] ArmorSlot; //This is the built in Array that stores the Items equipped. You can change this to static if you want to access it from another script.

    public Transform WeaponHolder;

    //These are keeping track of components such as equipmentEffects and Audio.
    private Inventory playersinv; //Refers to the Inventory script.
    private InvAudio invAudio;

    public int ItemEquippedNR;

    //Assign the differnet components to variables and other "behind the scenes" stuff.
    void Awake()
    {
        Instance = this;

        playersinv = GetComponent<Inventory>();

        invAudio = GetComponent<InvAudio>();
    }

    //Take care of the array lengths.
    void Start()
    {
        ArmorSlot = new Item[3];
        Weapons = new GameObject[4];
        WeaponSlot = new Item[4];

        if (WeaponHolder == null)
            Debug.LogError("No weapon holder");

        UpdateEquipment();
    }

    public void UpdateEquipment()
    {
        int i;
        for (i = 0; i < WeaponHolder.childCount; i++)
        {
            Destroy(WeaponHolder.GetChild(i).gameObject);
            Weapons[i] = null;
        }
        i = 0;
        foreach (Item Weapon in WeaponSlot)
        {
            if (Weapon == null)
            {
                i++;
                continue;
            }

            Weapons[i] = (GameObject) Instantiate(Weapon.equippedWeaponVersion, Vector3.zero, Quaternion.identity);

            if (Weapons[i] == null)
                Debug.Log("Weapons is null");

            Weapons[i].transform.parent = WeaponHolder;
            Weapons[i].transform.localRotation = Weapon.equippedWeaponVersion.transform.localRotation;
            Weapons[i].transform.localPosition = Weapon.equippedWeaponVersion.transform.localPosition;
            Weapons[i].transform.localScale = Weapon.equippedWeaponVersion.transform.localScale;

            if (i == ItemEquippedNR)
            {
                MakeItemActive(Weapons[i]);
                if (P2D_Controller.Instance.ItemBeingHeld != null)
                    Destroy(P2D_Controller.Instance.ItemBeingHeld.gameObject);
                P2D_Controller.Instance.ItemBeingHeld = Weapons[i].transform;
            }
            else
                MakeItemInactive(Weapons[i]);

            i++;
        }

        // TODO: Armor equip
    }

    public void MakeItemInactive(GameObject item)
    {
        item.SetActive(false);
    }

    public void MakeItemActive(GameObject item)
    {
        item.SetActive(true);
    }
}