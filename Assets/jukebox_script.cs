using UnityEngine;
using System.Collections;

public class jukebox_script : MonoBehaviour {

	private static bool firstRun = true;

	// Use this for initialization
	void Start () {
		if (firstRun)
			firstRun = false;
		else
			Destroy(gameObject);
	}
}
