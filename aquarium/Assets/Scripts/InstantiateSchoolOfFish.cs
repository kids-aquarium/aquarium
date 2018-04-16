using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateSchoolOfFish : MonoBehaviour {
	public GameObject prefab;
	public int numberOfFish = 20;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < numberOfFish; ++i) {
			Vector3 pos = new Vector3(0, 0.5f, i);
			Instantiate(prefab, pos, Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
