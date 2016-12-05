using UnityEngine;
using System.Collections.Generic;

public class BoidController : MonoBehaviour
{
	private Rigidbody body;

    //Bear Business
    public static Rigidbody bearBody = null;

    //Bear Terror Vars
    [SerializeField]
    private bool terrorEnabled;

    [SerializeField]
    private float bearAvoidanceRadius;

    [SerializeField]
    private float bearAvoidanceStrength;

    //Boid Business
    [SerializeField]
	private Vector3 initialVelocityMin;

	[SerializeField]
	private Vector3 initialVelocityMax;

	[SerializeField]
	private float forceAccumulationMax;

	[SerializeField]
	private float visionRadius;

	[SerializeField]
	private float avoidanceRadius;

	[SerializeField]
	private float avoidanceStrength;

	[SerializeField]
	private float centeringStrength;

	[SerializeField]
	private float timeToMatchVelocity;

	[SerializeField]
	private float velocityMatchScale;

	[SerializeField]
	private float maxVelocity;

	[SerializeField]
	private float minVelocity;

	[SerializeField]
	private float playerAttractionStrength;

	// Need to store local awareness of
	// 1. Boids
	public static List<Rigidbody> boidList = null;
	public static PlayerBoid player = null;
    public static PlayerBear bearPlayer = null;
	// 2. Obstacles
	// 3. Predators

	// Initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();
        bearBody = GetComponent<Rigidbody>();
        // body.velocity = new Vector3(Random.Range(initialVelocityMin.x, initialVelocityMax.x),
        // 		Random.Range(initialVelocityMin.y, initialVelocityMax.y),
        // 		Random.Range(initialVelocityMin.z, initialVelocityMax.z));

        avoidanceRadius = avoidanceRadius * avoidanceRadius;
		visionRadius = visionRadius * visionRadius;

        bearAvoidanceRadius = bearAvoidanceRadius * bearAvoidanceRadius;


		// Initialize the boidList
		if (boidList == null)
			boidList = new List<Rigidbody>();

		// Add self to boidList
		boidList.Add(body);
	}

	void FixedUpdate ()
	{
		// Turn the boid to it's current velocity
		body.rotation = Quaternion.LookRotation(body.velocity);

		// Make sure velocity is between magnitude (2,10)
		float speed = body.velocity.magnitude;
		if (speed < minVelocity)
		{
			float fac = minVelocity / speed;
			body.velocity *= fac;
		}
		else if (speed > maxVelocity)
		{
			float fac = maxVelocity / speed;
			body.velocity *= fac;
		}

		// Step 1: Update spatial awareness (Automatically done)

		// Step 2: Generate acceleration vectors
		List<Vector3> avoidance = new List<Vector3>();
		Vector3 flockPositionMin = new Vector3();
		Vector3 flockPositionMax = new Vector3();
		int flockMates = 0;
		Vector3 flockVelocity = new Vector3();

        if (terrorEnabled)
        {
            //Fear the Bear
            Vector3 displacement = bearBody.position - body.position;
            float sqrDist = displacement.sqrMagnitude;
            if (sqrDist < bearAvoidanceRadius)
            {
                // If close, flee in terror
                avoidance.Add(displacement.normalized * (-1) * (bearAvoidanceStrength / sqrDist));
            }
        }


        foreach (Rigidbody boid in boidList)
		{
			if (boid != body)
			{
				Vector3 displacement = boid.position - body.position;
				float sqrDist = displacement.sqrMagnitude;
				if (sqrDist < avoidanceRadius)
				{
					// If close, move away
					avoidance.Add(displacement.normalized * (-1) * (avoidanceStrength / sqrDist));
				}
				if (sqrDist < visionRadius)
				{
					// Update awareness of flock
					if (flockMates == 0)
					{
						flockPositionMin = boid.position;
						flockPositionMax = boid.position;
					}
					else
					{
						flockPositionMin = Vector3.Min(flockPositionMin, boid.position);
						flockPositionMax = Vector3.Max(flockPositionMax, boid.position);
					}

					// Match Velocities
					flockVelocity += boid.velocity;

					flockMates++;

					if (player && boid == player.body && player.attractionEnabled)
					{
						avoidance.Add(displacement * playerAttractionStrength);
					}
				}
			}
		}

		List<Vector3> accelerations = new List<Vector3>();
		accelerations.AddRange(avoidance);

		Vector3 flockCenter = new Vector3((flockPositionMin.x + flockPositionMax.x) / 2, 0, (flockPositionMin.z + flockPositionMax.z) / 2);
		if (flockMates > 0)
		{
			accelerations.Add((flockCenter - body.position).normalized * centeringStrength);
			accelerations.Add(((((flockVelocity / flockMates) * velocityMatchScale) - body.velocity) / timeToMatchVelocity) * body.mass);
		}

		// Step 3: Accumulate acceleration vectors
		Vector3 output = AccumulateAccelerations(accelerations);

		// Step 4: Apply acceleration vectors to boid
		body.AddForce(output);
	}

	private Vector3 AccumulateAccelerations (ICollection<Vector3> accelerations)
	{
		float totalMagnitude = 0;
		Vector3 result = Vector3.zero;

		foreach (Vector3 acc in accelerations)
		{
			totalMagnitude += acc.magnitude;
			if (totalMagnitude >= forceAccumulationMax)
			{
				// Scale the acceleration down when its entire magnitude not used
				float scale = 1 - ((totalMagnitude - forceAccumulationMax) / acc.magnitude);
				result += scale * acc;
				break;
			}
			else
				result += acc;
		}

		return result;
	}
}
