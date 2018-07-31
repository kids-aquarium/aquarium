using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct populationIndex{
	public int minPopulation;
	public int maxPopulation;
};

public class FishFlocker : MonoBehaviour {

	public FlockingParameters parameters;
	public bool passParametersToFlock;

	float minScale = 0;
	float maxScale = 5000;
	[Range(0, 5000)] // NB: need to match above
	public float fishScale = 1;

	[Range(10, 2628000)] // up to a month
	public float oldAge = 604800; // a week in seconds

	public populationIndex[] populationIndexes = new populationIndex[10];

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

		List<GameObject> allFish = getAllFish();

		//Unique list for each breed.
		List<GameObject>[] fishByBreed = new List<GameObject>[10];

		for(int i = 0; i < fishByBreed.Length; i++){
			fishByBreed[i] = new List<GameObject>();
		}

		//This will hold all the alive populations for each breed.
		int[] fishCountByBreed = new int[10];

		for(int i = 0; i<fishCountByBreed.Length; i++){ fishCountByBreed[i] = -1;} //Setting a temp value for now, seems to give some bugs otherwise once in a while.


		foreach(GameObject fish in allFish){
			if(fish.GetComponent<FlockingFish>().dying == false){
				int index = fish.GetComponent<FlockingFish>().GetBreed();
				fishByBreed[index].Add(fish.gameObject);
			}
		}

		for(int i = 0; i<fishByBreed.Length; i++){
			fishCountByBreed[i] = fishByBreed[i].Count;
			// Debug.Log("Number of fishes for breed " + i + " is: " + fishCountByBreed[i]);
		}

		//Go through each now. Sigh. I hate array of arrays. :|

		for(int i = 0; i<fishCountByBreed.Length; i++){
			if(fishCountByBreed[i] > populationIndexes[i].minPopulation){
				GameObject oldestFish = findOldest(fishByBreed[i]);

				if(oldestFish != null){
					if(oldestFish.GetComponent<FlockingFish>().age > oldAge){
						oldestFish.GetComponent<FlockingFish>().dying = true;
					}
				}
			}

			if(fishCountByBreed[i] > populationIndexes[i].maxPopulation){
				GameObject oldestFish = findOldest(fishByBreed[i]);
				if(oldestFish != null) {oldestFish.GetComponent<FlockingFish>().dying = true;}
			}
		}

	}

	GameObject findOldest(List<GameObject> fishes){
		float oldestAge = 0;

		GameObject oldestFish = null;

		foreach(GameObject fish in fishes){
			if(fish.GetComponent<FlockingFish>().age > oldestAge){
				oldestFish = fish;
				oldestAge = fish.GetComponent<FlockingFish>().age;
			}
		}

		return oldestFish;
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

	public void SavePreferences() {
		PlayerPrefs.SetFloat("fishScale", fishScale);
		PlayerPrefs.SetFloat("fishMinimumSpeed", parameters.minSpeed);
		PlayerPrefs.SetFloat("fishMaximumSpeed", parameters.maxSpeed);
		//PlayerPrefs.SetInt  ("minimumPopulation", minimumPopulation);
		//PlayerPrefs.SetInt  ("maximumPopulation", maximumPopulation);
		PlayerPrefs.SetFloat("oldAge", oldAge);
	}

	void LoadPreferences() {
		fishScale = PlayerPrefs.GetFloat("fishScale");
		parameters.minSpeed = PlayerPrefs.GetFloat("fishMinimumSpeed");
		parameters.maxSpeed = PlayerPrefs.GetFloat("fishMaximumSpeed");
		//minimumPopulation = PlayerPrefs.GetInt("minimumPopulation");
		//maximumPopulation = PlayerPrefs.GetInt("maximumPopulation");
		oldAge = PlayerPrefs.GetFloat("oldAge");
	}

	void OnApplicationQuit() {
		SavePreferences();
	}

	public void SetFishScale(float scale) {
		fishScale = scale;
	}

	public void SetFishMinimumSpeed(float minSpeed) {
		parameters.minSpeed = minSpeed;
	}

	public void SetFishMaximumSpeed(float maxSpeed) {
		parameters.maxSpeed = maxSpeed;
	}

	// public void SetMinimumPopulation(float minimumPopulation) {
	// 	this.minimumPopulation = Mathf.RoundToInt(minimumPopulation);
	// }

	// public void SetMaximumPopulation(float maximumPopulation) {
	// 	this.maximumPopulation = Mathf.RoundToInt(maximumPopulation);
	// }

	public void SetOldAge(float oldAge) {
		this.oldAge = oldAge;
	}
}
