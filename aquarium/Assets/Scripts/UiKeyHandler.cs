using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiKeyHandler : MonoBehaviour {

	public GameObject fishManager;

	void Start () {
		
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			foreach(Transform t in transform) t.gameObject.SetActive(!t.gameObject.active);
			if(fishManager != null) {
				fishManager.GetComponent<FishFlocker>().SavePreferences();
			}
		}
	}
}
