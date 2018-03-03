using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {
	
	// public string textureName;
	Component[] fishMaterial;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void loadTexture(string _texture){
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

}
