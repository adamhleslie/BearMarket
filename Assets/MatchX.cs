using UnityEngine;
using System.Collections;

public class MatchX : MonoBehaviour {

	public Transform watchMe;
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(watchMe.position.x, transform.position.y, transform.position.z);
	}
}
