using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlocker : MonoBehaviour {

	public FlockingParameters parameters;
	public bool passParametersToFlock;

	void Start () {
		//generateSeekPosition();
	}

	void Update () {
		if (passParametersToFlock) {
			foreach (Transform child in transform) {
				FlockingFish f = child.gameObject.GetComponent (typeof(FlockingFish)) as FlockingFish;
				f.parameters = parameters;
			}
		}
		if(Random.Range(0, 1000) < 5) {
			//generateSeekPosition();
		}
	}

	public List<GameObject> getAllFish(){
		List<GameObject> allFish = new List<GameObject>();
		foreach(Transform child in transform){
			allFish.Add(child.gameObject);
		}
		return allFish;
	}

	void generateSeekPosition(){
		// TODO implement
		throw new System.NotImplementedException ();
	}

	void OnValidate() {
		parameters.OnValidate ();
	}

}
