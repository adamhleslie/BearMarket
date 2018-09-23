using UnityEngine;
using System.Collections;

public class jukebox_script : MonoBehaviour 
{
	[System.NonSerialized]
	public int colMultiBoidWins;

	[System.NonSerialized]
	public int colMultiBearWins;

	[System.NonSerialized]
	public 

	[System.NonSerialized]
	public 

	private static bool firstRun = true;

	void Awake ()
	{
		if (firstRun)
			firstRun = false;
		else
			Destroy(gameObject);
	}
}
