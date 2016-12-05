using UnityEngine;
using System.Collections.Generic;

public class PlayerBear : MonoBehaviour {

    public Rigidbody body;
    public bool terrorEnabled;

    [SerializeField]
    private float horizSpeed;

    [SerializeField]
    private float vertSpeed;

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody>();

        BoidController.bearBody = body;
        BoidController.bearPlayer = this;

        terrorEnabled = true;
    }

    void FixedUpdate()
    {
        body.velocity = new Vector3(Input.GetAxis("Horizontal") * horizSpeed, 0, Input.GetAxis("Vertical") * vertSpeed);
        body.rotation = Quaternion.LookRotation(body.velocity);

        //terrorEnabled = Input.GetAxis("Fire1") != 0;
    }
}
}
