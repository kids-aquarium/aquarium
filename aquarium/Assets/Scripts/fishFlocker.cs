using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlocker : MonoBehaviour {

	//Setting the seekPosition in the start/update functions will be changed once we have the actual dimesions of our 'aquarium'
	public Vector3 seekPosition;

	//The aquariumDistance Vector is a placeholder till we set up the actual 'aquarium'.
	public Vector3 aquariumDistance;

//-----------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		aquariumDistance = new Vector3(20, 10, 20);
		generateSeekPosition();
		
	}

//-----------------------------------------------------------------------------
	
	// Update is called once per frame
	void Update () {

		if(Random.Range(0, 1000) < 10){
			generateSeekPosition();
		}
		
	}

//-----------------------------------------------------------------------------

public List<GameObject> getAllFish(){

	List<GameObject> allFish = new List<GameObject>();

		int numChildren = transform.childCount;

		for(int i = 0; i < numChildren; i++){
			GameObject g = transform.GetChild(i).gameObject;
			allFish.Add(g);
		}

	return allFish;

}

//-----------------------------------------------------------------------------

void generateSeekPosition(){
	seekPosition = new Vector3(Random.Range(-aquariumDistance.x, aquariumDistance.x),
							Random.Range(-aquariumDistance.y, aquariumDistance.y),
							Random.Range(-aquariumDistance.z, aquariumDistance.z));
}


}
