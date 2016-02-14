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

    private float timeFromLastShiftPress;
    private bool shiftWasPressed;
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
	    if(!isJumping)
        {
            isJumping = Input.GetButtonDown("Jump");
        }

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
                block = ItemBeingHeld.gameObject;
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
                    P2D_Animator.Instance.Attack();
            }

            if (canBreak)
                _DestroyBlock.MiningStart();
            if (canPlace)
                _PlaceBlock.Place(block);

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

        if (!isSprinting)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        if (!isDashing && !P2D_Motor.Instance.IsDashing)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                shiftWasPressed = true;
                timeFromLastShiftPress = Time.time;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (shiftWasPressed == true && Time.time < timeFromLastShiftPress + 0.2f)
                {
                    isDashing = true;
                }
                else
                    isDashing = false;
            }
        }

        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftWasPressed = false;
        }
        P2D_Motor.Instance.ImposedUpdate();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            int tries = 0;

            do
            {
                if (Character.Instance.ItemEquippedNR == 3)
                    Character.Instance.ItemEquippedNR = 0;
                else
                    Character.Instance.ItemEquippedNR++;

                tries++;

                if (tries > 4)
                    break;
            } while (Character.Instance.WeaponSlot[Character.Instance.ItemEquippedNR] == null);
            Character.Instance.UpdateEquipment();
        }

        if (InventoryDisplay.displayInventory)
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
        }
	}

    void FixedUpdate()
    {
        var deadZone = 0.1f;
        bool crouch = Input.GetKey(KeyCode.C);

        float moveHorizontal = 0f;

        if(Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            moveHorizontal = Input.GetAxis("Horizontal");

        P2D_Motor.Instance.Move(moveHorizontal, isJumping, crouch, isSprinting);

        P2D_Motor.Instance.Dash(moveHorizontal, isDashing);

        P2D_Motor.Instance.ImposedFixedUpdate();
        P2D_Animator.Instance.ImposedFixedUpdate();

        isJumping = false;
        isSprinting = false;
        isDashing = false;
    }
}
