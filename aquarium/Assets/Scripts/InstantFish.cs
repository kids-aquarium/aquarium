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
		// Quaternion rotationOffset = Quaternion.AngleAxis (90.0f, Vector3.up);
		Quaternion rotationOffset = Quaternion.AngleAxis (Random.value * 360.0f, Random.insideUnitSphere);
		GameObject newFish = Instantiate(prefab, transform.position + positionOffset, transform.rotation * rotationOffset);
		newFish.GetComponent<setMaterial>().LoadTexture(_textureName);
		newFish.transform.parent = this.transform;
	}

	public void InstantFishWithTexture2D(Texture2D tex){
		Vector3 positionOffset = Random.insideUnitSphere * instantiationRadius;
		// Quaternion rotationOffset = Quaternion.AngleAxis (90.0f, Vector3.up);
		Quaternion rotationOffset = Quaternion.AngleAxis (Random.value * 360.0f, Random.insideUnitSphere);
		GameObject newFish = Instantiate(prefab, transform.position + positionOffset, transform.rotation * rotationOffset);
		newFish.GetComponent<setMaterial>().LoadTexture2D(tex);
		newFish.transform.parent = this.transform;
	}

	public void InstantFishFromFile(string filepath){
		Vector3 positionOffset = Random.insideUnitSphere * instantiationRadius;
		// Quaternion rotationOffset = Quaternion.AngleAxis (90.0f, Vector3.up);
		Quaternion rotationOffset = Quaternion.AngleAxis (Random.value * 360.0f, Random.insideUnitSphere);
		GameObject newFish = Instantiate(prefab, transform.position + positionOffset, transform.rotation * rotationOffset);
		newFish.GetComponent<setMaterial>().LoadTextureF(filepath);
		newFish.transform.parent = this.transform;
	}
}
