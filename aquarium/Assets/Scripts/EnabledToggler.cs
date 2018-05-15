using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledToggler : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("t")) {
			foreach(Transform child in transform) {
				child.gameObject.SetActive(!child.gameObject.active);
			}
		}
	}
}
