using UnityEngine;
using System.Collections;

public class LevelTimer : MonoBehaviour {

	public string levelName = "Demo Level";
	public float timeLimit = 120f; // seconds for level timer
	private float startTime;

	// Use this for initialization
	void Start () {
		ResetTimer ();
	}
	
	// Update is called once per frame
	public string GetTimeRemaining () {
		float timeElapsed = Time.time - startTime;
		float timeRemaining = timeLimit - Mathf.Round(timeElapsed);
		int minutes = Mathf.FloorToInt(timeRemaining / 60F);
		int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);
		string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
		return (niceTime);
	}

	public int GetSecondsRemaining () {
		float timeElapsed = Time.time - startTime;
		float timeRemaining = timeLimit - Mathf.Round(timeElapsed);
		int seconds = Mathf.FloorToInt(timeRemaining * 60);
		return (seconds);
	}

	public void ResetTimer() {
		startTime = Time.time;
	}

}
