using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CameraMovement : MonoBehaviour
{
	public float smooth = 1.5f; // relative speed at which the camera will catch up
	public float rotateSmooth = 2.5f;
	private float minFollowDistance = 7f; // default distance
	public float maxFollowDistance = 10f; // distance we can be before camera will move
	public float followHeight = 2f; // height above player to follow at
	private float angleDifferenceAllowed = 2f; // allowable difference in rotations between camera andn player before moving cam to update
	public float joystickRotationForce = 3f; // camera rotation speed when joystick is pressed
	public float timeToHoldPositionAfterPlayerInput = 1f; // after releasing joystick camera pauses for thid long
	public bool isPaused = false; // disables camera movement, used when we fall off screen

	private GameObject playerObject;
	private Transform player;
	private Vector3 playerHeadPos; 
	private Vector3 playerFeetPos; 
	private CharacterController controller;
	private GameController gameController;


	private int zoomLevel = 0;
	private float[] zoomDistance = new float[3] {7f, 10f, 13f};
	private float[] zoomHeight = new float[3] {1f, 4f,14f};
	private bool zooming = false;
	private bool isRotating = false;
	private float cameraRotationSpeed = .5f;
	private float mediumCameraRotationSpeed = 1.4f;
	private float fastCameraRotationSpeed = 6f;

	private float rightJoystickThreshold = .98f;
	private float deadZoneThreshold = .16f;

	void Start ()
	{
		// Setting up the reference.
		playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;

		// if there's no player in scene, we are just testing the level
		if (player == null)
			return;

		// store refs
		controller = player.GetComponent<CharacterController> ();
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent <GameController> ();

		isPaused = false;

		// set starting positions
	}

	void Update ()
	{

		// update position of player's head and feat
		playerHeadPos = player.position + controller.center + (controller.height / 2f * player.transform.up);
		playerFeetPos = playerHeadPos + (controller.height * -player.transform.up) + (controller.height / 10f * player.transform.up);

		// dont change cam position once level ends
		if (gameController != null)
		if (gameController.isLevelEnd)
			return;

		// don't move camera but do rotate to track player if paused
		if (isPaused) {
			SmoothLookAt ();
			return;
		}
				
		// player moves right joystick, adjusts camera
		Vector2 playerInput = getPlayerInput ();

		if (zooming && Mathf.Abs(playerInput.y) < rightJoystickThreshold) {
			zooming = false;
		}

		if (playerInput.y > rightJoystickThreshold && !zooming) {
			zooming = true;
			zoomLevel++;
			zoomLevel = Mathf.Clamp (zoomLevel, 0, 2);
		} else if (playerInput.y < -rightJoystickThreshold && !zooming) {
			zooming = true;
			zoomLevel--;
			zoomLevel = Mathf.Clamp (zoomLevel, 0, 2);
		}
//		if (isZoomedIn && playerInput.y > rightJoystickThreshold) {
//			isZoomedIn = false;
//		} else if (!isZoomedIn && !isZoomedOut && playerInput.y > rightJoystickThreshold) {
//			isZoomedOut = true;
//		} else if (!isZoomedIn && playerInput.y < -rightJoystickThreshold) {
//			isZoomedIn = true;
//		}

		if (playerInput.x > rightJoystickThreshold && !isRotating) {
			transform.RotateAround (playerHeadPos, Vector3.up, -mediumCameraRotationSpeed);
			//StartCoroutine (Rotating (-cameraRotationDegrees, mediumCameraRotationSpeed));
		} else if (playerInput.x < -rightJoystickThreshold && !isRotating) {
			transform.RotateAround (playerHeadPos, Vector3.up, mediumCameraRotationSpeed);
			//StartCoroutine (Rotating (cameraRotationDegrees, mediumCameraRotationSpeed));
		}

		if (Input.GetButtonDown ("LeftTrigger")) {
			UpdatePosition (true);
		} else {
			UpdatePosition();
		}

		SmoothLookAt();
	}

	Vector2 getPlayerInput() {
		Vector2 playerInput = new Vector2 (Input.GetAxisRaw ("RightH"), Input.GetAxisRaw ("RightV"));

		if (Mathf.Abs (playerInput.x) <= deadZoneThreshold) {
			playerInput.x = 0;
		}

		if (Mathf.Abs (playerInput.y) <= deadZoneThreshold) {
			playerInput.y = 0;
		}

		return playerInput;	
	}

	float getRotationDirection() {
		float rotation = getRotation(player.transform.eulerAngles.y, transform.eulerAngles.y);
		return (rotation < 0) ? -1 : 1;
	}

	IEnumerator Rotating (float rotation, float rotSpeed)
	{
		isRotating = true;
		int direction = (rotation < 0) ? -1 : 1;

		rotation /= rotSpeed * direction;

		for(int i = 0; i < rotation; i++) {
			transform.RotateAround (playerHeadPos, Vector3.up, direction * rotSpeed);
			yield return 0;
		}
		isRotating = false;
	}

	void UpdatePosition(bool instant = false) {
		float distance = zoomDistance[zoomLevel];
		float height = zoomHeight[zoomLevel];
		Vector3 newPos = transform.position;
		float distToForward = playerObject.GetComponent<CharacterMotor> ().getDistForward ();

		Vector3 relPosition = playerHeadPos - transform.position;
		Vector3 newForward = new Vector3 (transform.forward.x, 0f, transform.forward.z);

		newPos = playerHeadPos + (-newForward * distance) + (Vector3.up * height);

		// Only rotate if player isn't moving and didn't move the camera with the stick
		if (playerObject.GetComponent<CharacterMotor> ().getCurrentSpeed () == 0 && !isRotating && Mathf.Abs(getRotation(player.transform.eulerAngles.y, transform.eulerAngles.y)) > angleDifferenceAllowed) {
			transform.RotateAround (playerHeadPos, Vector3.up, getRotationDirection() * cameraRotationSpeed);
		}

		// Camera speed needs to speed up if the camera gets too far, slow down if camera is too close
		if (instant) {
			StartCoroutine(Rotating(getRotation(player.transform.eulerAngles.y, transform.eulerAngles.y), fastCameraRotationSpeed));
		} else if (Vector3.Magnitude (relPosition) > distance) {
			transform.position = Vector3.Lerp (transform.position, newPos, 0.04f * Mathf.Pow((Vector3.Magnitude(relPosition)), 2) * Time.deltaTime);
		} else {
			transform.position = Vector3.Lerp (transform.position, newPos, 0.018f * Mathf.Pow((Vector3.Magnitude(relPosition)), 2) * Time.deltaTime);
		}
	}

	float getRotation (float firstRot, float secondRot)
	{
		float rotation = firstRot - secondRot;
		float newRotation = rotation;
		// Fix for some weird glitch that causes the rotation difference to be greater than 180 degrees
		if (Mathf.Abs (rotation) > 180) {
			newRotation = 360 - Mathf.Abs(rotation);
			newRotation = (rotation < 0) ? newRotation : newRotation * -1f;
		}

		return newRotation;
	}

	void SmoothLookAt ()
	{
		// Create a vector from the camera towards the player's head
		Vector3 relPlayerPosition = playerHeadPos - transform.position;

		// Create a rotation based on the relative position of the player being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);
		Vector3 currentRotation = transform.eulerAngles;

		// Can rotate each axis independently
		transform.eulerAngles = new Vector3(
			Mathf.LerpAngle(currentRotation.x, lookAtRotation.eulerAngles.x, rotateSmooth * Time.deltaTime),
			Mathf.LerpAngle(currentRotation.y, lookAtRotation.eulerAngles.y, rotateSmooth * Time.deltaTime),
			Mathf.LerpAngle(currentRotation.z, lookAtRotation.eulerAngles.z, rotateSmooth * Time.deltaTime)
		);
	}
}

