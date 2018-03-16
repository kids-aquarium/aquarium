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
			InstantFishWithTexture("fishTexTest001");
		}

		if(Input.GetKeyUp("2")){
			InstantFishWithTexture("fishTexTest002");
		}

		if(Input.GetKeyUp("3")){
			InstantFishWithTexture("fishTexTest003");
		}

		if(Input.GetKeyUp("4")){
			InstantFishWithTexture("fishTexTest004");
		}
		
	}

	void InstantFishWithTexture(string _textureName){
		GameObject newFish = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		newFish.GetComponent<setMaterial>().LoadTexture(_textureName);
		newFish.transform.parent = this.transform;
	}
}
