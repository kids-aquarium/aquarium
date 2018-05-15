using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSwim : MonoBehaviour {

	private GameObject cog;
	private float originalCogZ = 0.5f;
	private float phase;
	public float speed = 1f;
	public float depth = 1f;
	// Use this for initialization
	void Start () {
		phase = Random.Range(0.0f, 2 * Mathf.PI);
		cog = transform.GetChild (0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		cog.transform.localPosition = new Vector3(Mathf.Sin((Time.time + phase) * speed) * depth, 0, originalCogZ);
	}
}
