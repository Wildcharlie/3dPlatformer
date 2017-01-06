using UnityEngine;
using System.Collections;

public class JetpackJump : MonoBehaviour {

	public GameObject particleSystemPrefab;
	private ParticleSystem particles;
	private CharacterMotor motor;
	private GameObject jetpack;

	// Use this for initialization
	void Start () {
	
		motor = gameObject.GetComponent<CharacterMotor> ();

		// create particle effect and add to jetpack
		jetpack = GameObject.FindGameObjectWithTag ("Jetpack");
		GameObject jetParticlesObject = Instantiate (particleSystemPrefab) as GameObject;
		jetParticlesObject.transform.parent = jetpack.transform;
		jetParticlesObject.transform.localPosition = new Vector3 (-.771f, .706f, .38f); // as discovered in editor
		jetParticlesObject.transform.localEulerAngles = new Vector3 (72f, 258f, 256f);
		particles = jetParticlesObject.GetComponent<ParticleSystem> ();
		particles.enableEmission = false;

	}
	
	// Update is called once per frame
	void Update () {
		if (particles == null) 
			return;

		if (motor.canUseJetpack && motor.verticalVelocity > 0) {
			particles.enableEmission = true;
		} else {
			particles.enableEmission = false;
		}

	}
}
