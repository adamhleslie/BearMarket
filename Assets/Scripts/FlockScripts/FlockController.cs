using UnityEngine;
using System.Collections.Generic;

public class FlockController : MonoBehaviour
{
	// Boid Spawn Vars
	[SerializeField]
	private GameObject boid;

	[SerializeField]
	private int numToSpawn;

	[SerializeField]
	private Vector3 spawnRangeMin;

	[SerializeField]
	private Vector3 spawnRangeMax;

	[SerializeField]
	private Vector3 initialVelocityMin;

	[SerializeField]
	private Vector3 initialVelocityMax;

	// Boid Flocking Vars
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
	private Vector2 velocityClamp;

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

	// Awareness of all boids, obstacles, and predators
	private List<Rigidbody> boidList = new List<Rigidbody>();

	[SerializeField]
	private FlockPlayerBoid[] boidPlayers;

	// public static List<PlayerBoid> boidPlayers = new List<PlayerBoid>();
	// public static List<PlayerPredator> predPlayers = new List<PlayerPredator>();

	// Spawn all boids
	void Start ()
	{
		while (numToSpawn > 0)
		{
			// Instantiate Boid
			GameObject spawnedBoid = Instantiate(boid);
			spawnedBoid.SetActive(true);

			// Place Boid at random place
			Transform transf = spawnedBoid.transform;
			transf.SetParent(transform, false);
			transf.localPosition = new Vector3(
					Random.Range(spawnRangeMin.x, spawnRangeMax.x),
					Random.Range(spawnRangeMin.y, spawnRangeMax.y),
					Random.Range(spawnRangeMin.z, spawnRangeMax.z));

			Rigidbody body = spawnedBoid.GetComponent<Rigidbody>();
			body.velocity = new Vector3(Random.Range(initialVelocityMin.x, initialVelocityMax.x),
										Random.Range(initialVelocityMin.y, initialVelocityMax.y),
										Random.Range(initialVelocityMin.z, initialVelocityMax.z));
			boidList.Add(body);

			numToSpawn--;
		}

		foreach (FlockPlayerBoid player in boidPlayers)
		{
			boidList.Add(player.GetComponent<Rigidbody>());
		} 

		// Use sqr distances for efficiency
		avoidanceRadius = avoidanceRadius * avoidanceRadius;
		visionRadius = visionRadius * visionRadius;
		predAvoidanceRadius = predAvoidanceRadius * predAvoidanceRadius;
	}

	void FixedUpdate ()
	{
		foreach (Rigidbody body in boidList)
		{
			// Clamp velocity to a valid magnitude
			float speed = body.velocity.magnitude;
			if (speed == 0)
			{
				body.velocity = new Vector3(velocityClamp.x, 0, 0);
			}
			else if (speed < velocityClamp.x)
			{
				float fac = velocityClamp.x / speed;
				body.velocity *= fac;
			}
			else if (speed > velocityClamp.y)
			{
				float fac = velocityClamp.y / speed;
				body.velocity *= fac;
			}

			// Turn the boid to it's current velocity
			body.rotation = Quaternion.LookRotation(body.velocity);

			// Step 1: Update spatial awareness (Automatically done)

			// Step 2: Generate acceleration vectors
			List<Vector3> playerInfluence = new List<Vector3>();
			// if (predAvoidanceEnabled)
			// {
			// 	// Fear the Pred
			// 	foreach (PlayerPredator pred in predPlayers)
			// 	{
			// 		Vector3 displacement = pred.body.position - body.position;
			// 		float sqrDist = displacement.sqrMagnitude;
			// 		if (sqrDist < predAvoidanceRadius)
			// 		{
			// 			// If close, flee in terror
			// 			playerInfluence.Add(displacement.normalized * (-1) * (predAvoidanceStrength / sqrDist));
			// 		}
			// 	}
			// }
			if (playerAttractionEnabled)
			{
				foreach (FlockPlayerBoid player in boidPlayers)
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

			// Step 3: Accumulate acceleration vectors
			Vector3 output = AccumulateAccelerations(accelerations);

			// Step 4: Apply acceleration vectors to boid
			body.AddForce(output);
		}
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
