using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour {


	// walking back and forth
	public bool walkAround = true;
	public float walkSpeed = 1f;
	public float rotateSpeed = 20f;
	public float turnDegrees = 180f;
	public float secondsTillTurn = 3f;	
	public float gravity = 19f;
	public float terminalVelocity = 12f;
	private float timeUntilTurn; // countdown clock till turning
	private bool isTurning = false;
	private float anglesTurnedSoFar = 0f;
	private float yVel = 0f;
		
	// attacking
	public int attackStrength = 1;
	public bool attackTarget = true;
	public float attackRange = 2f;
	private GameObject player;
	private Vector3 ourPosition;
	private Animator animator;

	// chasing
	public bool chaseTarget = true;
	public float chaseTime = 5f; // sec to chase until give up

	// dieing
	public int points = 1;
	public float scaleDown = 10f;
	public AudioClip deathSound; // sound fx
	public GameObject deathParticleEffectPrefab; // particle explosions
	private GameController gameController;
	public bool damaged = false; // public so damageNPC can check vulnerability status
	private MeshRenderer[] renderers;
	private SkinnedMeshRenderer[] skinnedRenderers;
	private BoxCollider boxcollider; // for shrinking size during death animation

	// Use this for initialization
	void Start () {
		animator = gameObject.GetComponent<Animator> ();
		ourPosition = gameObject.transform.position;
		player = GameObject.FindGameObjectWithTag ("Player");
		timeUntilTurn = secondsTillTurn;

		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent<GameController> ();

		renderers = gameObject.GetComponentsInChildren<MeshRenderer> ();
		skinnedRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer> ();

		boxcollider = gameObject.GetComponent<BoxCollider> ();

	}
	
	// Update is called once per frame
	void Update () {

		if (gameController != null)
			if (!gameController.isPlaying) {
				animator.speed = 0f;
				StopAllCoroutines();
				return;
			} else {
				animator.speed = 1f;
			}
	
		if (attackTarget && !damaged) {

			Vector3 playerPosition = player.transform.position;
			float distance = Vector3.Distance (ourPosition, playerPosition);

			if (distance <= attackRange) {
				animator.SetTrigger("attack");
				Debug.Log("attacking! ");
			}

		}

		if (walkAround && !damaged) {
					
			timeUntilTurn -= Time.deltaTime;
			
			if (timeUntilTurn <= 0 && !isTurning) {
				//			Debug.Log ("Starting Turn");
				isTurning = true;
			}
			
			if (isTurning && anglesTurnedSoFar < turnDegrees) {
				//			Debug.Log ("Continuing Turn");
				transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);		
				anglesTurnedSoFar += rotateSpeed * Time.deltaTime;
			} else if (isTurning && anglesTurnedSoFar >= turnDegrees) {
				//			Debug.Log ("Ending Turn");
				isTurning = false;
				timeUntilTurn = secondsTillTurn;
				anglesTurnedSoFar = 0f;
			}

			// add gravity
//			float yDelta = gravity * Time.deltaTime;
//			yVel -= yDelta;
//			yVel = Mathf.Max (-terminalVelocity, yVel);
			Vector3 newPosition = transform.position;
//			newPosition.y += yVel;

			// move forward when not turning
			if (!isTurning) {
				newPosition += transform.forward * walkSpeed;
			}

			transform.position = Vector3.Lerp (transform.position, newPosition, Time.deltaTime);

		}// end walkaround

	}


	// turn when we run into a wall
	void OnCollisionEnter(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
			//	Debug.Log ("contact: " + contact);
			if (contact.normal == Vector3.forward) {
				Debug.DrawRay(contact.point, contact.normal, Color.white);
				timeUntilTurn = 0f;
				anglesTurnedSoFar = 0f;
			}
		}
	}

	public void TakeDamage() {
	
		// don't take damage again
		if (damaged)
			return;

		damaged = true;

		StartCoroutine ("Die");

//		animator.SetBool ("Damaged", true);
//
//		boxcollider.size = new Vector3(boxcollider.size.x, 0.2f, boxcollider.size.z);
//
//		//StartCoroutine ("Fade");
//		//StartCoroutine ("Shrink");
//
//
//		AudioSource.PlayClipAtPoint(deathSound, transform.position);
//		
//		scoreController.score += points;
//
//		GameObject particleObject = (GameObject) Instantiate(deathParticleEffectPrefab, transform.position, transform.rotation);
//		Destroy(particleObject, 3f); // destroy in 2 sec
//
//		Destroy(gameObject, 1f);

	}

	IEnumerator Die() {
		AudioSource.PlayClipAtPoint(deathSound, transform.position);
		animator.SetBool ("Damaged", true);
		boxcollider.size = new Vector3(boxcollider.size.x, 0.1f, boxcollider.size.z);
		yield return new WaitForSeconds(.75f);

		GameObject particleObject = (GameObject) Instantiate(deathParticleEffectPrefab, transform.position + Vector3.up*2f, transform.rotation);
//		scoreController.score += points;

		Destroy(gameObject);
		Destroy(particleObject, 3.5f);

		yield return null;
	}



//
//	IEnumerator Shrink() {
//		transform.localScale -= new Vector3 (scaleDown, scaleDown, scaleDown) * Time.deltaTime;
//		yield return new WaitForSeconds(.1f);
//	}
//
//
//	IEnumerator Fade() {
//		for (int i = 0; i < renderers.Length; i++) {
//			for (float f = 1f; f >= 0; f -= 0.1f) {
//				MeshRenderer renderer = renderers[i];
//				Color c = renderer.material.color;
//				c.a = f;
//				renderer.material.color = c;
//			}
//		}
//		for (int i = 0; i < skinnedRenderers.Length; i++) {
//			for (float f = 1f; f >= 0; f -= 0.1f) {
//				SkinnedMeshRenderer renderer = skinnedRenderers[i];
//				for (int j = 0; j < renderer.materials.Length; j++) {
//					Color c = renderer.materials[j].color;
//					c.a = f;
//					renderer.materials[j].color = c;
//				}
//			}
//		}
//		yield return new WaitForSeconds(.1f);
//	}

}
