using UnityEngine;
using System.Collections.Generic;

public class FlockPlayerBoid : MonoBehaviour 
{
	public Rigidbody body;
	public bool attractionEnabled;

	[SerializeField]
	private float horizForceMax;

	[SerializeField]
	private float vertForceMax;

	// Use this for initialization
	void Start () 
	{
		body = GetComponent<Rigidbody>();

		attractionEnabled = false;
	}
	
	void FixedUpdate () 
	{
		body.AddForce(new Vector3(Input.GetAxis("Horizontal") * horizForceMax, 0, Input.GetAxis("Vertical") * vertForceMax));
		body.rotation = Quaternion.LookRotation(body.velocity);

		attractionEnabled = Input.GetAxis("Fire1") != 0;
	}
}
