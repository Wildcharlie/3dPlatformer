using UnityEngine;
using System.Collections;

public class AttackNearby : MonoBehaviour {

	public float attackRange = 2f;
	private GameObject player;
	private Vector3 ourPosition;
	private Animator animator;


	// Use this for initialization
	void Start () {	
		animator = gameObject.GetComponent<Animator> ();
		ourPosition = gameObject.transform.position;
		player = GameObject.FindGameObjectWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () {
	
		Vector3 playerPosition = player.transform.position;
		float distance = Vector3.Distance (ourPosition, playerPosition);

		if (distance <= attackRange)
			animator.SetTrigger("attack");

	}
}
