using UnityEngine;
using System.Collections;

public class ShakeObject : MonoBehaviour {
	
	public float shake = 0f;
	public float shakeAmount = 0.7f;
	public float decreaseFactor = .8f;
	
	// Update is called once per frame
	void Update () {

		if (shake > 0) {
			gameObject.transform.localPosition = gameObject.transform.localPosition + Random.insideUnitSphere * shakeAmount;
			shake -= Time.deltaTime * decreaseFactor;
		} else {
			shake = 0f;
		}

	}

	public void Shake( float shakeAmountI) {
		shakeAmount = shakeAmountI;
		shake = .005f;
	}

}
