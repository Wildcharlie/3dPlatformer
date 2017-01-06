using UnityEngine;
using System.Collections;

public class DestroyOnClick : MonoBehaviour {

	public void DestroySelf () {

		Destroy (gameObject); // destroy the object this script is attached to and its children
	
	}


}
