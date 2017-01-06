using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlashLivesRemaining : MonoBehaviour {

	private Text livesText;
	private GameController gameController;
	private HealthController healthcontroller;

	// Use this for initialization
	void Start () {
//		Debug.Log ("TXT Starting");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		healthcontroller = player.GetComponent<HealthController> ();
		livesText = gameObject.GetComponent<Text> ();
		int lives = healthcontroller.remainingLives + 1;
		livesText.text = "x " + lives.ToString();
//		Debug.Log ("Flashing lives");		
		StartCoroutine(UpdateLives());
	}

	void Update() {
		//Debug.Log ("TXT Updating");
		if (!gameController) {
			GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
			gameController = gameControllerObject.GetComponent<GameController> ();
		}
	}


	void OnEnable() {
//		Debug.Log ("TXT Enabling");
		if (gameController) {
			int lives = healthcontroller.remainingLives + 1;
			livesText.text = "x " + lives.ToString();
			StartCoroutine(UpdateLives());
		}
	}

	void OnDisable () {
//		Debug.Log ("TXT Disabling");
	}

	private IEnumerator UpdateLives() {
		yield return new WaitForSeconds(.5f);
		int lives = healthcontroller.remainingLives;
		livesText.text = "x " + lives.ToString();
		yield return null;
	}


}
