using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlocker : MonoBehaviour {

	public FlockingParameters parameters;
	public bool passParametersToFlock;

	float minScale = 0;
	float maxScale = 5000;
	[Range(0, 5000)] // NB: need to match above
	public float fishScale = 1;

	[Range(1, 50)] //NB: What's a good value here?
	public int minimumPopulation = 10;

	[Range(1, 500)] // NB: Maximum value should be more than the max of minimumPopulation. Also, good value?
	public int maximumPopulation = 30;

	[Range(10, 10000)] //NB: What's a good value here? For the record, 24 hours is 86400 seconds.
	public float oldAge = 10000; // seconds

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
		List<GameObject> aliveFish = new List<GameObject>();

		foreach(GameObject fish in allFish){
			if(fish.GetComponent<FlockingFish>().dying == false){
				aliveFish.Add(fish.gameObject);
			}
		}

		int realFishCount = aliveFish.Count;

		if(realFishCount > minimumPopulation) {
			GameObject oldestFish = findOldest(aliveFish);

			if(oldestFish != null){
				if((oldestFish.GetComponent<FlockingFish>().age) > oldAge){
					oldestFish.GetComponent<FlockingFish>().dying = true;
				}

			}
		}
		if(realFishCount > maximumPopulation){
			GameObject oldestFish = findOldest(aliveFish);
			if(oldestFish != null) { oldestFish.GetComponent<FlockingFish>().dying = true; }
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
		PlayerPrefs.SetInt  ("minimumPopulation", minimumPopulation);
		PlayerPrefs.SetInt  ("maximumPopulation", maximumPopulation);
		PlayerPrefs.SetFloat("oldAge", oldAge);
	}

	void LoadPreferences() {
		fishScale = PlayerPrefs.GetFloat("fishScale");
		parameters.minSpeed = PlayerPrefs.GetFloat("fishMinimumSpeed");
		parameters.maxSpeed = PlayerPrefs.GetFloat("fishMaximumSpeed");
		minimumPopulation = PlayerPrefs.GetInt("minimumPopulation");
		maximumPopulation = PlayerPrefs.GetInt("maximumPopulation");
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

	public void SetMinimumPopulation(float minimumPopulation) {
		this.minimumPopulation = Mathf.RoundToInt(minimumPopulation);
	}

	public void SetMaximumPopulation(float maximumPopulation) {
		this.maximumPopulation = Mathf.RoundToInt(maximumPopulation);
	}

	public void SetOldAge(float oldAge) {
		this.oldAge = oldAge;
	}
}
