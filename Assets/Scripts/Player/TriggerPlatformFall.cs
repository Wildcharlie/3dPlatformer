using UnityEngine;
using System.Collections;

public class TriggerPlatformFall : MonoBehaviour {
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		// player must be above FallingPlatform for it to trigger
		if (hit.gameObject.tag == "FallingPlatform" && transform.position.y > hit.gameObject.transform.position.y) {
			PlatformController platform = hit.gameObject.GetComponent<PlatformController>();
			platform.SendMessage("Trigger");
		}		
	}

}
