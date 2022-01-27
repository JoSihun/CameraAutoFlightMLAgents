using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestDirector : MonoBehaviour
{
    private Transform tfAgent;
    private Rigidbody rbAgent;

    private Transform tfTarget;
    private Rigidbody rbTarget;

    private int cntTotal;
    private int cntSuccess;
    private int cntFailed;

    private double time;
    private double velocity;
    private double distance;
    private double accuracy;

    private double avgTime;

    private Text totalText;
    private Text successText;
    private Text failedText;

    private Text timeText;
    private Text velocityText;
    private Text distanceText;
    private Text accuracyText;


    // Start is called before the first frame update
    void Start()
    {
        //cntTotal = 100;
        //cntSuccess = 89;
        //cntFailed = 11;
        cntTotal = 0;
        cntSuccess = 0;
        cntFailed = 0;

        time = 0.0f;
        velocity = 0.0f;
        distance = 0.0f;
        accuracy = 0.0f;

        //avgTime = 2148.46f;
        avgTime = 0.0f;

        totalText = GameObject.Find("Total").GetComponent<Text>();
        successText = GameObject.Find("Success").GetComponent<Text>();
        failedText = GameObject.Find("Failed").GetComponent<Text>();

        timeText = GameObject.Find("Time").GetComponent<Text>();
        velocityText = GameObject.Find("Velocity").GetComponent<Text>();
        distanceText = GameObject.Find("Distance").GetComponent<Text>();
        accuracyText = GameObject.Find("Accuracy").GetComponent<Text>();

        tfAgent = GameObject.Find("Agent").GetComponent<Transform>();
        rbAgent = GameObject.Find("Agent").GetComponent<Rigidbody>();

        tfTarget = GameObject.Find("Target").GetComponent<Transform>();
        rbTarget = GameObject.Find("Target").GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        velocity = rbAgent.velocity.magnitude;
        distance = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);
        accuracy = (double)cntSuccess / cntTotal;

        totalText.text = "Total: " + cntTotal.ToString();
        successText.text = "Success: " + cntSuccess.ToString();
        failedText.text = "Failed:" + cntFailed.ToString();

        timeText.text = "Time: " + (avgTime / cntSuccess).ToString("F") + "s";
        //timeText.text = "Time: " + time.ToString("F") + "s";
        velocityText.text = "Velocity: " + velocity.ToString("F") + "m/s";
        distanceText.text = "Distance: " + distance.ToString("F") + "m";
        accuracyText.text = "Accuracy: " + accuracy.ToString("P");

    }

    public void IncreaseSuccess()
    {
        avgTime += time;
        time = 0.0f;
        cntTotal += 1;
        cntSuccess += 1;
        //PrintDebugLog();
    }
    public void IncreaseFailed()
    {
        time = 0.0f;
        cntTotal += 1;
        cntFailed += 1;
        //PrintDebugLog();
    }
    private void PrintDebugLog()
    {
        accuracy = (double)cntSuccess / cntTotal;
        Debug.Log("Total Try = " + cntTotal + " | Success = " + cntSuccess + " | Failed = " + cntFailed + " | Accuracy = " + accuracy.ToString("P"));
    }
}
