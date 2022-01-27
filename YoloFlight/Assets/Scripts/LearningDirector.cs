using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LearningDirector : MonoBehaviour
{
    private int cntTotal;
    private int cntSuccess;
    private int cntFailed;
    private double accuracy;

    private Text totalText;
    private Text successText;
    private Text failedText;
    private Text accuracyText;


    // Start is called before the first frame update
    void Start()
    {
        cntTotal = 0;
        cntSuccess = 0;
        cntFailed = 0;
        accuracy = 0.0f;

        totalText = GameObject.Find("Total").GetComponent<Text>();
        successText = GameObject.Find("Success").GetComponent<Text>();
        failedText = GameObject.Find("Failed").GetComponent<Text>();
        accuracyText = GameObject.Find("Accuracy").GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        accuracy = (double)cntSuccess / cntTotal;

        totalText.text = "Total: " + cntTotal.ToString();
        successText.text = "Success: " + cntSuccess.ToString();
        failedText.text = "Failed:" + cntFailed.ToString();
        accuracyText.text = "Accuracy: " + accuracy.ToString("P");

    }

    public void IncreaseSuccess()
    {
        cntTotal += 1;
        cntSuccess += 1;
        PrintDebugLog();
    }
    public void IncreaseFailed()
    {
        cntTotal += 1;
        cntFailed += 1;
        PrintDebugLog();
    }
    private void PrintDebugLog()
    {
        accuracy = (double)cntSuccess / cntTotal;
        Debug.Log("Total Try = " + cntTotal + " | Success = " + cntSuccess + " | Failed = " + cntFailed + " | Accuracy = " + accuracy.ToString("P"));
    }
}
