/* NOTES:
 * 
 * Terrain must be tagged "Terrain" in order to slide down it
 * Falling Platforms must be tagged "FallingPlatform" to be triggered by the player
 * Moving Platforms must be tagged "Platform" to affect the player's position
 * Enemies must be tagged "Enemy" to hurt player and be killed
 * Natural hazards (water, spikes) must be tagged "Hazard"
 * 
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CharacterMotor : MonoBehaviour {
	
	#region Parameters

	// basic physics controls
	private float gravity = 40f; // makes character fall
	private float terminalVelocity = 20f; // fastest possible fall/slide speed
	private float groundTolerance = .05f; // distance controller can be from ground and still be considered "grounded"

	private float runSpeed = 8f; // max speed while running
	private float currentSpeed = 0f;
	private float previousSpeed = 0f; // we base our new speed off the previous speed
	public Vector3 moveDirection = Vector3.zero; // global var used to track intended movement dir
	private Vector2 playerInput = Vector2.zero; // horiz and vert joystick input
	public float obstacleDistanceTolerance = .2f;

	// accel
	private float deceleration = 25f; // used to slow walk/run when no user input

	// jumping
	public bool useJump = true;
	public float jumpVelocity = 10f; // strength of upward jump force
	public bool useVariableHeightJump = true;
	public float cutJumpSpeed = 4f; // jump velocity to use when jump is cut short
	public float endFallAnimationTime = 1.0f; // length in sec of landing animation
	public float inAirControl = .5f; // reduces amount of horizontal control in air
	public float verticalVelocity { get; private set; } // jump/fall velocity	
	private float distToGround = 0f; // used instead of isGrounded
	private float distToCeiling = 1000f; // dist to obstacle above; set to 1000 if no obstacle 
	private float distToForward = 0f;
	private bool triggerJump = false;

	// double jumping
	public bool useDoubleJump = true;
	public float doubleJumpVelocity = 16f;
	public float timeDoubleJumpAvailable = .5f; // seconds after landing 1st jump in which we can double-jump
	private float timeDoubleJumpRemaining = 0f;
	private bool isDoubleJumping = false;

	// slide control
	public bool useSlide = true;
	private float slideSpeed = 0f; // slide velocity
	public float slideFriction = 2f; // slow the slide down by friction amount
	private bool isSliding = false;
	private Vector3 slideDirection = Vector3.zero; // direction to slide in
		
	// particles and sounds fx
	public GameObject dustParticlePrefab; // dust particle prefab for sudden start/stop
	public AudioClip footstepSound; 
	public AudioClip jumpSound;
	public AudioClip slideSound;
	public AudioClip landingSound;
	private AudioSource soundEffectsSource;

	// various gameobject references	
	private CharacterController controller; // used to move the player
	private Animator animator;
	private GameObject levelRoot;
	private GameController gameController;
	private HealthController healthController;

	// enable powerups!
	public bool canUseJetpack = true;

	// allow force to be applied from outside, like enemy hit
	private float forceAmount = 0f;
	private Vector3 forceDirection = Vector3.zero;

	// detect when we're on top of moving platforms and offset our motion by theirs
	private GameObject objectBelowPlayer;
	private bool isMovingWithPlatform;
	private Vector3 platformMoveDirection = Vector3.zero;
	private PlatformMove platformMove;

	// pole grabbing
	private bool isOnPole = false;
	private GameObject activePole = null;

	public bool fallingDeath;
	private bool isAgainstWall;
	private Vector3 WallDirection;
	private Vector3 airMomentum;

	private float slope;
	private bool isAirBorne = false;

	#endregion


	void Start () {		

		// cache object references
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent<GameController> ();
		healthController = gameObject.GetComponent<HealthController> ();

		// create new sound source for sfx
		soundEffectsSource = gameObject.AddComponent<AudioSource>();
		soundEffectsSource.loop = false;
		soundEffectsSource.Stop ();

		fallingDeath = false;
		isMovingWithPlatform = false;

	}


	void Update ()
	{	

		if (gameController != null) {
			if (!gameController.isPlaying) {
				// we are paused or a dialog is displaying
				animator.speed = 0f;
				StopAllCoroutines ();
				return;
			} else if (gameController.isLevelEnd || fallingDeath) {
				return;
			}
		} 

		animator.speed = 1f;

		// get input from player if not being moved elsewhere
		playerInput = ProcessPlayerInput ();

		// apply outside forces
		if (forceAmount > 0f) {
			forceAmount -= gravity * Time.deltaTime;
			forceAmount = Mathf.Clamp (forceAmount, 0f, terminalVelocity);
		}


		// if damage is being taken, don't let player control movement
		if (healthController.takingDamage && (animator.GetCurrentAnimatorStateInfo (0).IsName ("TakeDamage") || animator.GetCurrentAnimatorStateInfo (0).IsName ("StandUp"))) {
			moveDirection = forceDirection * forceAmount;
		} else if (isOnPole && activePole != null) {
			PoleStuff();
		} else {
			// figure current speed based on accel, past speed and player input
			currentSpeed = CalculatePlayerSpeed (playerInput, currentSpeed);
			
			// convert player input to worldspace direction, rotate player 
			if (!isSliding) moveDirection = LocalMovement(playerInput.x, playerInput.y, currentSpeed);
			
			// listen for jump input and handle jump animations, sfx
			if (useJump)
				ProcessJump ();
			
			// dust clouds during sudden accel
			AddDustClouds ();
			
			// slide on slopes
			if (useSlide)
				ProcessSlide ();
			
			// add gravity if not on ground
			if (!isNearlyGrounded()) {
				verticalVelocity -= gravity * Time.deltaTime;
				verticalVelocity = Mathf.Max(-terminalVelocity, verticalVelocity);
			} 
			
			// apply gravity & jump force if not sliding
			// if we slide then fall off the surface
			if (!isSliding || !isNearlyGrounded() ) {
				moveDirection.y = verticalVelocity;
			}

		} // end taking damage

		// in all cases position may be affected by moving platform
		if (objectBelowPlayer != null && !isMovingWithPlatform) {
			StartCoroutine(MoveWithPlatform());
		}
		// move
		if (moveDirection != Vector3.zero)
			controller.Move(moveDirection * Time.deltaTime);

		// if we're on a pole, we need to look toward its center
		if (isOnPole) {
			Vector3 lookDir = new Vector3(activePole.transform.position.x, transform.position.y, activePole.transform.position.z);
			transform.LookAt(lookDir);
		}
		// update the animator's values
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);

	}

	IEnumerator MoveWithPlatform() {

		isMovingWithPlatform = true;

		platformMove = objectBelowPlayer.GetComponent<PlatformMove>();

		while (objectBelowPlayer != null) {

			float moveDistance = .1f;
			float distanceMoved = 0f;
			while (distanceMoved < moveDistance) {
				distanceMoved += Time.deltaTime * platformMove.moveSpeed;
				Vector3 newPosition = transform.position + (platformMove.currentDirection * Time.deltaTime * platformMove.moveSpeed);
				transform.position = newPosition;
				yield return null;
			}
		}

		isMovingWithPlatform = false;

		yield return null;

	}
	
	void FixedUpdate() {
		// update distance to ground
		distToGround = ProbeDirection (Vector3.down);

		// update distance to obstacle above
		distToCeiling = ProbeDirection (Vector3.up);

		distToForward = ProbeDirection(transform.forward);
	}


	#region Movement
	
	/* 
	 * MOVEMENT & SPEED
	 */ 

	 public float getCurrentSpeed ()
	{
		return currentSpeed;
	}

	public float getDistForward ()
	{
		return distToForward;
	}

	private Vector2 ProcessPlayerInput() {		
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");

		if (Mathf.Abs (moveVertical) <= .16) {
			moveVertical = 0;
		}

		if (Mathf.Abs (moveHorizontal) <= .16) {
			moveHorizontal = 0;
		}

		return new Vector2(moveHorizontal, moveVertical);	
	}

	private float CalculatePlayerSpeed (Vector2 playerInput, float prevSpeed)
	{		

		previousSpeed = prevSpeed; // store as member var
		float speed = 0f;

		if (playerInput.y != 0 || playerInput.x != 0) {
			speed = ((Mathf.Abs (playerInput.y) + Mathf.Abs (playerInput.x))) * runSpeed;
		} else {
			speed = prevSpeed - (deceleration * Time.deltaTime);
		}

		speed = Mathf.Clamp (speed, 0f, runSpeed); // don't exceed running speed
		
		return (speed);
		
	}

	private Vector3 LocalMovement (float moveHorizontal, float moveVertical, float speed)
	{
		if (moveVertical == 0f && moveHorizontal == 0f) {
			// don't move
			moveDirection = Vector3.zero; 
		
		} else if (moveHorizontal != 0f || moveVertical != 0f) {
			if (!isNearlyGrounded ()) {

				Vector3 targetDirection = moveVertical * Camera.main.transform.forward + moveHorizontal * Camera.main.transform.right;
				targetDirection.y = 0f;
				moveDirection = airMomentum + targetDirection * speed * .5f;
				airMomentum = Vector3.ClampMagnitude (airMomentum + targetDirection * .1f, runSpeed);

				// Fix bug where character cant jump against walls
				float dist = ProbeDirection (targetDirection);
				if (dist > .15 && isAgainstWall) {
					isAgainstWall = false;
				}

				if (isAgainstWall) {
					moveDirection = Vector3.zero;
				}

			} else {
				Vector3 tempTarget = moveVertical * Camera.main.transform.forward + moveHorizontal * Camera.main.transform.right;
				tempTarget.y = 0f;

					transform.rotation = Quaternion.LookRotation (tempTarget);
					moveDirection = transform.TransformDirection (Vector3.forward * speed);

			}
		}
		return(moveDirection);
	}

	private void UpdateAnimations(float moveHorizontal, float moveVertical, float speed) {
		// update animation FSM with speed and turn info
		if (verticalVelocity <= 0f && isNearlyGrounded() ) { // not jumping or falling
			animator.SetFloat ("speed", speed / runSpeed); // normalize to 0-1
			animator.SetBool("JumpLoop", false); // in case we're stuck in animator loop
			animator.SetBool("JumpStart", false);
		}

		animator.SetFloat ("angularVelocity", moveHorizontal);

		// pole positions
		if (isOnPole && (moveHorizontal == 0f && moveVertical == 0f)) {
			animator.SetBool("Hanging", true);
			animator.SetBool("Climbing", false);
		} else if (isOnPole && (moveHorizontal != 0f || moveVertical != 0f)) {
			animator.SetBool("Hanging", true);
			animator.SetBool("Climbing", true);
		} else {
			animator.SetBool("Hanging", false);
			animator.SetBool("Climbing", false);
		}

	}

	#endregion
	#region Jumping


	/*
	 * JUMPING
	 */ 
	
	
	private void ProcessJump() {	
		// made it 1/2way through jump, stopped pressing button
		// stop jumping
		if (useVariableHeightJump) {
			if (verticalVelocity > 0f && verticalVelocity <= jumpVelocity * .75 && !Input.GetButton ("Jump")) {
				verticalVelocity = cutJumpSpeed;
			}
		}

		// jumping up
		if (verticalVelocity > 0f) {		
			// stop jumping up if we hit a ceiling
			if (distToCeiling <= .01f) {
				JumpLoop();
			}

		// falling down
		} else if (verticalVelocity < 0f) {

			// enter jump loop animation until we get close to the ground
			if( animator.GetCurrentAnimatorStateInfo(0).IsName("JumpStart") ) {
				JumpLoop();
			}

			// estimate time to reach ground based on current yVel
			float timeToReachGround = Mathf.Abs(distToGround / verticalVelocity); // meters / meters per sec
			// if we've hit the ground and are still in jump animation, end the jump animation
			if( animator.GetCurrentAnimatorStateInfo(0).IsName("JumpLoop") && 
			    (timeToReachGround <= endFallAnimationTime || isNearlyGrounded() ) )
				EndFall();
		}

		// ending fall, reset animations
		if (isNearlyGrounded() && !animator.GetCurrentAnimatorStateInfo(0).IsName ("Locomotion") && isAirBorne) {
			Land();
		}

		// double jump timer
		if (useDoubleJump && timeDoubleJumpRemaining > 0f) 
			timeDoubleJumpRemaining -= Time.deltaTime;

		// on ground
		if (useDoubleJump && isNearlyGrounded() && Input.GetButtonDown ("Jump") && timeDoubleJumpRemaining > 0f)
			StartDoubleJump ();
		else if ((isNearlyGrounded() && Input.GetButtonDown ("Jump")) || triggerJump )
			StartJump ();
	
	}

	public void StartDoubleJump() {
		timeDoubleJumpRemaining = 0f;
		// player pushed jump button; also used as message to react to jumping on npcs
		isDoubleJumping = true;
		verticalVelocity = doubleJumpVelocity;
		Jump();
	}
	
	public void StartJump() {
		// player pushed jump button; also used as message to react to jumping on npcs
		verticalVelocity = jumpVelocity;
		Jump();
	}

	public void Jump() {
		animator.SetBool("JumpStart", true);
		soundEffectsSource.Stop(); // no footsteps sfx in air		
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
		airMomentum =  currentSpeed * transform.forward;
		isAirBorne = true;

		// add platform velocity to ours
		if (isMovingWithPlatform) {
			Vector3 platformDir = platformMove.currentDirection * Time.deltaTime * platformMove.moveSpeed;
			if (platformDir.y > 0)
				verticalVelocity += platformMove.currentDirection.y * platformMove.moveSpeed;	
		}
	}
	
	public void JumpLoop() {
		// player walked off ledge	
		verticalVelocity = -.1f; // reset vspeed, gravity will cause fall; don't use 0 so we won't divide by 0 later
		animator.SetBool("JumpLoop", true);
		soundEffectsSource.Stop(); // no footsteps sfx in air		
	}
	
	public void EndFall() {
		// end jump or fall; same animation	
		if (animator.GetBool ("JumpStart") || animator.GetBool ("JumpLoop")) {
			animator.SetBool ("JumpEnd", true);
			StartCoroutine ("PlayDelayedAudio");
		}

	}

	public void Land() {
		EndFall();
		animator.SetBool ("JumpStart", false);
		animator.SetBool ("JumpLoop", false);
		animator.SetBool ("JumpEnd", false);
		animator.SetBool ("FallStart", false);
		isAirBorne = false;

		// reset double jump timer if this was a regular jump
		if (useDoubleJump) {
			if (isDoubleJumping) {
				isDoubleJumping = false;
				timeDoubleJumpRemaining = 0f;
			} else {
				timeDoubleJumpRemaining = timeDoubleJumpAvailable;
			}
		}

	}


	private void ProcessSlide() {	
		// slideDirection and isSliding are detected during OnControllerColliderHit
		// ProcessSlide runs every update to move player in slideDirection when sliding
		if (slideDirection != Vector3.zero && isSliding) {
			moveDirection += slideDirection * slideSpeed;
			if (soundEffectsSource.clip != slideSound || !soundEffectsSource.isPlaying) {
				soundEffectsSource.Stop ();
				soundEffectsSource.clip = slideSound;
				soundEffectsSource.loop = true;
				soundEffectsSource.Play ();	
			}
		}

		// if we're no longer on the ground, or if the surface below us is not sloped
		// we are no longer sliding
		if (!isNearlyGrounded() || !isSliding) {
			isSliding = false;
			if (soundEffectsSource.clip == slideSound && soundEffectsSource.isPlaying) {
				soundEffectsSource.Stop ();
				soundEffectsSource.loop = false;
			}
		}

		
	}


	#endregion
	#region Pole


	void PoleStuff() {
		// if we're on a pole, we have to be correct distance from it
		// also keep vertical position between pole bottom and pole top (minus flag's height)
		CapsuleCollider poleCollider = activePole.GetComponent<CapsuleCollider>();
		Vector3 poleCenter = activePole.transform.position + poleCollider.center;		
		Vector3 poleTopPosition = poleCenter + (Vector3.up * poleCollider.height/2f) + (-Vector3.up * 1f);
		Vector3 playerHeadPosition = transform.position + controller.center + (Vector3.up * (controller.height/2f));
		playerHeadPosition += -(Vector3.up * (controller.height/2f)); // don't overlap flag
		Vector3 playerFeetPosition = playerHeadPosition + (-Vector3.up * (controller.height));
		Vector3 playerCenter = transform.position + controller.center;
		Vector3 poleCenterHrz = new Vector3(poleCenter.x, playerCenter.y, poleCenter.z);

		float dist = Vector3.Distance(poleCenterHrz, playerCenter);
		float correctDist = controller.radius + poleCollider.radius;
		float diff = correctDist - dist;
		float diffAllowed = 0.01f;
		if (diff > diffAllowed) {
			Vector3 relDir = playerCenter - poleCenterHrz; 
			transform.position = Vector3.Lerp(transform.position, transform.position + relDir.normalized * diff, Time.deltaTime);
		}
		if (playerHeadPosition.y >= poleTopPosition.y) {
			float vertDiff = playerHeadPosition.y - poleTopPosition.y;
			Vector3 newpos = new Vector3(transform.position.x, transform.position.y - vertDiff, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, newpos, Time.deltaTime);
		} else if (playerFeetPosition.y <= activePole.transform.position.y) {
			float vertDiff = activePole.transform.position.y - playerFeetPosition.y;
			Vector3 newpos = new Vector3(transform.position.x, transform.position.y + vertDiff, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, newpos, Time.deltaTime);
		}

		// apply player input to pole position
		moveDirection = Vector3.zero;
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		
		// left/right = rotation around pole
		if (moveHorizontal != 0f) {			
			float joystickRotationForce = 120f;
			float yRot = Mathf.Clamp(moveHorizontal * joystickRotationForce, -179f, 179f);
			Vector3 newRot = transform.rotation * new Vector3(0f, yRot, 0f);
			Vector3 nearPoleCenter = new Vector3(poleCenter.x, transform.position.y, poleCenter.z);
			Vector3 newPos = RotatePointAroundPivot(transform.position, nearPoleCenter, newRot);
			Vector3 relPosition = newPos - transform.position;
			moveDirection = relPosition;				
		}
		
		if (moveVertical != 0f) {
			if (playerHeadPosition.y >= poleTopPosition.y) {
				moveVertical = Mathf.Min (0f, moveVertical);
			} else if (playerFeetPosition.y <= activePole.transform.position.y) {
				if (moveVertical < 0) {
					StartJump();
					isOnPole = false;
					activePole = null;
				}
				moveVertical = Mathf.Max (0f, moveVertical);					
			}
			
			moveDirection = moveDirection + new Vector3(0f, moveVertical * runSpeed / 4f, 0f);
			
			
		}
		
		if (Input.GetButtonDown ("Jump")) {
			StartJump();
			isOnPole = false;
			activePole = null;
		}
	}


	#endregion
	#region Physics
	
	/* 
	 * Physics, pre-emptive collision detection and Collisions
	 */

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Platform") {
			objectBelowPlayer = other.gameObject; // remember what's below us
		} else if (other.gameObject.tag == "Pole") {
			// grab the pole
			activePole = other.gameObject;
			isOnPole = true;

		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "Platform") {
			objectBelowPlayer = null; // nothing
		} 
	}



	void OnControllerColliderHit(ControllerColliderHit hit) {

		// detect collisions with sloped terrain
		
		slope = Mathf.Acos(hit.normal.y) * Mathf.Rad2Deg; // slope of hit surface
		
		// object must be tagged Terrain, we shouldn't slide on npcs or anything not terrain
		// also we shouldn't slide down near vertical surfaces; gravity will handle that
		if (slope > controller.slopeLimit && hit.gameObject.tag != "Enemy" && (slope <= 80)) {
			if (!isSliding) {
				isSliding = true;
				isAgainstWall = false;
				slideSpeed = 0f;
				verticalVelocity = -.1f; // need to reset this so jump can detect we're on ground
				//StartFall(); // animations
			}
			slideSpeed += slope/gravity/slideFriction * Time.deltaTime;
			slideSpeed = Mathf.Min(terminalVelocity, slideSpeed);
			Vector3 nonUnitAnswer = Vector3.down - Vector3.Dot (Vector3.down, hit.normal) * hit.normal; // could be 0
			slideDirection = Vector3.Normalize (nonUnitAnswer);
		} else if (slope >= 80f) {
			isAgainstWall = true;
			isSliding = false;
			WallDirection = hit.moveDirection;
		} else {
			// not on slope any longer
			isSliding = false;
			isAgainstWall = false;
		}

		
	}

	public bool isNearlyGrounded() {
		// can't trust character controller's isGrounded, so cast for nearest object below 
		// and use this distance instead to determine whether we're grounded		
		if (isMovingWithPlatform && distToGround * 3f <= groundTolerance) // need to be more forgiving when on platforms
			return true;		
		else if (distToGround <= groundTolerance)
			return true;		
		return false;
	}


	private float ProbeDirection( Vector3 direction) {
		Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.2F;
		Vector3 p2 = p1 + Vector3.up * controller.height * 0.2f;
		RaycastHit hit;
		float dist = 100f;
		if (Physics.CapsuleCast(p1, p2, controller.radius, direction, out hit, 10)) {
			if (hit.transform.tag != "Player") {
				dist = hit.distance;
			}
		}

		return (dist);
		
	}

	
	#endregion
	#region FX
	
	/* 
	 * VFX & SFX
	 */
	
	private IEnumerator PlayDelayedAudio() {
		yield return new WaitForSeconds(.3f); // approx time until landing anim appears to hit ground
		AudioSource.PlayClipAtPoint(landingSound, transform.position);
	}
	
	void PlayFootstep() {
		// play a single footstep sound; called by animation events
		soundEffectsSource.Stop ();
		soundEffectsSource.clip = footstepSound;
		soundEffectsSource.loop = false;
		soundEffectsSource.Play();
	}
	
	
	void AddDustClouds() {		
		// add dust particles if sudden acceleration
		if (Mathf.Abs(currentSpeed - previousSpeed) >= runSpeed / 2 && controller.isGrounded) {
			GameObject particleObject = (GameObject) Instantiate(dustParticlePrefab, transform.position, transform.rotation);
			if (!levelRoot)
				levelRoot = GameObject.FindGameObjectWithTag ("Level");
			particleObject.transform.parent = levelRoot.transform;
			Destroy(particleObject, 2f); // destroy in 2 sec
		}		
	}


	public void ApplyForce(Vector3 direction, float amount) {
		// used to push player away from enemy after hit; accessed via other scripts
		forceAmount = amount;
		forceDirection = direction;			
	}

	#endregion
	
	
	Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}    


}



	
