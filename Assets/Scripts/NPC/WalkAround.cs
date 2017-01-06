using UnityEngine;
using System.Collections;

public class WalkAround : MonoBehaviour {

	public float walkSpeed = 1f;
	public float rotateSpeed = 20f;
	public float turnDegrees = 180f;
	public float secondsTillTurn = 3f;

	private float timeUntilTurn; // countdown clock till turning
	private bool isTurning = false;
	private float anglesTurnedSoFar = 0f;

	// Use this for initialization
	void Start () {
		timeUntilTurn = secondsTillTurn;
	}
	
	// Update is called once per frame
	void Update () {
	
		timeUntilTurn -= Time.deltaTime;

		if (timeUntilTurn <= 0 && !isTurning) {
//			Debug.Log ("Starting Turn");
			isTurning = true;
		}

		if (isTurning && anglesTurnedSoFar < turnDegrees) {
//			Debug.Log ("Continuing Turn");
			transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);		
			anglesTurnedSoFar += rotateSpeed * Time.deltaTime;
		} else if (isTurning && anglesTurnedSoFar >= turnDegrees) {
//			Debug.Log ("Ending Turn");
			isTurning = false;
			timeUntilTurn = secondsTillTurn;
			anglesTurnedSoFar = 0f;
		}

		// don't move when turning
		if (!isTurning)
			transform.position = Vector3.Lerp (transform.position, transform.position + transform.forward * walkSpeed, Time.deltaTime);

	}


	void OnCollisionEnter(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
//			Debug.Log ("contact: " + contact);
			if (contact.normal != Vector3.up) {
				Debug.DrawRay(contact.point, contact.normal, Color.white);
				timeUntilTurn = 0f;
				anglesTurnedSoFar = 0f;
			}
		}
	}


}
