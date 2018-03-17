using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantFish : MonoBehaviour {

	public GameObject prefab;
	public float instantiationRadius = 10.0f;

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

	public void InstantFishWithTexture(string _textureName){
		Vector3 positionOffset = Random.insideUnitSphere * instantiationRadius;
		Quaternion rotationOffset = Quaternion.identity;
		GameObject newFish = Instantiate(prefab, transform.position + positionOffset, transform.rotation * rotationOffset);
		newFish.GetComponent<setMaterial>().LoadTexture(_textureName);
		newFish.transform.parent = this.transform;
	}
}
