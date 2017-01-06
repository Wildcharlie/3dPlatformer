using UnityEngine;
using System.Collections;

public class LoadOnEvent : MonoBehaviour {

	public string keyName;
	public string buttonName;
	public string sceneName;
	private bool isLoading;

	// Use this for initialization
	void Start () {
		isLoading = false;
		Debug.Log (keyName);
		Debug.Log (buttonName);
		Debug.Log (sceneName);
	}
	
	// Update is called once per frame
	void Update () {

		if (keyName != null && buttonName != null && sceneName != null) {
			if ((Input.GetKeyDown(keyName) || Input.GetButtonDown(buttonName)) && !isLoading) {
				Application.LoadLevel(sceneName);
				isLoading = true;
			}
		}

	}

}
