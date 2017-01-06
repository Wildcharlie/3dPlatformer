using UnityEngine;
using System.Collections;


public class SpinObject : MonoBehaviour {

	public bool rotateEnabled = true;
	public float rotateSpeed = 50.0f;



	void Start () {

		// set a random starting rotation so they don't all sync up
		float yRotation = (Random.Range (0, 359.0F));
		Quaternion quat = new Quaternion (0, yRotation, 0, 0);
		transform.rotation = quat;

	}
	
	// Update is called once per frame
	void Update () {

		if (rotateEnabled)
			transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
	}
	
}
