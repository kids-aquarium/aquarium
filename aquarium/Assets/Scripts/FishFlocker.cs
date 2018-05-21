using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlocker : MonoBehaviour {

	public FlockingParameters parameters;
	public bool passParametersToFlock;

	float minScale = 0;
	float maxScale = 1000;
	[Range(0, 1000)] // NB: need to match above
	public float fishScale = 1;

	void Start () {
		//generateSeekPosition();
		LoadPreferences();
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
		foreach (Transform child in transform) {
			child.localScale = new Vector3(fishScale, fishScale, fishScale);
		}
		if (fishScale < maxScale && Input.GetKey("up")) fishScale += 1f;
		if (fishScale > minScale && Input.GetKey("down")) fishScale -= 1f;
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

	void SavePreferences() {
		PlayerPrefs.SetFloat("fishScale", fishScale);
	}

	void LoadPreferences() {
		fishScale = PlayerPrefs.GetFloat("fishScale", fishScale);
	}

	void OnApplicationQuit() {
		SavePreferences();
	}

}
