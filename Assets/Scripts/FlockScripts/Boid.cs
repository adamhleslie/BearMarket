using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
	private List<Vector3> forces = new List<Vector3>();
	private Vector3 accumulatedForces = new Vector3();
	private Vector3 flockVelocity = new Vector3();
	private Vector3 flockPositionMin = new Vector3();
	private Vector3 flockPositionMax = new Vector3();
	private int flockMates = 0;

	[System.NonSerialized]
	public Rigidbody body;

	[System.NonSerialized]
	public FlockController flock;

	void Awake ()
	{
		body = GetComponent<Rigidbody>();
	}

	void OnDelete ()
	{
		if (flock != null)
			flock.RemoveBoid(this);
	}

	public void ResetState ()
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
