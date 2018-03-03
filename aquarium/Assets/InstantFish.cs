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
			InstanFishWithTexture("fishTexTest001");
		}

		if(Input.GetKeyUp("2")){
			InstanFishWithTexture("fishTextTest002");
		}

		if(Input.GetKeyUp("3")){
			InstanFishWithTexture("fishTexTest003");
		}
		
	}

	void InstanFishWithTexture(string _textureName){
		GameObject newFish = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			MaterialManager mat = newFish.GetComponent<MaterialManager>();
			mat.textureName = _textureName;
			newFish.transform.parent = this.transform;
	}

}
