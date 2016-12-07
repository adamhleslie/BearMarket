using UnityEngine;
using System.Collections.Generic;

public class BoidController : MonoBehaviour
{
	private Rigidbody body;

	// Boid Flocking Vars
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
	private bool drawFlock;
	GameObject cube;

	// Player Boid Vars
	[SerializeField]
	private bool playerAttractionEnabled;

	[SerializeField]
	private float playerAttractionStrength;

	// Pred Vars
	[SerializeField]
	private bool predAvoidanceEnabled;

	[SerializeField]
	private float predAvoidanceRadius;

	[SerializeField]
	private float predAvoidanceStrength;

	// Awareness of all boids, obstacles, and preds
	public static List<Rigidbody> boidList = new List<Rigidbody>();
	public static List<PlayerBoid> boidPlayers = new List<PlayerBoid>();
	public static List<PlayerPredator> predPlayers = new List<PlayerPredator>();

	// Initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();
		body.velocity = new Vector3(Random.Range(initialVelocityMin.x, initialVelocityMax.x),
									Random.Range(initialVelocityMin.y, initialVelocityMax.y),
									Random.Range(initialVelocityMin.z, initialVelocityMax.z));

		avoidanceRadius = avoidanceRadius * avoidanceRadius;
		visionRadius = visionRadius * visionRadius;
		predAvoidanceRadius = predAvoidanceRadius * predAvoidanceRadius;

		// Add self to boidList
		boidList.Add(body);
	}

	void FixedUpdate ()
	{
		// Clamp velocity to a valid magnitude
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

		// Turn the boid to it's current velocity
		body.rotation = Quaternion.LookRotation(body.velocity);

		// Step 1: Update spatial awareness (Automatically done)

		// Step 2: Generate acceleration vectors
		List<Vector3> playerInfluence = new List<Vector3>();
		if (predAvoidanceEnabled)
		{
			// Fear the Pred
			foreach (PlayerPredator pred in predPlayers)
			{
				Vector3 displacement = pred.body.position - body.position;
				float sqrDist = displacement.sqrMagnitude;
				if (sqrDist < predAvoidanceRadius)
				{
					// If close, flee in terror
					playerInfluence.Add(displacement.normalized * (-1) * (predAvoidanceStrength / sqrDist));
				}
			}
		}
		if (playerAttractionEnabled)
		{
			foreach (PlayerBoid player in boidPlayers)
			{
				if (player.attractionEnabled)
				{
					Vector3 displacement = player.body.position - body.position;
					float sqrDist = displacement.sqrMagnitude;
					if (sqrDist < visionRadius)
					{
						playerInfluence.Add(displacement.normalized * playerAttractionStrength);
					}
				}
			}
		}

		List<Vector3> avoidance = new List<Vector3>();
		Vector3 flockPositionMin = new Vector3();
		Vector3 flockPositionMax = new Vector3();
		int flockMates = 0;
		Vector3 flockVelocity = new Vector3();

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

					// Accumulate Velocities
					flockVelocity += boid.velocity;
					flockMates++;
				}
			}
		}

		List<Vector3> accelerations = new List<Vector3>();
		accelerations.AddRange(playerInfluence);
		accelerations.AddRange(avoidance);

		Vector3 flockCenter = new Vector3((flockPositionMin.x + flockPositionMax.x) / 2, (flockPositionMin.y + flockPositionMax.y) / 2, (flockPositionMin.z + flockPositionMax.z) / 2);
		if (flockMates > 0)
		{
			accelerations.Add((flockCenter - body.position).normalized * centeringStrength);
			accelerations.Add(((((flockVelocity / flockMates) * velocityMatchScale) - body.velocity) / timeToMatchVelocity) * body.mass);
		}

		if (drawFlock)
		{
			Destroy(cube);
			cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			flockCenter.y += 3;
			cube.transform.position = flockCenter;
			cube.transform.localScale = flockPositionMax - flockPositionMin;
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
