using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlocker : MonoBehaviour {

	public Vector3 seekPosition;
	public Vector3 aquariumSize;

//-----------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		aquariumSize = new Vector3(4, 3, 2);

		seekPosition = new Vector3(Random.Range(-aquariumSize.x, aquariumSize.x),
							Random.Range(-aquariumSize.y, aquariumSize.y),
							Random.Range(-aquariumSize.z, aquariumSize.z));
		
	}

//-----------------------------------------------------------------------------
	
	// Update is called once per frame
	void Update () {

		if(Random.Range(0, 1000) < 10){
			seekPosition = new Vector3(Random.Range(-aquariumSize.x, aquariumSize.x),
								Random.Range(-aquariumSize.y, aquariumSize.y),
								Random.Range(-aquariumSize.z, aquariumSize.z));
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


}
