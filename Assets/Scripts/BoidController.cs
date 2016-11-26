using UnityEngine;
using System.Collections.Generic;

public class BoidController : MonoBehaviour 
{
	private Rigidbody body;

	[SerializeField]
	private float maxMagnitude;

	// Need to store local awareness of
	// 1. Boids
	// 2. Obstacles
	// 3. Predators

	// Initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		// Step 1: Update spatial awareness

		// Step 2: Generate a container of acceleration vectors


		// Step 3: Accumulate acceleration vectors



		// Step 4: Apply acceleration vectors to boid
	}

	private Vector3 AccumulateAccelerations (ICollection<Vector3> accelerations)
	{
		float totalMagnitude = 0;
		Vector3 result = Vector3.zero;

		foreach (Vector3 acc in accelerations)
		{
			totalMagnitude += acc.magnitude;
			if (totalMagnitude >= maxMagnitude)
			{
				// Scale the acceleration down when its entire magnitude not used
				float scale = 1 - ((totalMagnitude - maxMagnitude) / acc.magnitude);
				result += scale * acc;
				break;
			}
			else
				result += acc;
		}

		return result;
	}
}
