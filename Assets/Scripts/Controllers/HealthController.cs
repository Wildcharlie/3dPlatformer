using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {
	
	public int startingLives = 3;
	public int remainingLives;
	public float startingHitPoints = 3f;
	public float remainingHitPoints;

	public GameObject damageParticlePrefab;
	public AudioClip damageSound;
	public AudioClip deathSound;
	
	public bool takingDamage; // for testing whether player should have control or not
	public float invulnerableTime = 1.05f; // seconds after damage player should be invulnerable and flashing
	public float blinkTime = .075f;
	private Component[] characterMeshes;
	private SkinnedMeshRenderer characterSkin;

	public bool isDead = false;

	private GameObject levelRoot; // make all instantiated objects children of this
	private GameController gameController;
	private GameObject particleObject;
//	private ShakeObject cameraShake;
	private Animator animator; // player's animator, to set damaged animation state
	private CameraMovement camMove;

	void Start () {	
		characterMeshes = gameObject.GetComponentsInChildren<MeshRenderer> ();
		characterSkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer> ();
		remainingHitPoints = startingHitPoints;
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent<GameController> ();
		animator = gameObject.GetComponent<Animator> ();
		takingDamage = false;
		isDead = false;

		Reset ();

	}


	public void Reset() {	
		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		if (levelRoot == null) // if the level hasn't loaded yet do nothing
			return;

		GameObject cam = GameObject.FindGameObjectWithTag ("MainCamera");
		camMove = cam.GetComponent<CameraMovement> ();
//		cameraShake = cam.GetComponent<ShakeObject> ();
		remainingHitPoints = startingHitPoints;
		remainingLives = startingLives;
	}



	void Update() {

		// game has been restarted and we need a new reference to level root
		if (levelRoot == null) 
				Reset ();

		// testing no gc
		if (gameController == null)
			return;

		// on pause or game over, stop everything
		if (!gameController.isPlaying) {
			StopAllCoroutines();

			// in case the blink coroutine didn't finish, turn all meshes back to visible
			foreach (MeshRenderer characterRenderer in characterMeshes) {
				characterRenderer.enabled = true;
			}
			characterSkin.enabled = true;					
			takingDamage = false;

			// turn off the particles
			if (particleObject) {
				ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
				particleSystem.enableEmission = false;
			}
		}

	}


	public void ApplyDamage(float points) {	

		if (gameController != null)
			if (!gameController.isPlaying || takingDamage)
				return;

		takingDamage = true;
		Debug.Log ("Applying damage, takingDamage: " + takingDamage);

		// DIE
		StartCoroutine(DoDamage (points));

	}

	public void Die() {
		if (!isDead) {
			isDead = true;
			animator.SetTrigger ("Dead");
			AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
			particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
			if (particleObject != null) {
				if (levelRoot == null)
					Reset();
				particleObject.transform.parent = levelRoot.transform;
				Destroy (particleObject, 3f); // destroy the particles in X sec
			}
			if (gameController != null) {
				if ( remainingLives > 0)
					gameController.EndLife ();
				else 
					gameController.EndGame();
			}
		}
	}


	public void FallDeath() {
		if (!isDead) {
			isDead = true;
			camMove.isPaused = true;
			animator.SetTrigger ("Dead");
			if (gameController != null) {
				if ( remainingLives > 0)
					gameController.EndLife ();
				else 
					gameController.EndGame();
			}
		}
	}


	IEnumerator DoDamage(float points) {

		if (remainingHitPoints == 0) {
			Die ();
		// OR TAKE DAMAGE
		} else {
			// cameraShake.SendMessage("Shake", .05f);		
			animator.SetTrigger ("Damaged");
			remainingHitPoints -= points;
			AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
			particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
			if (particleObject != null) {
				particleObject.transform.parent = levelRoot.transform;
				Destroy (particleObject, 3f); // destroy the particles in X sec
			}

			// blink
			float duration = invulnerableTime;
			while (duration > 0f) {
				duration -= Time.deltaTime;
				
				//toggle renderer
				foreach (MeshRenderer characterRenderer in characterMeshes) {
					characterRenderer.enabled = !characterRenderer.enabled;
				}
				characterSkin.enabled = !characterSkin.enabled;
				
				//wait for a bit
				yield return new WaitForSeconds(blinkTime);
			}
			
			//make sure renderer is enabled when we exit
			foreach (MeshRenderer characterRenderer in characterMeshes) {
				characterRenderer.enabled = true;
			}
			characterSkin.enabled = true;

			takingDamage = false;
		}
		
	}


}
