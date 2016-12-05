using UnityEngine;
using System.Collections.Generic;

public class PlayerPredator : MonoBehaviour {

	public Rigidbody body;

	[SerializeField]
	private float horizSpeed;

	[SerializeField]
	private float vertSpeed;

	// Use this for initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();

		BoidController.predPlayers.Add(this);
	}

	void FixedUpdate ()
	{
		body.velocity = new Vector3(Input.GetAxis("Horizontal") * horizSpeed, 0, Input.GetAxis("Vertical") * vertSpeed);
		body.rotation = Quaternion.LookRotation(body.velocity);
	}
}
