using UnityEngine;
using System.Collections;

public class ShipMover : MonoBehaviour
{
    public float fowardSpeed = 5;
    public float turnSpeed = 0.2f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * fowardSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, turnSpeed);
    }
}
