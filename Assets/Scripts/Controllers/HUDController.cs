using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {

	public float timeLimit = 30f;

	public Text timerText; // hud text
	public Text scoreText;
	public Text hitPointsText;
	public Text livesText;

	public float textEffectScale = 2f;
	public float textEffectSpeed = .5f;

	private float startTime; // timekeeping	
	private ScoreController scoreController;
	private HealthController healthController;
	private LevelTimer levelTimer;

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindWithTag ("Player");
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		scoreController = gameControllerObject.GetComponent<ScoreController> ();	
		healthController = player.GetComponent<HealthController> ();	

		GameObject levelTimerObject = GameObject.FindWithTag ("Level");
		levelTimer = levelTimerObject.GetComponent<LevelTimer> ();	
	}
	
	// Update is called once per frame
	void Update () {

		// update timer
		timerText.text = levelTimer.GetTimeRemaining();

		// update score
		if (int.Parse(scoreText.text) != scoreController.score) 
			StartCoroutine( ChangeText(scoreController.score.ToString(), scoreText) );

		// update hp and lives text with scale effect
		if (int.Parse(hitPointsText.text) != healthController.remainingHitPoints) 
			StartCoroutine( ChangeText(healthController.remainingHitPoints.ToString(), hitPointsText) );

		if (int.Parse(livesText.text) != healthController.remainingLives) 
			StartCoroutine( ChangeText(healthController.remainingLives.ToString(), livesText) );		

	}


	IEnumerator ChangeText(string newTextValue, Text textField) {
	
		// new text should start at 125% scale and highlight color 
		// then shrink and fade to normal color
		Vector3 normalScale = new Vector3 (1f, 1f, 1f);
		textField.text = newTextValue;
		textField.rectTransform.localScale = new Vector3(textEffectScale, textEffectScale, textEffectScale);

		while (textField.rectTransform.localScale.x > normalScale.x) {
			textField.rectTransform.localScale = new Vector3(textField.rectTransform.localScale.x - (textEffectSpeed * Time.deltaTime), 
			                                                 textField.rectTransform.localScale.y - (textEffectSpeed * Time.deltaTime),
			                                                 textField.rectTransform.localScale.z - (textEffectSpeed * Time.deltaTime));
			yield return null;
		}

		textField.rectTransform.localScale = normalScale;
			
	}


}



