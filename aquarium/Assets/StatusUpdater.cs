using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUpdater : MonoBehaviour {
	private string prefix = "Number of fish: ";
	private string formatText = "{0}";
	private Text text;
	public FishFlocker fishManager;

	void Start () {
		text = GetComponent<Text>();
		text.text = prefix + string.Format(formatText, fishManager.getAllFish().Count);
	}

	// FIXME inefficient, but doesn't really matter now, as this is only executed when menu is visible
	void Update () {
		text.text = prefix + string.Format(formatText, fishManager.getAllFish().Count);
	}
}
