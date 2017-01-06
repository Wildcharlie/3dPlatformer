using UnityEngine;
using System.Collections;

public class LoadAdditive : MonoBehaviour {

	// Use this for initialization
	public void LoadAddOnClick (int level) {
	
		Application.LoadLevelAdditive (level);

	}

}
