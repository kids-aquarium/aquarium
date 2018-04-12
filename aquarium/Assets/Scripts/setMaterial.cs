using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setMaterial : MonoBehaviour {

	Component[] fishMaterial;

	// Use this for initialization
	void Start () {

		// LoadTexture("fishTexTest004");
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadTexture(string _texture){
		fishMaterial = GetComponentsInChildren<Renderer>();

		foreach(Renderer rend in fishMaterial){
			Texture t = Resources.Load(_texture) as Texture;
			
			if (t == null)
			{
				Debug.Log("Load Fails");
			} else {
				Debug.Log("Load Successful");
			}

			rend.material.SetTexture("_MainTex", t);
		}
	}

	public void LoadTexture2D(Texture2D _tex){
		fishMaterial = GetComponentsInChildren<Renderer>();

		foreach(Renderer ren in fishMaterial){
			ren.material.mainTexture = _tex;
		}
	}
}
