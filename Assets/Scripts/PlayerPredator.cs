using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

  // Use this for initialization
  void Start ()
  {
    body = GetComponent<Rigidbody>();

    BoidController.predPlayers.Add(this);

    time.text = "Time: " + timeLeft;
    scoreTxt.text = "Score: " + score;
  }

  void Update () {
    timeLeft -= Time.deltaTime;
    time.text = "Time: " + timeLeft;
    if (timeLeft < 0) {
      GameOver();
    }
  }

  void FixedUpdate ()
  {
    body.velocity = new Vector3(Input.GetAxis("Horizontal") * horizSpeed, 0, Input.GetAxis("Vertical") * vertSpeed);
    body.rotation = Quaternion.LookRotation(body.velocity);
  }

  void OnTriggerEnter(Collider other) {
    if (other.gameObject.layer != 9) {
      Destroy(other.gameObject);
      score++;
      scoreTxt.text = "Score: " + score;
    }
  }

  void GameOver() {
  }

}
