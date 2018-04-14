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
				//Debug.Log(filename);
    
		}
		}

		yield return null;
	}
}
