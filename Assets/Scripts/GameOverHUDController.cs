using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverHUDController : MonoBehaviour {

	public GameObject scoreTextObj;
	public GameObject mineralsTextObj;
	public GameObject timerTextObj;
	public GameObject restartButton;
	public GameObject quitButton;
	
	private Text scoreText;
	private Text mineralsText;
	private Text timerText;
	private ScoreController scoreController;
	private LevelTimer levelTimer;
	
	// Use this for initialization
	void Start () {

		Debug.Log ("Starting!");

		scoreText = scoreTextObj.GetComponent<Text> ();
		mineralsText = mineralsTextObj.GetComponent<Text> ();
		timerText = timerTextObj.GetComponent<Text> ();

		GameObject gameControllerObj = GameObject.FindGameObjectWithTag ("GameController");
		scoreController = gameControllerObj.GetComponent<ScoreController> ();

		GameObject level = GameObject.FindGameObjectWithTag ("Level");
		levelTimer = level.GetComponent<LevelTimer> ();


		StartCoroutine("ShowScore");

	}

	void OnEnable() {
		StartCoroutine("ShowScore");
	}

	IEnumerator ShowScore() {

		// as long as the GameOverscreen starts out as inactive,
		// this is only called when the parent is enabled
		// which may be before the Start function is called

		// if we haven't set up yet, stop
		if (!mineralsText || !scoreText)
			yield break;

		restartButton.SetActive (false);
		quitButton.SetActive (false);
		
		if (!levelTimer) {
			GameObject gameControllerObj = GameObject.FindGameObjectWithTag ("GameController");
			scoreController = gameControllerObj.GetComponent<ScoreController> ();
			
			GameObject level = GameObject.FindGameObjectWithTag ("Level");
			levelTimer = level.GetComponent<LevelTimer> ();
		}

		// minerals shows total, score shows 0
		// then minerals counts down as score increases
		// after score is finished, show buttons

		int minerals = scoreController.score;
		int curMinerals = minerals;
		int curScore = 0;

		mineralsText.text = minerals.ToString();	
		timerText.text = levelTimer.GetTimeRemaining ();

		while (curMinerals >= 0) {

			scoreText.text = curScore.ToString();

			curScore += 100;
			curMinerals -= 1;
			yield return new WaitForSeconds(.2f);
		}

		restartButton.SetActive (true);
		quitButton.SetActive (true);

		Debug.Log ("Showing score done");

	}


	// Update is called once per frame
	void Update () {
	
	}
}
