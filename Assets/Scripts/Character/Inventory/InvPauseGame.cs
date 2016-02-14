using UnityEngine;
using System.Collections;

public class InvPauseGame : MonoBehaviour {



/*This script can be attached if you want to do one of the following things:
1. Pause/Unpause the game.
2. Enable/Disable the MouseLook component.
3. Lock/Unlock the mouse cursor.
*/
public bool pauseGame = true; //Do we want to pause/unpause the game?

public bool disableMouseLookComponent = true; //Do we want to enable/disable the MouseLook component?
//These two variables are used when disabling/enabling the MouseLook component.
public Transform ThePlayer;
public Transform TheCamera;

bool lockUnlockCursor = true; //Do we want to lock/unlock the mouse cursor?

//Storing the components
private CharacterController characterController;

[AddComponentMenu ("Inventory/Other/Inv Pause Game")]

//Checking for the Inventory object and loading in components.
void Awake (){
	if (transform.name != "Inventory")
	{
		Debug.LogError("A 'InvPauseGame' script is attached to " + transform.name + ". It needs to be attached to an 'Inventory' object.");
	}

	if (disableMouseLookComponent == true)
	{
		if (ThePlayer != null && TheCamera != null)
		{
			if (lockUnlockCursor)
			{
                characterController = GetComponent<CharacterController>();
			}
			else
			{
				Debug.LogError("The 'InvPauseGame' script on " + transform.name + " has a variable called 'disableMouseLookComponent' which is set to true though no MouseLook component can be found under (either) the Player or Camera");
				disableMouseLookComponent = false;
			}
		}
		else
		{
			Debug.LogError("The variables of the 'InvPauseGame' script on '" + transform.name + "' has not been assigned.");
			disableMouseLookComponent = false;
		}
	}
}

void Start()
{
    Time.timeScale = 1.0f;
    Time.fixedDeltaTime = 0.02f * Time.timeScale;
    if (lockUnlockCursor)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

//This function is called from the InventoryDisplay and Character script.
void PauseGame ( bool pauseIt  ){
	 //Locking the cursor
	if (lockUnlockCursor == true)
	{
		if (pauseIt == true)
		{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
		}
		else if (pauseIt == false)
		{
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
		}
	}

	//Pausing the game
	if (pauseGame == true)
	{
		if (pauseIt == true)
		{
			Time.timeScale = 0.0f;
			Time.fixedDeltaTime = 0.02f * Time.timeScale;
		}
		else
		{
			Time.timeScale = 1.0f;
			Time.fixedDeltaTime = 0.02f * Time.timeScale;
		}
	}
	
	//Disabling the MouseLook component
	if (disableMouseLookComponent == true)
	{
		if (ThePlayer != null && TheCamera != null)
		{
			if (pauseIt == true)
			{
                (GameObject.FindGameObjectWithTag("Player").GetComponent("FirstPersonController") as MonoBehaviour).enabled = false;
			}
			else
			{
                (GameObject.FindGameObjectWithTag("Player").GetComponent("FirstPersonController") as MonoBehaviour).enabled = true;
			}
		}
		else
		{
			Debug.LogError("The variables of the 'InvPauseGame' script on '" + transform.name + "' has not been assigned.");
		}
	}
}
}