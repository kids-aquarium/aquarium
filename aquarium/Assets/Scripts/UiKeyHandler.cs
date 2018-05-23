using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiKeyHandler : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			foreach(Transform t in transform) t.gameObject.SetActive(!t.gameObject.active);
		}
	}
}
