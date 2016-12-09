using UnityEngine;
using System.Collections.Generic;

public class PlayerBoid : MonoBehaviour
{
	[SerializeField]
	private float horizForceMax;

	[SerializeField]
	private float vertForceMax;

	[System.NonSerialized]
	public bool attractionEnabled;

	[System.NonSerialized]
	public Flock flock;

	[System.NonSerialized]
	public Rigidbody body;

	// Use this for initialization
	void Awake () 
	{
		body = GetComponent<Rigidbody>();

		attractionEnabled = false;
	}

	void OnDelete ()
	{
		if (flock != null)
			flock.RemovePlayerBoid(this);
	}
	
	void FixedUpdate () 
	{
		body.AddForce(new Vector3(Input.GetAxis("Horizontal") * horizForceMax, 0, Input.GetAxis("Vertical") * vertForceMax));
		body.rotation = Quaternion.LookRotation(body.velocity);

		attractionEnabled = Input.GetAxis("Fire1") != 0;
	}
}
