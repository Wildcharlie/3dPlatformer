using UnityEngine;
using System.Collections;

public class CollectObject : MonoBehaviour {

	public GameObject particleEffectPrefab; // particle explosions
	public AudioClip collectSound; // sound fx
	public int objectPoints = 1;
	private GameObject gameController;
	private ScoreController scoreController;
	private GameObject levelRoot;

	void Start() {	
		gameController = GameObject.FindWithTag ("GameController");
		if (gameController != null) {
			scoreController = gameController.GetComponent<ScoreController> ();	
			levelRoot = GameObject.FindGameObjectWithTag ("Level");
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") { 


			// show particles
			GameObject particleObject = (GameObject) Instantiate(particleEffectPrefab, transform.position, transform.rotation);

			if (levelRoot == null) 
				levelRoot = GameObject.FindGameObjectWithTag ("Level");
			particleObject.transform.parent = levelRoot.transform;

			// play sound
			AudioSource.PlayClipAtPoint(collectSound, transform.position);

			// increment score
			if (scoreController != null)
				scoreController.score += objectPoints;
			
			// destroy mineral and particle after collection
			Destroy(gameObject, .2f);
			Destroy(particleObject, 2f); // destroy in 2 sec

		} 		
	}
	

}
