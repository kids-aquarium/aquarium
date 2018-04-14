using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StreamingFolder : MonoBehaviour {

	public GameObject FishManager;
	List<string> CachedFishes = new List<string>();
	IEnumerator fishChecker;

	// Use this for initialization
	void Start () {

		//Load all files in the folder into the CachedFishes to avoid system temp files.
		//IMPORTANT: This would also prevent pre-existing fish files from instantiating, only files added to the folder
		//AFTER the build is running would work.

		var info = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] fileInfo = info.GetFiles();

		if(fileInfo != null){
			foreach(FileInfo file in fileInfo){
				string name = file.Name;
				CachedFishes.Add(name);
		}
		}
		
	}
	
	// Update is called once per frame
	void Update () {

		fishChecker = CheckForFishes();
		StartCoroutine(fishChecker);
		
	}

	IEnumerator CheckForFishes(){

		Debug.Log("Checking for fishes...");

		var info = new DirectoryInfo(Application.streamingAssetsPath);
           
		FileInfo[] fileInfo = info.GetFiles();

		if (fileInfo == null){
			Debug.Log("SA folder is empty");
			yield return null;
		
		} else {

			foreach (FileInfo file in fileInfo) {
				string filename = file.Name;
				
				//Go through CachedFishes to check for duplicates...
				foreach(string cached in CachedFishes){
					if(filename == cached){
						Debug.Log("Found Duplicate");
						yield return null;
				} else {
					Debug.Log("New Fish!");
					WWW FishFile = new WWW("Application.streamingAssetsPath + filename");

					yield return FishFile;
					
					FishManager.GetComponent<InstantFish>().InstantFishFromFile(FishFile);

					CachedFishes.Add(filename);
				}

			}
		}
		}

		yield return null;
	}
}
