using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour {

	public float WaveSpeed = 2f;
	public float WaveFrequency = 2f;
	public float WaveHeight = 1f;

//	private int numberRandoms = 5;
//	private float[] randomOffsets;
//	private float minRandomOffset = 0.05f;
//	private float maxRandomOffset = 1f;

	private GameObject gameControllerObject;
	private GameController gameController;

	// Use this for initialization
	void Start () {
		gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
			gameController = gameControllerObject.GetComponent<GameController> ();

//		randomOffsets = new float[numberRandoms];
//		for (int i = 0; i < numberRandoms; i++) {
//			randomOffsets[i] = Random.Range(minRandomOffset, maxRandomOffset);
//		}
	}
	
	// Update is called once per frame
	void Update () {

		if (gameController != null)
			if (gameController.isPaused || gameController.isGameOver)
				return;

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		while (i < vertices.Length) {
			float phase = Time.time * WaveSpeed;
			Vector3 wpos = vertices[i];
			float offset = (wpos.x + (wpos.z * 0.2f)) * WaveFrequency;
			wpos.y = wpos.y + (Mathf.Sin(phase + offset) * WaveHeight);
//			wpos.x = wpos.x + random_offset;
//			wpos.z = wpos.z + random_offset;
			vertices[i] = wpos;
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
}
