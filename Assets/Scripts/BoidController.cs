using UnityEngine;
using System.Collections.Generic;

public class BoidController : MonoBehaviour
{
	private Rigidbody body;

	// instantaneous velocity max
	// instantaneous force max

	[SerializeField]
	private Vector3 initialVelocity;

	[SerializeField]
	private float maxForceMagnitude;

	[SerializeField]
	private float repulsionRadius;

	[SerializeField]
	private float repulsionScale;

	[SerializeField]
	private float attractionRadius;

	[SerializeField]
	private float attractionScale;

	// Need to store local awareness of
	// 1. Boids
	private static Dictionary<int, Vector3> boidList = null;
	private int id;
	private static int nextId = 0;
	// 2. Obstacles
	// 3. Predators
	private Vector3 predLoc;

	// Initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();
		body.velocity = initialVelocity;

		// Initialize the boidList and square radius's if needed
		if (boidList == null)
		{
			boidList = new Dictionary<int, Vector3>();
			repulsionRadius = repulsionRadius * repulsionRadius;
			attractionRadius = attractionRadius * attractionRadius;
		}

		// Generate id for this boid
		id = nextId;
		nextId++;

		// Add to boidList
		boidList.Add(id, body.position);
	}

	void FixedUpdate ()
	{
		// Step 1: Update spatial awareness
		boidList[id] = body.position;

		// Step 2: Generate a container of acceleration vectors
		List<Vector3> repulsions = new List<Vector3>();
		List<Vector3> attractions = new List<Vector3>();
		List<Vector3> velocityMatching = new List<Vector3>();
		foreach (KeyValuePair<int, Vector3> entry in boidList)
		{
			if (entry.Key != id) 
			{
				Vector3 displacement = entry.Value - body.position;
				float sqrDist = displacement.sqrMagnitude;
				if (sqrDist < repulsionRadius) 
				{
					// If close, move away
					// add to list of repulsions
					repulsions.Add(displacement.normalized * (-1) * (repulsionScale / sqrDist));
				}
				else if (sqrDist < attractionRadius) 
				{
					// If far, move closer
					// add to list of attractions
					attractions.Add(displacement.normalized * (attractionScale / sqrDist));

					// attempt to match velocity of other boid
					velocityMatching.Add()
				}
			}
		}

		// Step 3: Accumulate acceleration vectors
		// sum each list into its own combined vector representing different forces acting on the boid, adding each to the list of vectors to be accumulated
		List<Vector3> accelerations = new List<Vector3>();
		accelerations.AddRange(repulsions);
		accelerations.AddRange(attractions);
		Vector3 output = AccumulateAccelerations(accelerations);

		// Step 4: Apply acceleration vectors to boid
		body.AddForce(output);

		// need to apply the found vector by rotating towards it, and then applying (a percentage of) its magnitude as a force in the direction currently faced
	}

	private Vector3 AccumulateAccelerations (ICollection<Vector3> accelerations)
	{
		float totalMagnitude = 0;
		Vector3 result = Vector3.zero;

		foreach (Vector3 acc in accelerations)
		{
			totalMagnitude += acc.magnitude;
			if (totalMagnitude >= maxForceMagnitude)
			{
				// Scale the acceleration down when its entire magnitude not used
				float scale = 1 - ((totalMagnitude - maxForceMagnitude) / acc.magnitude);
				result += scale * acc;
				break;
			}
			else
				result += acc;
		}

		return result;
	}
}
