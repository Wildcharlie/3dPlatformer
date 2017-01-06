using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public string level1SceneName = "level01";
	public string level2SceneName = "level02";
	public string level3SceneName = "level00";
	
	public int currentLevel = 1;

	public bool showIntro = false;

	// public so we can check the status in our controllers
	public bool isPaused = false;
	public bool isGameOver = false;
	public bool isPlaying = true;
	public bool isLevelEnd = false;

	public GameObject pauseCanvas; // need to manually wire this up in IDE
	public GameObject loseLifeCanvas;
	public GameObject gameOverCanvas;
	public GameObject hudCanvas;
	public GameObject levelWinCanvas;
	//private int loseScene = 1; // scene # from build settings
	
	private HealthController healthController;
	private ScoreController scoreController;
	private LevelTimer levelTimer;

	public GameObject levelIntro; // need to manually wire this up in IDE
//	private bool isPlaying = false;

	private GameObject player;
	private CharacterMotor motor;
	private Animator playerAnim; 
	private Vector3 playerSpawnPoint;
	private Quaternion playerSpawnRotation;
	private Vector3 cameraSpawnPoint;
	private Quaternion cameraSpawnRotation;
	private GameObject levelRoot;
	private CameraMovement camMove;

	// Use this for initialization
	void Start () {


		player = GameObject.FindWithTag ("Player");
		motor = player.GetComponent<CharacterMotor> ();
		healthController = player.GetComponent<HealthController> ();
		scoreController = gameObject.GetComponent<ScoreController> ();
		playerAnim = player.GetComponent<Animator> ();

		RestartLevel ();

	}

	// Update is called once per frame
	void Update () {
	
		// pause/unpause
		if (Input.GetButtonDown ("Pause") && !pauseCanvas.activeInHierarchy) {
			Pause ();
		} else if (Input.GetButtonDown ("Pause") && pauseCanvas.activeInHierarchy) {
			Unpause ();
		}

		//lose conditions
		if (healthController.remainingHitPoints < 0 && healthController.remainingLives >= 0) {		 // 
			StartCoroutine ("LoseLife");
		} else if (healthController.remainingHitPoints <= 0 && healthController.remainingLives < 0) {				
			StartCoroutine("GameOver");

		}
	
	}
		
	public void Pause() {
		pauseCanvas.SetActive(true);
		isPlaying = false;
		isPaused = true;
		camMove.isPaused = true;
	}

	public void Unpause() {	
		pauseCanvas.SetActive(false);
		isPlaying = true;
		isPaused = false;
		camMove.isPaused = false;
	}

	public void EndGame() {
		// public wrapper for gameover coroutine
		StartCoroutine ("GameOver");
	}
	
	IEnumerator GameOver() {
		Debug.Log ("GAME OVER!");	
		if (!isGameOver) {
			// finish death animation
			yield return new WaitForSeconds(3.8f);
			isGameOver = true;
			isPlaying = false;
			hudCanvas.SetActive(false);
			gameOverCanvas.SetActive(true);
		}
		yield return null;
		
	}
	
	public void EndLife() {
		// public wrapper for gameover coroutine
		StartCoroutine ("LoseLife");
	}

	IEnumerator LoseLife() {
		Debug.Log ("Losing life!!!");
		if (!isGameOver) {
			// finish death animation
			yield return new WaitForSeconds(3.8f);
			isGameOver = true;
			isPlaying = false;
			hudCanvas.SetActive(false);
			healthController.remainingLives -= 1;
			healthController.remainingHitPoints = healthController.startingHitPoints;
			loseLifeCanvas.SetActive(true);
			yield return null;
		}
		yield return null;
	}

	// TODO can't just restart, have to reset all npcs minerals etc... might as well reload level 
	// requires game controller to be outside level so it won't get destroyed
	// should this contain the health logic instead of the player character?

	IEnumerator LoadLevelAction() {
		
		levelRoot = GameObject.FindGameObjectWithTag("Level");
		if (levelRoot) { // if level object is in scene, destroy and reload
			Destroy (levelRoot);
		}
		
		if (currentLevel == 1) {
			Application.LoadLevelAdditive(level1SceneName);
		} else if (currentLevel == 2) {
			Application.LoadLevelAdditive(level2SceneName);
		} else if (currentLevel == 3) {
			Application.LoadLevelAdditive(level3SceneName);
		}
		// wait a frame for loadlevel to take effect
		yield return 0;

		levelRoot = GameObject.FindGameObjectWithTag("Level");


		levelTimer = levelRoot.GetComponent<LevelTimer> ();	
		levelTimer.ResetTimer ();
		
		healthController.Reset ();
		scoreController.score = 0;

		GameObject cam = GameObject.FindGameObjectWithTag ("MainCamera");
		camMove = cam.GetComponent<CameraMovement> ();
		camMove.isPaused = false;
		
		GameObject respawn = GameObject.FindGameObjectWithTag ("Respawn");
		playerSpawnPoint = respawn.transform.position;
		playerSpawnRotation = respawn.transform.rotation;		
		player.transform.position = playerSpawnPoint;
		player.transform.rotation = playerSpawnRotation;

		// reset animations (after game over)
		playerAnim.SetBool ("Damaged", false);
		playerAnim.SetBool ("FallStart", false);
		playerAnim.SetBool ("JumpStart", false);
		playerAnim.SetBool ("JumpLoop", false);
		playerAnim.SetBool ("JumpEnd", false);
		playerAnim.SetBool ("DoubleJump", false);
		playerAnim.SetTrigger ("Alive");
		
		hudCanvas.SetActive(true);
		loseLifeCanvas.SetActive(false);
		gameOverCanvas.SetActive(false);
		levelWinCanvas.SetActive (false);
		
		isGameOver = false;
		isPlaying = true;
		isLevelEnd = false;
		motor.fallingDeath = false;
		healthController.isDead = false;
		
		Unpause (); // need to unpause in case we have left a paused scene behind
		
		// wait for player to hit start before upausing and playing
		if (showIntro) {
			isPlaying = false;
			levelIntro.SetActive(true);
		} else {
			StartLevel ();
		}


	}


	public void RestartLevel() {
		// loadleveladditive requires a frame to set up, so we use RestartLevel() as a public function
		// that calls a coroutine to load the level and wait a frame for it to load before acting upon it
		StartCoroutine (LoadLevelAction ());
	}


	public void StartLevel() {
		Debug.Log ("Starting level!");
		levelIntro.SetActive(false);
		hudCanvas.SetActive(true);
		isGameOver = false;
		isPaused = false;
		Unpause (); // resumes timescale = 1
	}


	public void QuitGame() {
		Application.Quit();
	}


	public void RestartGame() {
		healthController.Reset ();
		currentLevel = 1;
		RestartLevel ();
	}

	public void NextLevel() {
		currentLevel = Mathf.Min (3, currentLevel+1);
		RestartLevel ();
	}

	public void WinLevel() {
		StartCoroutine (WinLevelAction ());
	}


	IEnumerator WinLevelAction() {
		if (!isLevelEnd) {

			// wait until jump is landed
			while (!motor.isNearlyGrounded()) {
//				player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position + (-player.transform.up * Time.deltaTime), 1f);
				yield return null;
			}

			// turn off HUD
			isLevelEnd = true;
			hudCanvas.SetActive(false);

			// do turning animation
			playerAnim.SetFloat("speed", 0f);
			playerAnim.SetFloat("angularVelocity", 0f);

			// rotate to face camera
			Vector3 playerToCamDir = Camera.main.transform.position - player.transform.position;
			Vector3 playerForwardDir = player.transform.forward;
			playerToCamDir.y = 0f;
			playerForwardDir.y = 0f;
			float angleDiff = Vector3.Angle(playerToCamDir, playerForwardDir);
			float angleDiffAllowed = 2f;
			while (angleDiff > angleDiffAllowed) {				
				// rotate player toward us
				playerToCamDir = Camera.main.transform.position - player.transform.position;
				playerForwardDir = player.transform.forward;
				playerToCamDir.y = 0f;
				playerForwardDir.y = 0f;
				angleDiff = Vector3.Angle(playerToCamDir, playerForwardDir);
				Quaternion lookAtRotation = Quaternion.LookRotation(playerToCamDir);
				player.transform.rotation = Quaternion.Slerp(player.transform.rotation, lookAtRotation, Time.deltaTime * 10f);	
				yield return null;
			}

			// wait a second in idle
			playerAnim.SetFloat("angularVelocity", 0f);
			playerAnim.SetBool("JumpStart", false);
			playerAnim.SetBool("JumpLoop", false);
			playerAnim.SetBool("JumpEnd", false);
			playerAnim.SetBool("DoubleJump", false);
			playerAnim.SetBool("FallStart", false);
			playerAnim.SetBool("Climbing", false);
			playerAnim.SetBool("Hanging", false);

			// do victory animation, wait for it to finish
			playerAnim.SetTrigger("Victory");
			yield return new WaitForSeconds(2.0f);

			// show in canvas
			levelWinCanvas.SetActive(true);

		}
		yield return null;
	}
	
}
