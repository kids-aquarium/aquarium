using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

	public void OnValidate () {
		pFactor = Mathf.Max (0, pFactor);
		iFactor = Mathf.Max (0, iFactor);
		dFactor = Mathf.Max (0, dFactor);
	}
}

[System.Serializable]
public class FlockingParameters {
	public int 	 breed             = 0;
	[Header("Physical parameters")]
	public float minSpeed          = 1.0f;
	public float maxSpeed          = 10.0f;

	[Header("Behaviour parameters")]
	public float desiredSeparation = 1.0f;
	public float separationWeight  = 1.0f;
	public float alignmentDistance = 6.0f;
	public float alignmentWeight   = 1.0f;
	public float cohesionDistance  = 6.0f;
	public float cohesionWeight    = 1.0f;
	public Vector3 destination     = new Vector3(0f, 0f, 0f);
	public float destinationWeight = 1.0f;

	[Header("Boundaries")]
	public bool useFrustumForBounds = true;
	public bool useCurrentY         = false; // Always have fish try to get back to center of screen
	public float minimumZ           = 0.0f;
	public float maximumZ           = 20.0f;

	[Header("Debug")]
	public bool debugDrawings       = true;

	public void OnValidate() {
		minSpeed          = Mathf.Min (minSpeed, maxSpeed);
		maxSpeed          = Mathf.Max (minSpeed, maxSpeed);
		minSpeed          = Mathf.Max (0, minSpeed);
		maxSpeed          = Mathf.Max (0, maxSpeed); // Order matters with speed clamping
		desiredSeparation = Mathf.Max (0, desiredSeparation);
		separationWeight  = Mathf.Max (0, separationWeight);
		alignmentDistance = Mathf.Max (0, alignmentDistance);
		alignmentWeight   = Mathf.Max (0, alignmentWeight);
		cohesionDistance  = Mathf.Max (0, cohesionDistance);
		cohesionWeight    = Mathf.Max (0, cohesionWeight);
		destinationWeight = Mathf.Max (0, destinationWeight);

		minimumZ = Mathf.Min (minimumZ, maximumZ);
		maximumZ = Mathf.Max (minimumZ, maximumZ);
	}
};

public class FlockingFish : MonoBehaviour {
	public FlockingParameters parameters;
	
	//Variables for population control.
	private float BirthTime;
	public float age;
	public bool dying;

	//NB: There might be a better way to set the deathbed. 
	Vector3 deathBed = new Vector3(100, 0, 50); //this is roughly out of screen (+ a bit more) towards the right
												//in Start() there's a chance for it to flip to screen left.

	Rigidbody rb;

	public VectorPid angularVelocityController = new VectorPid(0f, 0f, 0f);
	public VectorPid headingController         = new VectorPid(0f, 0f, 0f);
	public VectorPid upController              = new VectorPid(0f, 0f, 0f);

	void Start () {
		rb = GetComponent<Rigidbody>();
		
		BirthTime = Time.timeSinceLevelLoad;
		dying = false;

		int chance = Random.Range(0, 2);
		if (chance == 1) deathBed.x *= -1; //change the x position of the deathBed randomly for each.

	}

	void OnValidate() {
		parameters.OnValidate ();
		angularVelocityController.OnValidate ();
		headingController.OnValidate ();
		upController.OnValidate ();
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

		age = Time.timeSinceLevelLoad - BirthTime;

		MatchVelocity ();
		ConstrainVelocityToLocalForward ();

		Vector3 targetHeading = transform.forward;

		if(!dying){
			Vector3? separation = Separate ();
			Vector3? cohesion = Cohere ();
			Vector3? alignment = Align ();
			Vector3? destination = Destination ();

			Vector3? bounds = parameters.useFrustumForBounds ? CheckViewFrustum () : CheckBoundaries ();
			if (bounds == null) {
				if (cohesion != null) {
					Vector3 cohesionHeading = cohesion.Value - transform.position;
					cohesionHeading.Normalize ();
					if(parameters.debugDrawings) Debug.DrawRay (transform.position, cohesionHeading, Color.red);
					targetHeading += cohesionHeading * parameters.cohesionWeight;
					if(parameters.debugDrawings) DrawPoint (cohesion.Value, Color.red, 1);
				}
				if (separation != null) {
					Vector3 separationHeading = separation.Value - transform.position;
					separationHeading.Normalize ();
					if(parameters.debugDrawings) Debug.DrawRay (transform.position, separationHeading, Color.green);
					targetHeading += separationHeading * parameters.separationWeight;
					if(parameters.debugDrawings) DrawPoint (separation.Value, Color.green, 1);
				}
				if (alignment != null) {
					Vector3 alignmentHeading = alignment.Value - transform.position;
					alignmentHeading.Normalize ();
					if(parameters.debugDrawings) Debug.DrawRay (transform.position, alignmentHeading, Color.blue);
					targetHeading += alignmentHeading * parameters.alignmentWeight;
					if(parameters.debugDrawings) DrawPoint (alignment.Value, Color.blue, 1);
				}
				if (destination != null) {
					Vector3 destinationHeading = destination.Value - transform.position;
					destinationHeading.Normalize ();
					targetHeading += destinationHeading * parameters.destinationWeight;
				}
			}

			if (bounds != null) {
				targetHeading = bounds.Value - transform.position;
				if(parameters.debugDrawings) Debug.DrawRay(transform.position, targetHeading, Color.cyan);
			}
			targetHeading.Normalize ();

			if(parameters.debugDrawings) Debug.DrawRay(transform.position, targetHeading, Color.yellow);
			turnTowardsHeading(targetHeading);
			
			} else {
			Vector3 destination = deathBed;

			Vector3 destinationHeading = destination - transform.position;
			destinationHeading.Normalize ();
			targetHeading += destinationHeading * parameters.destinationWeight;

			targetHeading.Normalize();

			turnTowardsHeading(targetHeading);

			float distanceToDeathBed = Vector3.Distance(deathBed, transform.position);
			
			if(Mathf.Abs(distanceToDeathBed) < 15.0){
				Destroy(gameObject);
			}
		}
	}

	void ConstrainVelocityToLocalForward() {
		Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
		localVelocity.x = 0;
		localVelocity.y = 0;
		if(localVelocity.z < 0) localVelocity.z = 0;

		Vector3 constrainedVelocity = transform.TransformDirection(localVelocity);
		rb.velocity = constrainedVelocity;
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
		//Debug.Log (angularVelocityError);
		//Debug.DrawRay(transform.position, rb.angularVelocity * 10, Color.black);


		var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.deltaTime);
		Debug.DrawRay(transform.position, angularVelocityCorrection, Color.red);
		rb.AddTorque(angularVelocityCorrection);

		//Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

		var currentHeading = transform.forward;
		//Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

		var headingError = Vector3.Cross(currentHeading, desiredHeading);
		var headingCorrection = headingController.Update(headingError, Time.deltaTime);
		Debug.DrawRay(transform.position, headingCorrection, Color.green);
		rb.AddTorque(headingCorrection);

		var currentUp = transform.up;
		var upError = Vector3.Cross (currentUp, desiredUp.Value);
		var upCorrection = upController.Update (upError, Time.deltaTime);
		Debug.DrawRay(transform.position, upCorrection, Color.blue);
		rb.AddTorque (upCorrection);
	}

	void MatchVelocity() {
		if (rb.velocity.magnitude <= parameters.minSpeed)
			rb.AddRelativeForce (Vector3.forward);
		if (rb.velocity.magnitude >= parameters.maxSpeed)
			rb.AddRelativeForce (Vector3.back);
	}

	Vector3? CheckBoundaries() {
		// TODO implement
		throw new System.NotImplementedException ();
	}

	Vector3? CheckViewFrustum() {
		Vector2 screenPoint = Camera.main.WorldToScreenPoint (transform.position);
		if (screenPoint.x >= 0 && screenPoint.x < Screen.width &&
			screenPoint.y >= 0 && screenPoint.y < Screen.height &&
			transform.position.z >= parameters.minimumZ && transform.position.z < parameters.maximumZ) {
			return null;
		} else {
			Vector3 c;
			if (parameters.useCurrentY) {
				c = new Vector3 (0.0f, transform.position.y, (parameters.minimumZ + parameters.maximumZ) / 2.0f);
			} else {
				c = new Vector3 (0.0f, 0.0f, (parameters.minimumZ + parameters.maximumZ) / 2.0f);
			}
			return c;
		}
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
			if ((this != other) && (this.GetComponent<FlockingParameters>().breed == other.GetComponent<FlockingParameters>().breed)) { //cohesion only with same breed
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
			if ((this != other) && (this.GetComponent<FlockingParameters>().breed == other.GetComponent<FlockingParameters>().breed)) { //Only align with same breed
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

	Vector3? Destination() {
		return parameters.destination;
	}
}
