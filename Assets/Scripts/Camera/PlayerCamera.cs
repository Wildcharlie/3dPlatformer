using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {
	
	public float distanceAway = 8.0f;
	public float distanceUp = 3.0f;
	
	public float minSpeed = 12f;
	public float maxSpeed = 12f;
	public float moveAccel = 2f;
	private float startMoveTime = 0f;
	private float curMoveSpeed = 0f;
	
	public float minDegreesToRotate = 3f;
	public float allowableDistance = 10f;
//	public float timeTillRotateReset = 4f;
//	private float timeRemainingTillReset;

	public float maxRotSpeed = 300f;
	public float minRotSpeed = 2f;
	public float rotAccel = 2f;
//	private float curRotSpeed = 0f;


	private GameObject player;
	private Transform follow;
	private Vector3 cameraTargetPosition;	
	private CharacterController controller;

	public bool avoidObstacles = true;
	public float timeTillMove = 1f; // seconds for view to be obstructed until we move; avoid jitter
//	private bool viewIsClear = true;
//	private float rotateOffset = 0f;

//	private bool isMoving = false;
//	private bool isRotating = false;
	private bool holdPosition = true;



	void Start () {
		// follow the player
		player = GameObject.FindWithTag ("Player");
		follow = player.transform;			
		controller = player.GetComponent<CharacterController> ();
	}
	

	
	void Update() {

		if (!player) {
			player = GameObject.FindWithTag ("Player");
			controller = player.GetComponent<CharacterController> ();
		}


		// update camera position to follow player
		Vector3 playerTarget = controller.transform.position + controller.center + Vector3.up * distanceUp;
		cameraTargetPosition = playerTarget - (controller.transform.forward * distanceAway);
		Vector3 camToPlayerDir = (playerTarget - transform.position);
		float distance = Vector3.Distance (cameraTargetPosition, transform.position);
		Quaternion targetRotation = Quaternion.LookRotation(camToPlayerDir, Vector3.up);		
		float angle = Quaternion.Angle(transform.rotation, targetRotation);

		if (angle > minDegreesToRotate || distance > allowableDistance) {
			holdPosition = false;
		} else {
			holdPosition = true;
		}

		if (distance > 1f && startMoveTime <= 0f) {
//			isMoving = true;
			startMoveTime = Time.time;
		} else if (distance <= .1f) {
//			isMoving = false;
			startMoveTime = 0f;
		}			
				
		Debug.Log ("holdPosition: " + holdPosition);
		Debug.Log ("angle: " + angle.ToString("F1"));
		Debug.Log ("distance: " + distance.ToString("F1"));

		if (!holdPosition) {
			//curMoveSpeed += curMoveSpeed * moveAccel * Time.deltaTime;
			curMoveSpeed = Mathf.Clamp(curMoveSpeed, minSpeed, maxSpeed);
			float distCovered = (Time.time - startMoveTime) * curMoveSpeed;
			float fracDistance = distCovered / distance;
			transform.position = Vector3.Lerp(transform.position, cameraTargetPosition, fracDistance);

			// Create a rotation that is an increment closer to the target rotation from the player's rotation.
			Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, minRotSpeed * Time.deltaTime);
			
			// Change the players rotation to this new rotation.
			transform.rotation = newRotation;
		}


//
//
//
//		Quaternion lookRotation = Quaternion.LookRotation(camToPlayerDir);
//
//		float angle = Quaternion.Angle(transform.rotation, lookRotation);
//
//		curRotSpeed += minRotSpeed;
//		//curRotSpeed = Mathf.Pow (curRotSpeed, 2f) * Time.deltaTime;
//		curRotSpeed = Mathf.Clamp(curRotSpeed, minRotSpeed, maxRotSpeed);
//		float timeToComplete = angle / curRotSpeed;
//		float donePercentage = Mathf.Min(1F, Time.deltaTime / timeToComplete);
//		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, donePercentage);
//
//		// reset speed if not turning anymore
//		if (angle < minDegreesToRotate) 
//			curRotSpeed = minRotSpeed;
//


		// TODO if !viewIsClear rotate around player until it is

	}


	void FixedUpdate() {

		// viewIsClear = isViewClear();		
	
	}


	
	private bool isViewClear() {

		// only need to see player's head, not feet
		Vector3 p1 = follow.position + controller.center + Vector3.up * -controller.height * 0.2F;
		Vector3 p2 = p1 + Vector3.up * controller.height * 0.2f;
		Vector3 playerToCameraDir = p2 - transform.position;

		RaycastHit hit;
		float dist = playerToCameraDir.magnitude;
		if (Physics.SphereCast(transform.position, (controller.radius * .7f), playerToCameraDir, out hit, 100)) {
			if (hit.transform.tag == "Terrain") {
				// Debug.Log ("View obstructed, distance to hit: " + hit.distance.ToString ("F1"));
				Debug.DrawRay (transform.position, (playerToCameraDir.normalized * hit.distance), Color.white, Time.deltaTime);
				dist = hit.distance;
			}
		}	

		if (dist < playerToCameraDir.magnitude) {
			return false;
		}
		return true;
		
	}
	

}
