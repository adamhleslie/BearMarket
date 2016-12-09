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

	public string horizAxis;
	public string vertAxis;
	public string attractButton;

	// Use this for initialization
	void Awake () 
	{
		body = GetComponent<Rigidbody>();

		attractionEnabled = false;
	}

	void OnDestroy ()
	{
		if (flock != null)
		{
			flock.NotifyPlayerDead();
			flock.RemovePlayerBoid(this);
		}
	}
	
	void FixedUpdate () 
	{
		body.AddForce(new Vector3(Input.GetAxis(horizAxis) * horizForceMax, 0, Input.GetAxis(vertAxis) * vertForceMax));
		body.rotation = Quaternion.LookRotation(body.velocity);

		attractionEnabled = Input.GetAxis(attractButton) != 0;
	}
}
