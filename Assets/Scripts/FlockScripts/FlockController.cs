using UnityEngine;
using System.Collections.Generic;

// DO NOT DYNAMICALLY ADD BOIDS TO THE FLOCK CONTROLLER

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

	[SerializeField]
	private bool treatPlayerBoidAsBoid;

	[SerializeField]
	private FlockPlayerBoid[] initialBoidPlayers;

	// Awareness of all boids, players, obstacles, and predators
	private List<Boid> boidList = new List<Boid>();
	private List<FlockPlayerBoid> boidPlayers = new List<FlockPlayerBoid>();

	// public static List<PlayerPredator> predPlayers = new List<PlayerPredator>();

	// Spawn all boids
	void Start ()
	{
		// Create and add boids
		boidList.Capacity = (treatPlayerBoidAsBoid) ? (numToSpawn + initialBoidPlayers.Length) : (numToSpawn);
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

			// Set Boids initial velocity
			spawnedBoid.GetComponent<Rigidbody>().velocity = new Vector3(
					Random.Range(initialVelocityMin.x, initialVelocityMax.x),
					Random.Range(initialVelocityMin.y, initialVelocityMax.y),
					Random.Range(initialVelocityMin.z, initialVelocityMax.z));

			Boid boidComponent = spawnedBoid.AddComponent<Boid>() as Boid;
			AddBoid(boidComponent);

			numToSpawn--;
		}

		// Add players
		boidPlayers.Capacity = initialBoidPlayers.Length;
		foreach (FlockPlayerBoid player in initialBoidPlayers)
		{
			AddPlayerBoid(player);
		}

		// Use sqr distances for efficiency
		avoidanceRadius = avoidanceRadius * avoidanceRadius;
		visionRadius = visionRadius * visionRadius;
		predAvoidanceRadius = predAvoidanceRadius * predAvoidanceRadius;
		curUpdatesToWait = updatesToWait;

		// Update collision layer to be a valid mask
		collisionLayer = (collisionLayer > 0) ? (1 << collisionLayer) : 0;
	}

	public void AddBoid (Boid boid)
	{
		boid.flock = this;
		boidList.Insert(boidList.Count - boidPlayers.Count, boid);
	}

	public void RemoveBoid (Boid boid)
	{
		boid.flock = null;
		boidList.Remove(boid);
	}

	public void AddPlayerBoid (FlockPlayerBoid player)
	{
		// Add player boids last
		if (treatPlayerBoidAsBoid)
		{
			Boid playerBoid = player.gameObject.AddComponent<Boid>() as Boid;
			AddBoid(playerBoid);
		}

		player.flock = this;
		boidPlayers.Add(player);
	}

	public void RemovePlayerBoid (FlockPlayerBoid player)
	{
		player.flock = null;
		boidPlayers.Remove(player);
	}

	void FixedUpdate ()
	{
		bool updatePlayersOnly = false;
		if (curUpdatesToWait > 0)
		{
			curUpdatesToWait--;

			// Ignore non-player changes, use the same forces from last update
			foreach (Boid boid in boidList)
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
		int lastActiveBoid = (updatePlayersOnly) ? (boidList.Count - boidPlayers.Count) : 0;
		for (int activeBoid = boidList.Count - 1; activeBoid >= lastActiveBoid; activeBoid--)
		{
			Boid boid = boidList[activeBoid];

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
				if (Physics.SphereCast(boid.body.position, 5f, boid.body.velocity, out hit, wallDetectDist, collisionLayer))
				{
					Vector3 reflectDir = Vector3.Reflect(boid.body.velocity.normalized * hit.distance, hit.normal).normalized;
					reflectDir += boid.body.velocity.normalized;
					boid.AddForce(reflectDir.normalized * wallAvoidanceStrength);
				}
			}

			// 2.3 Flock Behaviour
			for (int curFlockMate = 0; curFlockMate < activeBoid; curFlockMate++)
			{
				Boid flockMate = boidList[curFlockMate];

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
			boid.ResetState();
		}
	}
}