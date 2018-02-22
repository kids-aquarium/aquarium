using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSwim : MonoBehaviour {

	private GameObject cog;
	public float speed = 1f;
	public float depth = 1f;
	// Use this for initialization
	void Start () {
		cog = transform.GetChild (0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		cog.transform.localPosition = new Vector3(Mathf.Sin(Time.time * speed) * depth, 0, 0.5f);
	}
}
