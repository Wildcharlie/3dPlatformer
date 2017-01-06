using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// add gamepad support to gui buttons

public class GUIController : MonoBehaviour {

	public GUIStyle focusedButtonGUIStyle;

	// get list of buttons in gui
	private GameObject gui;
	private Button[] buttons;


	// Use this for initialization
	void Start () {
//		gui = GameObject.FindGameObjectWithTag ("GUI");
//		buttons = gui.GetComponentsInChildren<Button> ();
//
//		// set first button to focused
//		foreach (Button button in buttons) {
//			Debug.Log(button.name);
//		}


	}

//	void OnGUI() {
//		Debug.Log ("Setting focus to: " + buttons[0].name);
////		GUI.SetNextControlName(buttons[0].name);
////		GUI.FocusControl(buttons[0].name);
//		EventSystem.current.SetSelectedGameObject(buttons[0], null);
//	}

	// Update is called once per frame
	void Update () {
	
	}
}
