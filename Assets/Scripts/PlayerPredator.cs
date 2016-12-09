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

  public float maxVelocity;

  private int score = 0;
  public Text time;
  public Text scoreTxt;
  public Text endGameText;
  public Button reset;
  private bool gameOver = false;

  public string timeText;
  public string boidText;

  // Use this for initialization
  void Start ()
  {
    body = GetComponent<Rigidbody>();

    BoidController.predPlayers.Add(this);

    reset.gameObject.SetActive(false);
    endGameText.gameObject.SetActive(false);

    reset.onClick.AddListener(TaskOnClick);

    time.text = timeText + (timeLeft  - (timeLeft % 1));
    scoreTxt.text = boidText + score;
  }

  void Update () {
    body.rotation = Quaternion.LookRotation(body.velocity);
    if (!gameOver) {
      timeLeft -= Time.deltaTime;
      time.text = timeText + (timeLeft  - (timeLeft % 1));
      if (timeLeft < 0) {
        time.text = timeText + "0";
        reset.gameObject.SetActive(true);
        endGameText.gameObject.SetActive(true);
        GameOver();
      }
      else if (timeLeft <= 10)
      {
        time.text = timeText + (timeLeft  - (timeLeft % .1));
      }
    }
  }

  void FixedUpdate ()
  {
    if (!gameOver) {
      body.AddForce(new Vector3(Input.GetAxis("Horizontal") * horizSpeed,
          0, Input.GetAxis("Vertical") * vertSpeed));

      float speed = body.velocity.magnitude;
      if (speed > maxVelocity)
      {
        float fac = maxVelocity / speed;
        body.velocity *= fac;
      }
    }
  }

  void OnCollisionEnter(Collision other) {
    if (!gameOver && other.gameObject.layer != 9) {
      Destroy(other.gameObject);
      score++;
      scoreTxt.text = boidText + score;
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
