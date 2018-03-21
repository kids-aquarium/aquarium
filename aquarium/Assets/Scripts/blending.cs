using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class blending : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        ;
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 20);
	}

    public void ReadFishFile(string filename)
    {
        Debug.Log("Fish file name is " + filename);
    }
}
