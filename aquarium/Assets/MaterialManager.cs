using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {
	
	public string textureName;
	public Component[] fishMaterial;

	// Use this for initialization
	void Start () {

		fishMaterial = GetComponentsInChildren<Renderer>();

		foreach(Renderer rend in fishMaterial){
			Texture t = Resources.Load(textureName) as Texture;
			
			if (t == null)
			{
				Debug.Log("Load Fails");
			} else {
				Debug.Log("Load Successful");
			}

			rend.material.SetTexture("_MainTex", t);
		}
		
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
