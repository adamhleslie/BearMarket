using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InstantSpawnPoint : MonoBehaviour {

	// Set in the inspector as the thing that will be spawned
	public GameObject blueprint;

	public int numToSpawn;

	// All set in editor (each should have a size of 2)
	public float[] scaleRange;
	public float[] xRange;
	public float[] yRange;
	public float[] zRange;

  //Set in editor, determines potential speed range
	public float initialVelocityMin;
	public float initialVelocityMax;

	//set whether boids will fly in 2D or 3D space
	public bool is3D;

	private float initialYMinVelocity;
	private float initialYMaxVelocity;



	// Use this for initialization
	private void Start ()
	{
		if (is3D){
			initialYMinVelocity = initialVelocityMin;
			initialYMaxVelocity = initialVelocityMax;
		}
		else{
			initialYMinVelocity = 0;
			initialYMaxVelocity = 0;
		}
		Assert.IsNotNull(blueprint);
		blueprint.SetActive(false);
	}

	// Update is called once per frame
	private void Update ()
	{
		Rigidbody body;
		while (numToSpawn > 0)
		{
			// Instantiate Template
			GameObject spawnedObject = Instantiate(blueprint);
			Transform spawnedTransf = spawnedObject.GetComponent<Transform>();
			spawnedTransf.SetParent(GetComponent<Transform>(), false);

			//randomize inition velocity
			body = spawnedObject.GetComponent<Rigidbody>();
			body.velocity = new Vector3(Random.Range(initialVelocityMin, initialVelocityMax),
					Random.Range(initialYMinVelocity, initialYMaxVelocity),
					Random.Range(initialVelocityMin, initialVelocityMax));
			body.rotation = Quaternion.LookRotation(body.velocity.normalized);


			float scale = Random.Range(scaleRange[0], scaleRange[1]);
			spawnedTransf.localScale = new Vector3(scale, scale, scale);
			spawnedTransf.localPosition = new Vector3(
					Random.Range(xRange[0], xRange[1]),
					Random.Range(yRange[0], yRange[1]),
					Random.Range(zRange[0], zRange[1]));
			spawnedObject.SetActive(true);
			spawnedTransf.SetParent(null);

			numToSpawn--;
		}
		Destroy(gameObject);
	}
}
