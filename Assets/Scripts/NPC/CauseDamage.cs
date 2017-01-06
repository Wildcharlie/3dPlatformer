using UnityEngine;
using System.Collections;

public class CauseDamage : MonoBehaviour {
	
	public float damagePoints = 1f;
	private HealthController healthController;

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindWithTag ("Player");
		if (player != null)
			healthController = player.GetComponent<HealthController> ();
	}
		
	/* 
	 * Detecting collisions: 
	 * If the object hits the player, it's detected here.
	 * If the player hits the object, it's detected by the player's TakeDamage script.
	*/

	void Update() {


	}


	void OnCollisionEnter(Collision collision) {

		if (collision.gameObject.tag == "Player") {
			if (healthController != null) {
				healthController.SendMessage("ApplyDamage", damagePoints);
			}
		}

	}
	
}
