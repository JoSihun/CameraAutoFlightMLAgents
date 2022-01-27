using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private GameObject Agent;
    private GameObject Target;

    // Start is called before the first frame update
    void Start()
    {
        Agent = GameObject.Find("Agent");
        Target = GameObject.Find("Target");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Agent.transform.forward.normalized * -10.0f + new Vector3(0.0f, 3.0f, 0.0f);
        transform.position = Agent.transform.position + direction;
        transform.LookAt(Agent.transform.position);
    }
}
