using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitoringUI : MonoBehaviour
{
    public int totalCount;
    public int successCount;
    public int failedCount;
    public double distance;
    public double accuracy;

    public float time;              // 시간
    public float velocity;          // 속력
    public float moveDistance;      // 이동거리

    private Text totalText;
    private Text successText;
    private Text failedText;
    private Text distanceText;
    private Text accuracyText;

    private Text timeText;
    private Text velocityText;
    private Text moveDistanceText;

    // Start is called before the first frame update
    void Start()
    {
        totalCount = 0;
        successCount = 0;
        failedCount = 0;
        distance = 0.0f;
        accuracy = 0.0f;

        time = 0.0f;
        velocity = 0.0f;
        moveDistance = 0.0f;

        totalText = GameObject.Find("TotalCount").GetComponent<Text>();
        successText = GameObject.Find("SuccessCount").GetComponent<Text>();
        failedText = GameObject.Find("FailedCount").GetComponent<Text>();
        distanceText = GameObject.Find("Distance").GetComponent<Text>();
        accuracyText = GameObject.Find("Accuracy").GetComponent<Text>();

        timeText = GameObject.Find("Time").GetComponent<Text>();
        velocityText = GameObject.Find("Velocity").GetComponent<Text>();
        moveDistanceText = GameObject.Find("MoveDistance").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        totalText.text = "Total Try: " + totalCount.ToString();
        successText.text = "Success: " + successCount.ToString();
        failedText.text = "Failed:" + failedCount.ToString();

        accuracy = (double)successCount / totalCount;
        distanceText.text = "TargetDistance: " + distance.ToString("F") + "m";
        accuracyText.text = "Accuracy: " + accuracy.ToString("P");

        time += Time.deltaTime;
        velocity = moveDistance / time;

        timeText.text = "Time: " + time.ToString("F") + "s";
        velocityText.text = "Velocity: " + velocity.ToString("F") + "m/s";
        moveDistanceText.text = "Moved Distance: " + moveDistance.ToString("F") + "m";
    }
}
