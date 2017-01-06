using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour {
	
	public bool fallWhenWalkedOn = false;
	public float whenToFall = 2f;
	public float fallSpeed = 1f;
	public float fallAcceleration = .2f;
	public Color triggeredColor;
	public float secondsTillReset = 10f; // time after fall to reset
	public float secondsAfterFallTillDisappear = 2f;

	private float fallTimeElapsed = 0f;
	private bool triggered = false;
	private bool falling = false;
	private float timeElapsed = 0f; // time waiting after trigger for action

	private CharacterController playerController;
	private Vector3 originalPosition;
	private Color originalColor;


	void Start() {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player != null)
			playerController = player.GetComponent<CharacterController> ();
		originalPosition = transform.position; // remember for when we reset the platform
		originalColor = gameObject.GetComponent<Renderer> ().material.color;
	}


	void Update () {
		
		if (fallWhenWalkedOn) {
			
			if (triggered) {
				timeElapsed += Time.deltaTime;
			}
			
			if (timeElapsed > whenToFall && !falling) {
				falling = true;			
			}
			
			if (falling)
				Fall();

			// fell below screen, reset
			if (falling && transform.position.y <= -100f)  {
				StartCoroutine("ResetPlatform", secondsTillReset);
			}

		}
		

	}

	
	public void Trigger() {

		// have to check fallWhenWalkedOn because this may be called via message instead of Update		
		if (!fallWhenWalkedOn)
			return;

		if (!triggered) {
			triggered = true;
			timeElapsed = 0f; // reset these
			
			// change material color
			gameObject.GetComponent<Renderer>().material.color = triggeredColor;
		}
		
	}
	
	private void Fall() {

		// move downward
		float newY = gameObject.transform.position.y - (timeElapsed * fallAcceleration) - (fallSpeed * Time.deltaTime);
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, 
		                                              new Vector3 (gameObject.transform.position.x, newY, gameObject.transform.position.z),
		                                              Time.deltaTime);

		// fade color to white
		float change = (fallSpeed * Time.deltaTime);
		Color newColor = new Color (gameObject.GetComponent<Renderer>().material.color.r + change, gameObject.GetComponent<Renderer>().material.color.g + change, gameObject.GetComponent<Renderer>().material.color.b + change);
		gameObject.GetComponent<Renderer>().material.color = newColor;

		
		fallTimeElapsed += Time.deltaTime;
		if (fallTimeElapsed >= secondsAfterFallTillDisappear) {
			fallTimeElapsed = 0f;
			gameObject.GetComponent<Renderer>().enabled = false;
			falling = false;
		}
	}

	
	private IEnumerator ResetPlatform (float seconds) {
		
		// stop falling
		timeElapsed = 0f;
		triggered = false;
		timeElapsed = 0f;

		// wait till reset timer goes off
		yield return new WaitForSeconds (seconds);
		
		// restore appearance
		transform.position = originalPosition; 
		gameObject.GetComponent<Renderer>().material.color = originalColor;
		gameObject.GetComponent<Renderer>().enabled = true;
		
	}


	void OnCollisionEnter(Collision collision) {

		if (!fallWhenWalkedOn)
			return;

		foreach (ContactPoint contact in collision.contacts) {
			// make sure player is above platform; don't trigger if hit from below
			if (contact.otherCollider.tag == "Player" && playerController.transform.position.y > transform.position.y)
				Trigger ();

		}
	}


}
