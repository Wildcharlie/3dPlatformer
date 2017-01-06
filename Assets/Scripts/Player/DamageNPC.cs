using UnityEngine;
using System.Collections;

public class DamageNPC : MonoBehaviour {

	private CharacterController controller;
	private CharacterMotor motor;

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		controller = player.GetComponent<CharacterController> ();
		motor = player.GetComponent<CharacterMotor> ();
	}

	
	// Update is called once per frame
	void Update () {
	
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {

		// enemies must have Enemy or Boss tag
		if (hit.collider.gameObject.tag != "Enemy" && hit.collider.gameObject.tag != "Boss")		   	 
			return;

		// do our own raycast to make sure we're standing on top of NPC		
		RaycastHit rayhit;
		Vector3 position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
		if (Physics.Raycast(position, -transform.up, out rayhit)) {

			//Debug.Log ("downward hit distance: " + rayhit.distance.ToString("F1"));


			// make sure ray hit same enemy that the controller collided with
			GameObject npc = rayhit.collider.gameObject;
			if (npc.tag != "Enemy" && npc.tag != "Boss")
				return;

			Debug.DrawRay(position, -transform.up * rayhit.distance, Color.magenta, Time.deltaTime, false);				

			// if npc is below player, assume we jumped on int and should damage it
			NPCController npccontroller = npc.GetComponent<NPCController>();
			if (rayhit.distance <= .2f && !npccontroller.damaged) {
				//Debug.Log ("Damaging NPC");
				// npc takes damage if vulnerable
				// and player should be sprung upwards
				npccontroller.SendMessage("TakeDamage");
				motor.SendMessage("StartJump");
			}

		}

	}

	void OnCollisionEnter(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
		Debug.Log ("Collision: " + collision.gameObject.tag);

	}



}
