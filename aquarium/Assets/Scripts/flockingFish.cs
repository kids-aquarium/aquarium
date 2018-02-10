﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingFish : MonoBehaviour {
	public float speed;
	float rotationSpeed = 3.0f;
	float neighbourDistance = 4.0f;
	float avoidDist = 1.0f;

	Vector3 otherDir;
	bool turn;

	public float maxSpeed = 0.5f;
	public float minSpeed = 0.1f;

	public float flockingChance;

//-----------------------------------------------------------------------------


	// Use this for initialization
	void Start () {

		speed = Random.Range(minSpeed, maxSpeed);
		turn = false;
		flockingChance = Random.Range(20, 60);
		
	}

//-----------------------------------------------------------------------------
	
	// Update is called once per frame
	void Update () {

		if(turn){
			Vector3 dir = otherDir - this.transform.position;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
			speed = Random.Range(minSpeed, maxSpeed);
		} else {
			if(Random.Range(0, 100) < flockingChance) ApplyRules();
			}

		transform.Translate(transform.forward * speed * Time.deltaTime);
		}

//-----------------------------------------------------------------------------

	void ApplyRules(){

		//Instead of calling this function everytime, should I store a local list
		//which I update with this function call everytime that there is a new fish?
		//For that we'd need to set up listeners to the actual spawning code - necessary
		//or is this good enough?
		List<GameObject> fishes = GetComponentInParent<FishFlocker>().getAllFish();

		Vector3 fCentre = Vector3.zero;
		Vector3 fAvoid = Vector3.zero;
		float groupSpeed = 0.1f;
		float distance;

		Vector3 seek = GetComponentInParent<FishFlocker>().seekPosition;

		int groupSize = 0;

		foreach(GameObject other in fishes){
			if(other != this.gameObject){
				distance = Vector3.Distance(this.transform.position, other.transform.position);

				if(distance <= neighbourDistance){
					fCentre += other.transform.position;
					groupSize ++;

					if(distance < avoidDist){
						fAvoid = fAvoid + (this.transform.position - other.transform.position);
					}

					groupSpeed += other.GetComponent<FlockingFish>().speed;
				}
			}
		}

		if(groupSize > 0){
			fCentre = fCentre/groupSize + (seek - this.transform.position);
			speed = groupSpeed/groupSize;

			// speed = Mathf.Clamp(speed, 0.1f, 10.0f);

			Vector3 direction = (fCentre + fAvoid) - this.transform.position;
			if(direction != Vector3.zero){
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * speed);
			}
		}

	}

//-----------------------------------------------------------------------------

	void OnCollisionEnter(Collision c){

		if(!turn){
			otherDir = this.transform.position - c.collider.gameObject.transform.position;
		}

		turn = true;

	}

//-----------------------------------------------------------------------------

	void OnCollisionExit(Collision c){

		turn = false;

	}

//-----------------------------------------------------------------------------
		
}

