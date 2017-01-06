using UnityEngine;
using System.Collections;

public class PlayerNPCDamage : MonoBehaviour {

	public float hazardDamagePoints = 1f;
	public float npcAboveAmount = -.3f; // vertical distance player must be from top of npc to be npc vs player damage
	private HealthController healthController;
	private ShakeObject cameraShake;
	private CharacterController controller;
	private CharacterMotor motor;

	private Vector3 p1 = Vector3.zero;
	private Vector3 p2 = Vector3.zero;

	void Start () {
		healthController = gameObject.GetComponent<HealthController> ();	
		controller = gameObject.GetComponent<CharacterController> ();
		motor = gameObject.GetComponent<CharacterMotor> ();
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {

		if (hit.gameObject.tag == "Hazard") {
			healthController.ApplyDamage(hazardDamagePoints);
		}

		if (hit.gameObject.tag == "Enemy" || hit.gameObject.tag == "Boss") {


			// check if enemy is dangerous/vulnerable
			NPCController npccontroller = hit.gameObject.GetComponent<NPCController>();
			if (npccontroller.damaged)
				return;

			// hit an enemy that we're not on top of -> take damage
			Vector3 playerFeetPosition = transform.position + controller.center + Vector3.up * -controller.height/2;
			BoxCollider npcCollider = hit.gameObject.GetComponent<BoxCollider>();
			Vector3 npcPosition = hit.gameObject.transform.position + npcCollider.center * hit.gameObject.transform.localScale.x;
			Vector3 npcHeadPosition = new Vector3(npcPosition.x, npcPosition.y + npcCollider.bounds.size.y/2, npcPosition.z);
			p1 = playerFeetPosition;
			p2 = npcHeadPosition;		
			float yDiff = playerFeetPosition.y - npcHeadPosition.y;
			//Debug.Log ("yDiff: " + yDiff);
			if (yDiff < npcAboveAmount) {
				float damagePoints = npccontroller.points;
				healthController.ApplyDamage(damagePoints);
				Vector3 pushDir = -hit.moveDirection;
				// if this isn't death, move him backwards
				if (healthController.remainingHitPoints > 0)
					motor.ApplyForce(pushDir, 20f);
			} else {
				npccontroller.TakeDamage();
				//Debug.Log("pushing player up!");
				//motor.TriggerJump(); // make him bounce up in reaction
			}

		}
	}


	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(p1, .3f);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(p2, .3f);
	}


}
