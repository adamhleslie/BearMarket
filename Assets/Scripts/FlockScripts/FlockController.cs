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
	private int updatesToWait;
	private int curUpdatesToWait;

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
	private bool matchVelocityBeforeCentering;

	[SerializeField]
	private Vector2 velocityClamp;

	// Player Boid Vars
	[SerializeField]
	private bool playerAttractionEnabled;

	[SerializeField]
	private float playerAttractionStrength;

	// Predator Vars
	[SerializeField]
	private bool predAvoidanceEnabled;

	[SerializeField]
	private float predAvoidanceRadius;

	[SerializeField]
	private float predAvoidanceStrength;

	// Collision Detection Vars
	[SerializeField]
	private bool collisionAvoidanceEnabled;

	[SerializeField]
	private int collisionLayer;

	[SerializeField]
	private float wallDetectDist;

	[SerializeField]
	private float wallAvoidanceStrength;

	// Awareness of all boids, players, obstacles, and predators
	private List<BoidInfo> boidList = new List<BoidInfo>();

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
			boidList.Add(new BoidInfo(body));

			numToSpawn--;
		}

		// Place player boids last
		foreach (FlockPlayerBoid player in boidPlayers)
		{
			boidList.Add(new BoidInfo(player.GetComponent<Rigidbody>()));
		} 

		// Use sqr distances for efficiency
		avoidanceRadius = avoidanceRadius * avoidanceRadius;
		visionRadius = visionRadius * visionRadius;
		predAvoidanceRadius = predAvoidanceRadius * predAvoidanceRadius;
		curUpdatesToWait = updatesToWait;

		// Update collision layer to be a valid mask
		collisionLayer = (collisionLayer > 0) ? (1 << collisionLayer) : 0;
	}

	void FixedUpdate ()
	{
		bool updatePlayersOnly = false;
		if (curUpdatesToWait > 0)
		{
			curUpdatesToWait--;

			// Ignore non-player changes, use the same forces from last update
			foreach (BoidInfo boid in boidList)
			{
				boid.ReapplyForces();
			}
			updatePlayersOnly = true;
		}
		else
		{
			curUpdatesToWait = updatesToWait;
		}

		// Assume player boids are last in boidList
		int lastActiveBoid = (updatePlayersOnly) ? (boidList.Count - boidPlayers.Length) : 0;
		for (int activeBoid = boidList.Count - 1; activeBoid >= lastActiveBoid; activeBoid--)
		{
			BoidInfo boid = boidList[activeBoid];

			// Clamp velocity to a valid magnitude
			boid.ClampVelocity(velocityClamp.x, velocityClamp.y);

			// Turn the boid to it's current velocity
			boid.UpdateRotation();

			// Step 1: Update spatial awareness (Automatically done)

			// Step 2: Generate force vectors

			// 2.1 Player Influences
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
			// 			boid.AddForce(displacement.normalized * (-1) * (predAvoidanceStrength / sqrDist));
			// 		}
			// 	}
			// }
			if (playerAttractionEnabled)
			{
				foreach (FlockPlayerBoid player in boidPlayers)
				{
					if (player.attractionEnabled)
					{
						Vector3 displacement = player.body.position - boid.body.position;
						float sqrDist = displacement.sqrMagnitude;
						if (sqrDist < visionRadius)
						{
							boid.AddForce(displacement.normalized * playerAttractionStrength);
						}
					}
				}
			}

			// 2.2 Collision Avoidance
			if (collisionAvoidanceEnabled)
			{
				RaycastHit hit;
				if (Physics.Raycast(boid.body.position, boid.body.velocity, out hit, wallDetectDist, collisionLayer)) 
				{
					Vector3 reflectDir = Vector3.Reflect(boid.body.velocity.normalized * (hit.distance), hit.normal) + hit.point;
					reflectDir -= boid.body.position;
					boid.AddForce(reflectDir.normalized * wallAvoidanceStrength);

					// If too close, manually change the velocity
					if (hit.distance < wallDetectDist / 5 && reflectDir != Vector3.zero) 
					{
						float scaleVec = reflectDir.magnitude / boid.body.velocity.magnitude;
						boid.body.velocity = reflectDir / scaleVec;
					}
				}
			}

			// 2.3 Flock Behavior
			for (int curFlockMate = 0; curFlockMate < activeBoid; curFlockMate++)
			{
				BoidInfo flockMate = boidList[curFlockMate];

				Vector3 displacement = flockMate.body.position - boid.body.position;
				float sqrDist = displacement.sqrMagnitude;
				if (sqrDist < visionRadius)
				{
					// Update awareness of flock
					boid.AddFlockMate(flockMate.body);
					flockMate.AddFlockMate(boid.body);

					// If close, move away
					if (sqrDist < avoidanceRadius)
					{
						Vector3 avoidanceVector = displacement.normalized * (avoidanceStrength / sqrDist);
						boid.AddForce(avoidanceVector * (-1));
						flockMate.AddForce(avoidanceVector);
					}
				}
			}

			if (matchVelocityBeforeCentering)
			{
				boid.GenerateVelocityMatching(velocityMatchScale, timeToMatchVelocity);
				boid.GenerateCentering(centeringStrength);
			}
			else
			{
				boid.GenerateCentering(centeringStrength);
				boid.GenerateVelocityMatching(velocityMatchScale, timeToMatchVelocity);
			}

			// Step 3: Accumulate and apply force vectors to boid
			boid.ApplyForces(forceAccumulationMax);
			boid.Reset();
		}
	}
}

public class BoidInfo
{
	private List<Vector3> forces = new List<Vector3>();
	private Vector3 accumulatedForces = new Vector3();
	private Vector3 flockVelocity = new Vector3();
	private Vector3 flockPositionMin = new Vector3();
	private Vector3 flockPositionMax = new Vector3();
	private int flockMates = 0;
	public Rigidbody body;

	public BoidInfo (Rigidbody boidBody)
	{
		body = boidBody;
	}

	public void Reset ()
	{
		forces.Clear();
		flockVelocity.Set(0, 0, 0);
		flockPositionMin.Set(0, 0, 0);
		flockPositionMax.Set(0, 0, 0);
		flockMates = 0;
	}

	public void AddForce (Vector3 force)
	{
		forces.Add(force);
	}

	public void AddFlockMate (Rigidbody flockMate)
	{
		// Update perception of flock for centering
		if (flockMates == 0)
		{
			flockPositionMin = flockMate.position;
			flockPositionMax = flockMate.position;
		}
		else
		{
			flockPositionMin = Vector3.Min(flockPositionMin, flockMate.position);
			flockPositionMax = Vector3.Max(flockPositionMax, flockMate.position);
		}

		// Accumulate velocity for velocity matching
		flockVelocity += flockMate.velocity;
		flockMates++;
	}

	public void GenerateCentering (float centeringStrength)
	{
		if (flockMates > 0)
		{
			Vector3 flockCenter = (flockPositionMin + flockPositionMax) / 2;
			AddForce((flockCenter - body.position).normalized * centeringStrength);
		}
	}

	public void GenerateVelocityMatching (float velocityMatchScale, float timeToMatchVelocity)
	{
		if (flockMates > 0)
			AddForce((((flockVelocity / flockMates) * velocityMatchScale) - body.velocity) / timeToMatchVelocity);
	}

	public void ApplyForces (float forceAccumulationMax)
	{
		accumulatedForces = AccumulateForces(forces, forceAccumulationMax);
		body.AddForce(accumulatedForces);
	}

	public void ReapplyForces ()
	{
		body.AddForce(accumulatedForces);
	}

	public void ClampVelocity (float min, float max)
	{
		float speed = body.velocity.magnitude;
		if (speed == 0)
		{
			body.velocity = new Vector3(min, 0, 0);
		}
		else if (speed < min)
		{
			float fac = min / speed;
			body.velocity *= fac;
		}
		else if (speed > max)
		{
			float fac = max / speed;
			body.velocity *= fac;
		}
	}

	public void UpdateRotation ()
	{
		body.rotation = Quaternion.LookRotation(body.velocity);
	}

	private Vector3 AccumulateForces (ICollection<Vector3> forces, float forceAccumulationMax)
	{
		float totalMagnitude = 0;
		Vector3 result = Vector3.zero;

		foreach (Vector3 f in forces)
		{
			totalMagnitude += f.magnitude;
			if (totalMagnitude >= forceAccumulationMax)
			{
				// Scale the force down when its entire magnitude not used
				float scale = 1 - ((totalMagnitude - forceAccumulationMax) / f.magnitude);
				result += scale * f;
				break;
			}
			else
				result += f;
		}

		return result;
	}
}
