using UnityEngine;
using System.Collections;

[RequireComponent(typeof(P2D_Motor))]
[RequireComponent(typeof(P2D_Animator))]
public class P2D_Controller : MonoBehaviour
{
    public static P2D_Controller Instance;

    private P2DI_DestroyBlock _DestroyBlock;
	private P2DI_PlaceBlock _PlaceBlock;

    private bool isJumping;
    private bool isSprinting;

    private float timeToShifPressReset;
    private bool dashKeyWasPressed;
    private bool isDashing;

    /// <summary>
    /// Keys that need to be pressed in order to use an Hotbar's item
    /// </summary>
    public KeyCode[] HotbarKeyCodes;

    public Transform ItemHolderObject;
    public Transform ItemBeingHeld;
    public int IndexOfItemBeingHeld;

    public bool canPlace = false;
    public bool canBreak = false;

	public GameObject block;

    private float timeToFire;

	void Awake() 
    {
        Instance = this;

        _DestroyBlock = GetComponent<P2DI_DestroyBlock>();
		_PlaceBlock = GetComponent<P2DI_PlaceBlock>();
	}
	
	void Update() 
    {
        if (!GameMaster.gm.isMenuActive)
            P2D_Motor.Instance.ImposedUpdate();

	    if(!isJumping)
            isJumping = Input.GetButtonDown("Jump");

        P2D_Animator.Instance.FacingRight(P2D_Motor.Instance.FacingRight);

        if(ItemBeingHeld != null)
        {
            if (ItemBeingHeld.GetComponent<MayBreak>() != null)
            {
                _DestroyBlock._speed = ItemBeingHeld.GetComponent<MayBreak>().Speed;
                canBreak = true;
                canPlace = false;
                P2D_Animator.Instance.HoldGun(false);
            }
            else if (ItemBeingHeld.tag == "Breakable")
            {
                block = ItemBeingHeld.GetComponent<Item>().ItemPlacement;
                canPlace = true;
                canBreak = false;
                P2D_Animator.Instance.HoldGun(false);
            }
            else if(ItemBeingHeld.GetComponent<Weapon>() != null)
            {
                P2D_Animator.Instance.HoldGun(true);
                canPlace = false;
                canBreak = false;
            }
            else
            {
                canPlace = false;
                canBreak = false;
                P2D_Animator.Instance.HoldGun(false);
            }
        }
        else
        {
            P2D_Animator.Instance.HoldGun(false);
            canPlace = false;
            canBreak = false;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (ItemBeingHeld != null)
            {
                if (ItemBeingHeld.GetComponent<Weapon>() == null)
                {
                    if (Player.Instance.pStats.curStamina > 0)
                        P2D_Animator.Instance.Attack();
                }
            }

            if (canBreak)
                _DestroyBlock.MiningStart();
            if (canPlace)
            {
                _PlaceBlock.Place(block);
                Inventory.Instance.Contents[Inventory.Instance.HotbarContents[IndexOfItemBeingHeld]].GetComponent<Item>().Count--;
                if (Inventory.Instance.Contents[Inventory.Instance.HotbarContents[IndexOfItemBeingHeld]].GetComponent<Item>().Count == 0)
                {
                    Destroy(Inventory.Instance.Contents[Inventory.Instance.HotbarContents[IndexOfItemBeingHeld]].gameObject);
                    block = null;
                    Destroy(ItemBeingHeld.gameObject);
                }
            }

        }
        else if (Input.GetButtonUp("Fire1"))
        {
            if (ItemBeingHeld != null)
            {
                if (ItemBeingHeld.GetComponent<Weapon>() == null)
                    P2D_Animator.Instance.Attack(false);
            }

            _DestroyBlock.MiningStop();
        }

        if(!isDashing && Input.GetKey(KeyCode.LeftShift))
        {
            if (!dashKeyWasPressed)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    dashKeyWasPressed = true;
                    timeToShifPressReset = Time.time + 0.2f;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                    isDashing = true;
                else if (timeToShifPressReset < Time.time)
                    dashKeyWasPressed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            int tries = 0;

            do
            {
                if (Character.Instance.WeaponInUseIndex == 3)
                    Character.Instance.WeaponInUseIndex = 0;
                else
                    Character.Instance.WeaponInUseIndex++;

                tries++;

                if (tries > 4)
                    break;
            } while (Character.Instance.ItemsInWeaponSlots[Character.Instance.WeaponInUseIndex] == null);
            Character.Instance.UpdateEquipment();
        }
        /*
        if (InventoryDisplay.Instance.displayInventory)
            return;

        int i = 0;
        foreach (KeyCode k in HotbarKeyCodes)
        {
            if (Input.GetKeyDown(k))
            {
                if (Inventory.Instance.HotbarContents[i].GetComponent<Item>().isUsable)
                    Inventory.Instance.UseItem(Inventory.Instance.HotbarContents[i]);
                else
                {
                    Inventory.Instance.EquipItem(Inventory.Instance.HotbarContents[i], i);
                    IndexOfItemBeingHeld = i;
                }
            }
            i++;
        }*/
	}

    void FixedUpdate()
    {
        var deadZone = 0.1f;
        bool crouch = Input.GetKey(KeyCode.C);

        if (Player.Instance.pStats.curStamina > 0)
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        else
            isSprinting = false;

        float moveHorizontal = 0f;

        if(Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            moveHorizontal = Input.GetAxis("Horizontal");

        if (isJumping)
            if (Player.Instance.pStats.curStamina == 0)
                isJumping = false;

        P2D_Motor.Instance.ImposedFixedUpdate();

        P2D_Motor.Instance.Move(moveHorizontal, isJumping, crouch, isSprinting);

        if (isDashing)
            if (Player.Instance.pStats.curStamina == 0)
                isDashing = false;

        P2D_Motor.Instance.Dash(isDashing);

        P2D_Animator.Instance.ImposedFixedUpdate();

        isJumping = false;
    }
}
