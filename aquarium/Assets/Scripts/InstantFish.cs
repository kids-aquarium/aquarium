using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InstantFish : MonoBehaviour {

	public GameObject prefab;
	public GameObject[] fishPrefabs = new GameObject[10];
	public float instantiationRadius = 10.0f;

	// Use this for initialization
	void Start ()
    {
        Cursor.visible = true; // on startup, menu is open, so keep cursor visible
	}

	// Update is called once per frame
		void Update () {

		if(Input.GetKeyUp(KeyCode.Space)){
			InstantFishWithDefaultTexture(Random.Range(0, 10));
		}

        if(Input.GetKeyUp(KeyCode.K)) {
            GetComponent<FishFlocker>().KillNewest();
        }

	}

	public void InstantFishWithDefaultTexture(int fishID){
		Vector3 positionOffset = Random.insideUnitSphere * instantiationRadius;
		// Quaternion rotationOffset = Quaternion.AngleAxis (90.0f, Vector3.up);
		Quaternion rotationOffset = Quaternion.AngleAxis (Random.value * 360.0f, Vector3.up);
		GameObject newFish = Instantiate(fishPrefabs[fishID], transform.position + positionOffset, transform.rotation * rotationOffset);
		// newFish.GetComponent<setMaterial>().LoadTexture(_textureName);
		newFish.GetComponent<FlockingFish>().SetBreed(fishID);
		newFish.transform.parent = this.transform;
	}

	public void InstantFishWithTexture2D(int fishID, Texture2D tex){
		Vector3 positionOffset = Random.insideUnitSphere * instantiationRadius;
		// Quaternion rotationOffset = Quaternion.AngleAxis (90.0f, Vector3.up);
		Quaternion rotationOffset = Quaternion.AngleAxis (Random.value * 360.0f, Vector3.up);
		GameObject newFish = Instantiate(fishPrefabs[fishID], transform.position + positionOffset, transform.rotation * rotationOffset);
		newFish.GetComponent<setMaterial>().LoadTexture2D(tex);
		newFish.GetComponent<FlockingFish>().SetBreed(fishID);
		newFish.transform.parent = this.transform;
	}

}
