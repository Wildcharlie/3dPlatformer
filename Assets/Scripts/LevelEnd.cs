using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour {

	private GameController gameController;
	private SpinObject spinObjectScript;

	// Use this for initialization
	void Start () {
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent<GameController> ();
		spinObjectScript = gameObject.GetComponent<SpinObject> ();
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			StartCoroutine(StarVictoryMovement());
			if (gameController != null)
				gameController.WinLevel ();
		}
	}

	
	IEnumerator StarVictoryMovement() {
	
		// stop current rotation script
		spinObjectScript.rotateEnabled = false;

		// now rotate with increasing speed and move upward
		// until max speed is reached, then disappear into puff
		float maxSpeed = 1000f;
		float curSpeed = 200f;
		while (curSpeed < maxSpeed) {
			curSpeed += Time.deltaTime * curSpeed;
			transform.Rotate(Vector3.up, curSpeed * Time.deltaTime);
			transform.position += Vector3.up * Time.deltaTime * .75f;
			yield return null;
		}
		gameObject.SetActive (false);


	}

}
