using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour {


	public float rotationAmount = 2f;

	void Update () {
	
		transform.Rotate (Vector3.up * (rotationAmount * Time.deltaTime));

	}
}