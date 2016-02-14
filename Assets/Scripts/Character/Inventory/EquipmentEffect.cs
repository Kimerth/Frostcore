using UnityEngine;
using System.Collections;
[AddComponentMenu ("Inventory/Items/Equipment Effect")]
[RequireComponent(typeof (Item))]
public class EquipmentEffect : MonoBehaviour {


//This script allows you to create equipment effects that will be called either OnEquip or WhileEquipped. This is usefull for magic effects and stat handling.



bool effectActive = false;

void Update (){
	if (effectActive == true)
	{
		//-----> THIS IS WHERE YOU INSERT CODE YOU WANT TO EXECUTE AS LONG AS THE ITEM IS EQUIPPED. <-----
	}
}

public void EquipmentEffectToggle ( bool effectIs  ){
	 if (effectIs == true)
	{
		effectActive = true;
		Debug.LogWarning("Remember to insert code for the EquipmentEffect script you have attached to " + transform.name + ".");
		
		//-----> THIS IS WHERE YOU INSERT CODE YOU WANT TO EXECUTE JUST WHEN THE ITEM IS EQUIPPED. <-----
		
	}
	else
	{
		effectActive = false;
		
		//-----> THIS IS WHERE YOU INSERT CODE YOU WANT TO EXECUTE JUST WHEN THE ITEM IS UNEQUIPPED. <-----
	}
}
}