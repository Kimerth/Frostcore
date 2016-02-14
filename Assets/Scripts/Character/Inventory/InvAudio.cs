using UnityEngine;
using System.Collections;
[RequireComponent(typeof (AudioSource))]
[AddComponentMenu ("Inventory/Other/Inv Audio")]
public class InvAudio : MonoBehaviour {


//The sound clips
AudioClip openSound;
AudioClip closeSound;
AudioClip equipSound;
AudioClip pickUpSound;
AudioClip dropItemSound;



void Awake (){
	//This is where we check if the script is attached to the Inventory.
	if (transform.name != "Inventory")
	{
		Debug.LogError("An InvAudio script is placed on " + transform.name + ". It should only be attached to an 'Inventory' object");
	}
	
	//This is where we assign the default sounds if nothing else has been put in.
	if (openSound == null)
	{
		openSound = Resources.Load("Sounds/InvOpenSound") as AudioClip;
	}
	if (closeSound == null)
	{
		closeSound = Resources.Load("Sounds/InvCloseSound") as AudioClip;
	}
	if (equipSound == null)
	{
		equipSound = Resources.Load("Sounds/InvEquipSound") as AudioClip;
	}
	if (pickUpSound == null)
	{
		pickUpSound = Resources.Load("Sounds/InvPickUpSound") as AudioClip;
	}
	if (dropItemSound == null)
	{
		dropItemSound = Resources.Load("Sounds/InvDropItemSound") as AudioClip;
	}
}

//This is where we play the open and close sounds.
void ChangedState ( bool open  ){
	 if (open)
	{
		GetComponent<AudioSource>().clip = openSound;
		GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.1f);
		GetComponent<AudioSource>().Play();
	}
	else
	{
		GetComponent<AudioSource>().clip = closeSound;
		GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.1f);
		GetComponent<AudioSource>().Play();
	}
}

//The rest of the functions can easily be called to play different sounds using SendMessage("Play<NameOfSound>", SendMessageOptions.DontRequireReceiver);

void PlayEquipSound (){
	GetComponent<AudioSource>().clip = equipSound;
	GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.1f);
	GetComponent<AudioSource>().Play();
}

void PlayPickUpSound (){
	GetComponent<AudioSource>().clip = pickUpSound;
	GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.1f);
	GetComponent<AudioSource>().Play();
}

void PlayDropItemSound (){
	GetComponent<AudioSource>().clip = dropItemSound;
	GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.1f);
	GetComponent<AudioSource>().Play();
}
}