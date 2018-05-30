using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValueLegend : MonoBehaviour {
	private string formatText = "{0}";
	private Text text;

	void Start () {
		GetComponentInParent<Slider>().onValueChanged.AddListener(HandleValueChanged);
		text = GetComponent<Text>();
	}

	private void HandleValueChanged(float value) {
        text.text = string.Format(formatText, value);
	}
}
