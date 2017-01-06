using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformMove : MonoBehaviour {
	
	public Vector3 moveDirection = Vector3.up;
	public float moveDistance = 3f;
	public float moveSpeed = 1f;
	public float timeDelay = 0f;
	
	private CharacterController playerController;
	private Vector3 originalPosition;

	private bool isMoving = false;
	public Vector3 currentDirection {get; private set;}
	private float distanceMoved = 0f; // change direction timer

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player != null)
			playerController = player.GetComponent<CharacterController> ();
		originalPosition = transform.position; // remember for when we reset the platform
		currentDirection = moveDirection;
	}
	
	// Update is called once per frame
	void Update () {
			
		if (timeDelay > 0f) {
			timeDelay -= Time.deltaTime;
		} else {
			if (!isMoving) {
				isMoving = true;
				StartCoroutine(moveInDirection());
			}
		}

	}

	IEnumerator moveInDirection() {
		distanceMoved = 0f;
		while (distanceMoved < moveDistance) {
			distanceMoved += Time.deltaTime * moveSpeed;
			Vector3 newPosition = transform.position + (currentDirection * Time.deltaTime * moveSpeed);
			transform.position = newPosition;
//			Debug.Log ("distanceMoved: " + distanceMoved.ToString("F1"));
			yield return null;
		}
		currentDirection = -currentDirection;
		//Debug.Log ("changing direction: " + currentDirection);
		isMoving = false;
		yield return null;
	}


}
