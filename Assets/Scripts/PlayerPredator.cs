using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerPredator : MonoBehaviour {

  public Rigidbody body;

  [SerializeField]
    private float horizSpeed;

  [SerializeField]
    private float vertSpeed;

  [SerializeField]
    private float timeLeft;

  private int score = 0;
  public Text time;
  public Text scoreTxt;
  public Text endGameText;
  public Button reset;
  private bool gameOver = false;

  // Use this for initialization
  void Start ()
  {
    body = GetComponent<Rigidbody>();

    BoidController.predPlayers.Add(this);

    reset.gameObject.SetActive(false);
    endGameText.gameObject.SetActive(false);

    reset.onClick.AddListener(TaskOnClick);

    time.text = "Time: " + timeLeft;
    scoreTxt.text = "Boids Eaten: " + score;
  }

  void Update () {
    if (!gameOver) {
      timeLeft -= Time.deltaTime;
      time.text = "Time: " + timeLeft;
      if (timeLeft < 0) {
        time.text = "Time: 0";
        reset.gameObject.SetActive(true);
        endGameText.gameObject.SetActive(true);
        GameOver();
      }
    }
  }

  void FixedUpdate ()
  {
    if (!gameOver) {
      body.velocity = new Vector3(Input.GetAxis("Horizontal") * horizSpeed,
          0, Input.GetAxis("Vertical") * vertSpeed);
      body.rotation = Quaternion.LookRotation(body.velocity);
    }
  }

  void OnTriggerEnter(Collider other) {
    if (!gameOver && other.gameObject.layer != 9) {
      Destroy(other.gameObject);
      score++;
      scoreTxt.text = "Boids Eaten: " + score;
    }
  }

  void GameOver() {
    //
    gameOver = true;
  }

  void TaskOnClick () {
    SceneManager.LoadScene("Main Menu");
  }
}
