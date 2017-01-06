using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour {

	GameObject player;
	HealthController playerHealth;
	CharacterMotor motor;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		if (player == null)
						return;
		playerHealth = player.GetComponent<HealthController> ();
		motor = player.GetComponent<CharacterMotor> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			playerHealth.FallDeath ();
			motor.fallingDeath = true;
		}
	}


}
