using UnityEngine;
using System.Collections.Generic;
using System;

public class BoidController : MonoBehaviour
{
  private Rigidbody body;

  // instantaneous velocity max
  // instantaneous force max

  [SerializeField]
    private Vector3 initialVelocity;

  [SerializeField]
    private float forceAccumulationMax;

  [SerializeField]
    private float avoidanceRadius;

  [SerializeField]
    private float avoidanceStrength;

  [SerializeField]
    private float centeringRadius;

  [SerializeField]
    private float centeringStrength;

  // Need to store local awareness of
  // 1. Boids
  private static List<Rigidbody> boidList = null;
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
      boidList = new List<Rigidbody>();
      avoidanceRadius = avoidanceRadius * avoidanceRadius;
      centeringRadius = centeringRadius * centeringRadius;
      Debug.Log("Sqr Avoidance radius = " + avoidanceRadius);
      Debug.Log("Sqr Centering radius = " + centeringRadius);
    }

    // Add to boidList
    boidList.Add(body);
  }

  void FixedUpdate ()
  {
    // Step 1: Update spatial awareness (Automatically)

    // Step 2: Generate acceleration vectors
    List<Vector3> avoidance = new List<Vector3>();
    bool flockFound = false;
    Vector3 flockPositionMin = new Vector3();
    Vector3 flockPositionMax = new Vector3();

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
        if (sqrDist < centeringRadius)
        {
          // Update awareness of flock
          if (!flockFound)
          {
            flockPositionMin = boid.position;
            flockPositionMax = boid.position;
            flockFound = true;
          }
          else
          {
            flockPositionMin = Vector3.Min(flockPositionMin, boid.position);
            flockPositionMax = Vector3.Max(flockPositionMax, boid.position);
          }
        }
      }
    }

    List<Vector3> accelerations = new List<Vector3>();
    accelerations.AddRange(avoidance);

    Vector3 flockCenter = new Vector3((flockPositionMin.x + flockPositionMax.x) / 2, 0, (flockPositionMin.z + flockPositionMax.z) / 2);
    if (flockFound)
      accelerations.Add((flockCenter - body.position).normalized * centeringStrength);

    // Step 3: Accumulate acceleration vectors
    // sum each list into its own combined vector representing different forces acting on the boid, adding each to the list of vectors to be accumulated
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
