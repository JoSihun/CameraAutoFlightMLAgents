using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Rigidbody motorFR;
    private Rigidbody motorFL;
    private Rigidbody motorRR;
    private Rigidbody motorRL;

    private float addForce = 1000.0f;

    // Start is called before the first frame update
    void Start()
    {
        motorFR = transform.Find("Fans").Find("fan.002").GetComponent<Rigidbody>();
        motorFL = transform.Find("Fans").Find("fan.004").GetComponent<Rigidbody>();
        motorRR = transform.Find("Fans").Find("fan.003").GetComponent<Rigidbody>();
        motorRL = transform.Find("Fans").Find("fan.001").GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKey(KeyCode.W)) {
            motorFR.AddRelativeForce(Vector3.up * addForce);
            motorFL.AddRelativeForce(Vector3.up * addForce);
            motorRR.AddRelativeForce(Vector3.up * addForce);
            motorRL.AddRelativeForce(Vector3.up * addForce);
        }
        

    }
}
