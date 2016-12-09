using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerPredator : MonoBehaviour 
{
	public Rigidbody body;

	[SerializeField]
	private float horizSpeed;

	[SerializeField]
	private float vertSpeed;

	[SerializeField]
	private float timeLeft;
	private float trueTimeLeft;

	public float maxVelocity;

	private int score = 0;
	public Text time;
	public Text scoreTxt;
	public Text endGameText;
	public Button reset;
	private bool gameOver = false;

	public string timeText;
	public string boidText;
	public string overText;

	public string horizAxis;
	public string vertAxis;

	[System.NonSerialized]
	public bool playerKilled;
	public bool multi;

	// Use this for initialization
	void Start ()
	{
		body = GetComponent<Rigidbody>();

		BoidController.predPlayers.Add(this);

		reset.gameObject.SetActive(false);
		endGameText.gameObject.SetActive(false);

		reset.onClick.AddListener(TaskOnClick);

		trueTimeLeft = timeLeft;

		time.text = (timeText + Mathf.Ceil(trueTimeLeft));
		if (!multi)
		{
			scoreTxt.text = boidText + score;
		}
		else
			scoreTxt.text = "";
	}

	void Update () 
	{
		if (playerKilled)
		{
			reset.gameObject.SetActive(true);
			endGameText.gameObject.SetActive(true);
			endGameText.text = "THE BEAR WON in " + ((timeLeft - trueTimeLeft) - ((timeLeft - trueTimeLeft) % .1)) + "s";
			GameOver();
		}
		body.rotation = Quaternion.LookRotation(body.velocity);
		if (!gameOver) 
		{
			trueTimeLeft -= Time.deltaTime;
			time.text = (timeText + Mathf.Ceil(trueTimeLeft));
			if (trueTimeLeft < 0) 
			{
				time.text = timeText + "0";
				reset.gameObject.SetActive(true);
				endGameText.gameObject.SetActive(true);
				if (!multi)
					endGameText.text = overText + score + " Birdnessmen in " + (timeLeft) + "s";
				else
					endGameText.text = "THE BIRD WON!";
				GameOver();
			}
			else if (trueTimeLeft <= 10)
			{
				time.text = timeText + (trueTimeLeft  - (trueTimeLeft % .1));
			}
		}
	}

	void FixedUpdate ()
	{
		if (!gameOver) {
			body.AddForce(new Vector3(Input.GetAxis(horizAxis) * horizSpeed,
					0, Input.GetAxis(vertAxis) * vertSpeed));

			float speed = body.velocity.magnitude;
			if (speed > maxVelocity)
			{
				float fac = maxVelocity / speed;
				body.velocity *= fac;
			}
		}
	}

	void OnCollisionEnter(Collision other) 
	{
		if (!gameOver && other.gameObject.layer != 9) {
			Destroy(other.gameObject);
			score++;
			if (!multi)
				scoreTxt.text = boidText + score;

		}
	}

	void GameOver() 
	{
		//
		gameOver = true;

	}

	void TaskOnClick () 
	{
		SceneManager.LoadScene("Main Menu");
	}
}
