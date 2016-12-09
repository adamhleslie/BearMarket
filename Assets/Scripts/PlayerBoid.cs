using UnityEngine;
using System.Collections.Generic;

public class PlayerBoid : MonoBehaviour 
{
	public Rigidbody body;
	public bool attractionEnabled;

	[SerializeField]
	private float horizSpeed;

	[SerializeField]
	private float vertSpeed;

	// Use this for initialization
	void Start () 
	{
		body = GetComponent<Rigidbody>();

		BoidController.boidPlayers.Add(this);
		BoidController.boidList.Add(body);

		attractionEnabled = false;
	}
	
	void FixedUpdate () 
	{
		body.velocity = new Vector3(Input.GetAxis("Horizontal") * horizSpeed, 0, Input.GetAxis("Vertical") * vertSpeed);
		body.rotation = Quaternion.LookRotation(body.velocity);

		attractionEnabled = Input.GetAxis("Fire1") != 0;
	}
}
