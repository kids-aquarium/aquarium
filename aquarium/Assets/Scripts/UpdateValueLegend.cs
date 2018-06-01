using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValueLegend : MonoBehaviour {
	private string formatText = "{0}";
	private Text text;

	void Start () {
		Slider s = GetComponentInParent<Slider>();
		s.onValueChanged.AddListener(HandleValueChanged);
		text = GetComponent<Text>();
		text.text = string.Format(formatText, s.value);
	}

	private void HandleValueChanged(float value) {
        text.text = string.Format(formatText, value);
	}
}
