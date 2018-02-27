using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorPid
{
	public float pFactor, iFactor, dFactor;

	private Vector3 integral;
	private Vector3 lastError;

	public VectorPid(float pFactor, float iFactor, float dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}

	public Vector3 Update(Vector3 currentError, float timeFrame)
	{
		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;
		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;
	}
}

[System.Serializable]
public class FlockingParameters {
	public int 	 breed             = 0;
	public float minSpeed          = 0.1f;
	public float maxSpeed          = 5.0f;
	public float rotationSpeed     = 3.0f;
	public float desiredSeparation = 1.0f;
	public float separationWeight  = 1.0f;
	public float alignmentDistance = 6.0f;
	public float alignmentWeight   = 1.0f;
	public float cohesionDistance  = 6.0f;
	public float cohesionWeight    = 1.0f;
	public float minimumX          = -50.0f;
	public float maximumX          = 50.0f;
	public float minimumY          = 0.0f;
	public float maximumY          = 5.0f;
	public float minimumZ          = 0.0f;
	public float maximumZ          = 20.0f;
};

public class FlockingFish : MonoBehaviour {
	public FlockingParameters parameters;

	Rigidbody rb;


	private readonly VectorPid angularVelocityController = new VectorPid(33.7766f, 0, 0.2553191f);
	private readonly VectorPid headingController = new VectorPid(9.244681f, 0, 0.06382979f);
	private readonly VectorPid upController = new VectorPid(9.244681f, 0, 0.06382979f); // TODO balance

	void Start () {
		rb = GetComponent<Rigidbody>();
	}

	public static void DrawPoint (Vector3 pos, Color col, float scale)
	{
		Vector3[] points = new Vector3[] 
		{
			pos + (Vector3.up * scale), 
			pos - (Vector3.up * scale), 
			pos + (Vector3.right * scale), 
			pos - (Vector3.right * scale), 
			pos + (Vector3.forward * scale), 
			pos - (Vector3.forward * scale)
		}; 		

		Debug.DrawLine (points[0], points[1], col ); 
		Debug.DrawLine (points[2], points[3], col ); 
		Debug.DrawLine (points[4], points[5], col ); 

		Debug.DrawLine (points[0], points[2], col ); 
		Debug.DrawLine (points[0], points[3], col ); 
		Debug.DrawLine (points[0], points[4], col ); 
		Debug.DrawLine (points[0], points[5], col ); 

		Debug.DrawLine (points[1], points[2], col ); 
		Debug.DrawLine (points[1], points[3], col ); 
		Debug.DrawLine (points[1], points[4], col ); 
		Debug.DrawLine (points[1], points[5], col ); 

		Debug.DrawLine (points[4], points[2], col ); 
		Debug.DrawLine (points[4], points[3], col ); 
		Debug.DrawLine (points[5], points[2], col ); 
		Debug.DrawLine (points[5], points[3], col ); 

	}

	void FixedUpdate () {
		MatchVelocity ();
		ConstrainVelocityToLocalForward ();

		Vector3 targetHeading = transform.forward;

		Vector3? separation = Separate ();
		Vector3? cohesion = Cohere ();
		Vector3? alignment = Align ();

		Vector3? bounds = CheckBoundaries (); // FIXME if out of bounds, maybe skip all other checks


		if (cohesion != null) {
			Vector3 cohesionHeading = cohesion.Value - transform.position;
			cohesionHeading.Normalize ();
			//Debug.DrawRay (transform.position, cohesionHeading, Color.red);
			targetHeading += cohesionHeading * parameters.cohesionWeight;
			//DrawPoint (cohesion.Value, Color.red, 1);
			//turnTowardsWorldPosition (cohesion.Value);
		}
		if (separation != null) {
			Vector3 separationHeading = separation.Value - transform.position;
			separationHeading.Normalize ();
			//Debug.DrawRay (transform.position, separationHeading, Color.green);
			targetHeading += separationHeading * parameters.separationWeight;
			//DrawPoint (separation.Value, Color.green, 1);
			//turnTowardsWorldPosition (separation.Value);
		}
		if (alignment != null) {
			Vector3 alignmentHeading = alignment.Value - transform.position;
			alignmentHeading.Normalize ();
			//Debug.DrawRay (transform.position, alignmentHeading, Color.blue);
			targetHeading += alignmentHeading * parameters.alignmentWeight;
			//DrawPoint (alignment.Value, Color.blue, 1);
			//turnTowardsWorldPosition (alignment.Value);
		}

		if (bounds != null) {
			targetHeading = bounds.Value - transform.position;
		}
		targetHeading.Normalize ();

		//DrawPoint (target, Color.yellow, 1);
		//turnTowards (transform.TransformPoint(separation));
		//Debug.DrawRay(transform.position, targetHeading);
		turnTowardsHeading(targetHeading);
	}

	void ConstrainVelocityToLocalForward() {
		Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
		localVelocity.x = 0;
		localVelocity.y = 0;

		rb.velocity = transform.TransformDirection(localVelocity);
	}

	void turnTowardsWorldPosition(Vector3 target) {
		var angularVelocityError = rb.angularVelocity * -1;
		//Debug.DrawRay(transform.position, rb.angularVelocity * 10, Color.black);

		var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.deltaTime);
		//Debug.DrawRay(transform.position, angularVelocityCorrection, Color.green);

		rb.AddTorque(angularVelocityCorrection);

		var desiredHeading = target - transform.position;
		//Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

		var currentHeading = transform.forward;
		//Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

		var headingError = Vector3.Cross(currentHeading, desiredHeading);
		var headingCorrection = headingController.Update(headingError, Time.deltaTime);

		rb.AddTorque(headingCorrection);
	}

	void turnTowardsHeading(Vector3 desiredHeading, Vector3? desiredUp = null) {
		if (desiredUp == null)
			desiredUp = Vector3.up;
		// local y points up, up control should happen as rotations about x and z axes
		
		var angularVelocityError = rb.angularVelocity * -1;
		//Debug.DrawRay(transform.position, rb.angularVelocity * 10, Color.black);

		var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.deltaTime);
		//Debug.DrawRay(transform.position, angularVelocityCorrection, Color.green);

		rb.AddTorque(angularVelocityCorrection);

		//Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

		var currentHeading = transform.forward;
		//Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

		var headingError = Vector3.Cross(currentHeading, desiredHeading);
		var headingCorrection = headingController.Update(headingError, Time.deltaTime);

		rb.AddTorque(headingCorrection);

		var currentUp = transform.up;
		var upError = Vector3.Cross (currentUp, desiredUp.Value);
		var upCorrection = upController.Update (upError, Time.deltaTime);
		rb.AddTorque (upCorrection);
	}

	void MatchVelocity() {
		if (rb.velocity.magnitude <= parameters.minSpeed)
			rb.AddRelativeForce (Vector3.forward);
		if (rb.velocity.magnitude >= parameters.maxSpeed)
			rb.AddRelativeForce (Vector3.back);
	}

	Vector3? CheckBoundaries() {
		if (transform.position.x <  parameters.minimumX ||
		    transform.position.x >= parameters.maximumX ||
		    transform.position.y <  parameters.minimumY ||
		    transform.position.y >= parameters.maximumY ||
		    transform.position.z <  parameters.minimumZ ||
		    transform.position.z >= parameters.maximumZ) {
			Vector3 c = new Vector3 ((parameters.minimumX + parameters.maximumX) / 2.0f,
				                     (parameters.minimumY + parameters.maximumY) / 2.0f,
				                     (parameters.minimumZ + parameters.maximumZ) / 2.0f);
			return c;
		}
		else return null;
	}

	Vector3? Separate() {
		Vector3 c = Vector3.zero;
		List<GameObject> fishes = GetComponentInParent<FishFlocker> ().getAllFish ();
		int numberOfAffectingFishes = 0;
		foreach (GameObject other in fishes) {
			if (this != other) {
				Rigidbody otherRb = other.GetComponent<Rigidbody> ();
				float distance = Vector3.Distance (rb.position, otherRb.position);
				if (distance < parameters.desiredSeparation && distance > 0) {
					++numberOfAffectingFishes;
					Vector3 d = otherRb.position - rb.position;
					//d.Normalize ();
					//d /= distance;
					c -= d;
				}
			}
		}
		if (numberOfAffectingFishes == 0)
			return null;
		//Debug.DrawRay(transform.position, c);
		c.Normalize();
		return c + transform.position;
	}

	Vector3? Cohere() {
		Vector3 c = Vector3.zero;
		List<GameObject> fishes = GetComponentInParent<FishFlocker>().getAllFish();
		int numberOfAffectingFishes = 0;
		foreach (GameObject other in fishes) {
			if (this != other) {
				Rigidbody otherRb = other.GetComponent<Rigidbody> ();
				float distance = Vector3.Distance (rb.position, otherRb.position);
				if (distance < parameters.cohesionDistance && distance > 0) {
					++numberOfAffectingFishes;
					Vector3 d = otherRb.position;
					c += d;
				}
			}
		}
		if (numberOfAffectingFishes == 0)
			return null;
		c /= numberOfAffectingFishes;
		return c;
	}

	Vector3? Align() {
		Vector3 c = Vector3.zero;
		List<GameObject> fishes = GetComponentInParent<FishFlocker>().getAllFish();
		int numberOfAffectingFishes = 0;
		foreach (GameObject other in fishes) {
			if (this != other) {
				Rigidbody otherRb = other.GetComponent<Rigidbody> ();
				float distance = Vector3.Distance (rb.position, otherRb.position);
				if (distance < parameters.alignmentDistance && distance > 0) {
					++numberOfAffectingFishes;
					Vector3 d = otherRb.transform.forward;
					c += d;
				}
			}
		}
		if (numberOfAffectingFishes == 0)
			return null;
		c.Normalize ();
		//Debug.DrawRay(transform.position, c);
		return c + transform.position;
	}
}

