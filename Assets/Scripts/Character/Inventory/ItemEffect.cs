using UnityEngine;
using System.Collections;
[AddComponentMenu ("Inventory/Items/Item Effect")]
[RequireComponent(typeof (Item))]

//This script allows you to insert code when the Item is used (clicked on in the inventory).
public class ItemEffect : MonoBehaviour {
	public int amountToChange;
	
	bool deleteOnUse = true;

	private Inventory playersInv;
	private Item item;

	//This is where we find the components we need
	void Awake (){
		playersInv = FindObjectOfType(typeof(Inventory)) as Inventory; //finding the players inv.
		if (playersInv == null)
		{
			Debug.LogWarning("No 'Inventory' found in game. The Item " + transform.name + " has been disabled for pickup (canGet = false).");
		}
		item = GetComponent<Item>();
	}
	
	//This is called when the object should be used.
	public void UseEffect (){
		// INSERT CODE TO DO SOMETHING HERE
		/* e.g. health += 10; */

		//Play a sound
		playersInv.gameObject.SendMessage("PlayDropItemSound", SendMessageOptions.DontRequireReceiver);
		
		//This will delete the item on use or remove 1 from the stack (if stackable).
		if (deleteOnUse)
		{
			DeleteUsedItem();
		}
	}

	//This takes care of deletion
	void DeleteUsedItem (){
		if (item.stack == 1) //Remove item
		{
			playersInv.RemoveItem(this.gameObject.transform);
			GameObject.Destroy(this.gameObject);
		}
		else //Remove from stack
		{
			item.stack -= 1;
		}	
		Debug.Log(item.name + " has been deleted on use");
	}
}