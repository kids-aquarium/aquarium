using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantFish : MonoBehaviour {
	public GameObject prefab;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyUp("1")){
			GameObject newFish = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			MaterialManager mat = newFish.GetComponent<MaterialManager>();
			mat.textureName = "fishTexTest001";
			newFish.transform.parent = this.transform;
		}

		if(Input.GetKeyUp("2")){
			GameObject newFish = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			MaterialManager mat = newFish.GetComponent<MaterialManager>();
			mat.textureName = "fishTexTest002";
			newFish.transform.parent = this.transform;
		}

		if(Input.GetKeyUp("3")){
			GameObject newFish = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			MaterialManager mat = newFish.GetComponent<MaterialManager>();
			mat.textureName = "fishTexTest003";
			newFish.transform.parent = this.transform;
		}
		
	}
}
