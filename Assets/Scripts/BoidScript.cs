using UnityEngine;
using System.Collections;

public class BoidScript : MonoBehaviour {

  public static int[][] boidArr;
  private int id;
  public

  // Use this for initialization
  void Start () {
    if (boidArr == null) {
      boidArr = new int[][];
    }
    id = new Random().Next(1,10000);
  }

  // Update is called once per frame
  void Update () {

  }

}
