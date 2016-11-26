using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InstantSpawnPoint : MonoBehaviour {

  // Set in the inspector as the thing that will be spawned
  public GameObject blueprint;

  public int numToSpawn;

  // All set in editor (each should have a size of 2)
  public float[] scaleRange;
  public float[] xRange;
  public float[] yRange;
  public float[] zRange;

  // Use this for initialization
  private void Start ()
  {

    Assert.IsNotNull(blueprint);
    blueprint.SetActive(false);
  }

  // Update is called once per frame
  private void Update ()
  {
    while (numToSpawn > 0)
    {
      // Instantiate Template
      GameObject spawnedObject = Instantiate(blueprint);
      Transform spawnedTransf = spawnedObject.GetComponent<Transform>();
      spawnedTransf.SetParent(GetComponent<Transform>(), false);

      float scale = Random.Range(scaleRange[0], scaleRange[1]);
      spawnedTransf.localScale = new Vector3(scale, scale, scale);
      spawnedTransf.localPosition = new Vector3(
          Random.Range(xRange[0], xRange[1]),
          Random.Range(yRange[0], yRange[1]),
          Random.Range(zRange[0], zRange[1]));
      spawnedObject.SetActive(true);
      spawnedTransf.SetParent(null);

      numToSpawn--;
    }
    Destroy(gameObject);
  }
}
