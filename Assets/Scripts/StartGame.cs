using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {

	public Button startGame;
	// Use this for initialization
	void Start () {
		Button btn = startGame.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}
	
	void TaskOnClick () {
		SceneManager.LoadScene ("FlockingColiseum");
	}
}
