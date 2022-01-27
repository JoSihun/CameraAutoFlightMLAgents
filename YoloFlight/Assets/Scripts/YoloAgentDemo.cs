using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class YoloAgentDemo : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;
    private Renderer renderTarget;

    private float addForce = 25.0f;
    private float rotSpeed = 25.0f;

    private float distAfter;
    private float distBefore;
    
    public override void Initialize()
    {
        MaxStep = 1000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();
        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();

    }

    public override void OnEpisodeBegin()
    {
        // Agent ������ �ʱ�ȭ
        tfAgent.Rotate(0, 0, 0);
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;

        // Agent & Target & Obstacle �ʱ���ġ ������ ����
        tfAgent.localPosition = new Vector3(Random.Range(-500.0f, 500.0f), Random.Range(10.0f, 500.0f), Random.Range(-500.0f, 500.0f));
        tfTarget.localPosition = new Vector3(Random.Range(-500.0f, 500.0f), Random.Range(10.0f, 500.0f), Random.Range(-500.0f, 500.0f));
        foreach (Transform child in GameObject.Find("Obstacles").transform)
            child.localPosition = new Vector3(Random.Range(-500.0f, 500.0f), Random.Range(10.0f, 500.0f), Random.Range(-500.0f, 500.0f));

        // ���� ����� ���� Agent�� Target�� �ʱ�Ÿ�
        distBefore = (tfAgent.localPosition - tfTarget.localPosition).magnitude;

        // Material �÷� �ʱ�ȭ, ����Ȯ���� ���� �ڷ�ƾ ������
        StartCoroutine(RevertMaterial());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(tfAgent.localPosition - tfTarget.localPosition);      // ������ 3��, Agent�κ��� Target������ �Ÿ����⺤��(x, y, z) 3��

        sensor.AddObservation(tfTarget.localPosition);                              // ������ 3��, Target ������ǥ��(x, y, z) 3�� 
        sensor.AddObservation(tfAgent.localPosition);                               // ������ 3��, Agent ������ǥ��(x, y, z) 3�� 
        sensor.AddObservation(tfAgent.eulerAngles.y);                               // ������ 1��, Agent ���Ϸ�����(y) 1�� 

        sensor.AddObservation(rbAgent.velocity.x);                                  // ������ 1��, Agent x�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.y);                                  // ������ 1��, Agent y�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.z);                                  // ������ 1��, Agent z�� ���� ������ 1��
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float upDown = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float backForth = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float leftRight = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);
        float rotationY = Mathf.Clamp(actions.ContinuousActions[3], -1.0f, 1.0f);

        Vector3 direction = Vector3.up * upDown + Vector3.forward * backForth + Vector3.right * leftRight;
        Vector3 rotation = new Vector3(0, rotationY, 0);
        rbAgent.AddForce(direction.normalized * addForce);
        tfAgent.Rotate(rotation.normalized * rotSpeed * Time.deltaTime);

        // �����۾��ʿ�
        // �󺧿� ���� ����/�г�Ƽ ��� ����
        // RectSize�� ���� ����/�г�Ƽ ����
        string[] labels_high = { "Person", "Bus", "Car", "Motorbike", "Bird", "Cat", "Dog", "Train", "Bicycle" };
        string[] labels_middle = { "Plane", "Table", "Chair", "Sofa", "TV", "Bottle", "Plant" };
        string[] labels_low = { "Boat", "Cow", "Horse", "Sheep" };
        var list_labels_high = new List<string>();
        var list_labels_middle = new List<string>();
        var list_labels_low = new List<string>();
        list_labels_high.AddRange(labels_high);
        list_labels_middle.AddRange(labels_middle);
        list_labels_low.AddRange(labels_low);

        foreach (Transform child in GameObject.Find("Result1").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;
                
                AddReward(penalty);
                Debug.Log("UpCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
                
        }
        foreach (Transform child in GameObject.Find("Result2").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;

                AddReward(penalty);
                Debug.Log("DownCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }
        foreach (Transform child in GameObject.Find("Result3").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;

                AddReward(penalty);
                Debug.Log("FrontCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }
        foreach (Transform child in GameObject.Find("Result4").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;

                AddReward(penalty);
                Debug.Log("BackCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }
        foreach (Transform child in GameObject.Find("Result5").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;

                AddReward(penalty);
                Debug.Log("LeftCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }
        foreach (Transform child in GameObject.Find("Result6").transform)
        {
            if (child.gameObject.activeSelf)
            {
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float areaThreshold = 100.0f;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;
                float areaWeight = Mathf.Clamp(width * height, 0, areaThreshold * areaThreshold) / (areaThreshold * areaThreshold);
                float penalty = 0.0f;

                string[] label = text.Split(' ');
                if (list_labels_high.Contains(label[0]))
                    penalty = -2.0f * percent * areaWeight;
                else if (list_labels_middle.Contains(label[0]))
                    penalty = -1.5f * percent * areaWeight;
                else if (list_labels_low.Contains(label[0]))
                    penalty = -1.0f * percent * areaWeight;

                AddReward(penalty);
                Debug.Log("RightCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }

        // ����ó��
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;         // �ൿ������ Agent, Target�� �Ÿ�
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;     // �ൿ������ UI �Ÿ�ǥ�� ����

        AddReward((distBefore - distAfter) * 10.0f);      // ����Ÿ��� �����Ÿ����� ª���� +����, �ָ� -�г�Ƽ
        distBefore = distAfter;                 // �����ൿ�� ���� �����Ÿ� ����
        AddReward(-0.01f);                      // �������� �ൿ������ ���� �г�Ƽ

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ContinousActionsOut = actionsOut.ContinuousActions;
        // Heuristic User Control Up Down
        if (Input.GetKey(KeyCode.W))
            ContinousActionsOut[0] = 1.0f;
        if (Input.GetKey(KeyCode.S))
            ContinousActionsOut[0] = -1.0f;

        // Heuristic User Control Back Forth
        if (Input.GetKey(KeyCode.UpArrow))
            ContinousActionsOut[1] = 1.0f;
        if (Input.GetKey(KeyCode.DownArrow))
            ContinousActionsOut[1] = -1.0f;

        // Heuristic User Control Left Right
        if (Input.GetKey(KeyCode.D))
            ContinousActionsOut[2] = 1.0f;
        if (Input.GetKey(KeyCode.A))
            ContinousActionsOut[2] = -1.0f;

        // Heuristic User Control Rotate Left Right
        if (Input.GetKey(KeyCode.Keypad4))
            ContinousActionsOut[3] = -1.0f;
        if (Input.GetKey(KeyCode.Keypad6))
            ContinousActionsOut[3] = 1.0f;


    }


    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Collider �浹 �� �г�Ƽ�ο�, EndEpisode O
        Debug.Log("Collision Occured!!! Collision Name = " + collision.collider.name);
        if (collision.collider.name.Equals("Target"))
        {
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount += 1;
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().successCount += 1;

            AddReward(10.0f);
            EndEpisode();
        }
        else
        {
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount += 1;
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().failedCount += 1;

            AddReward(-10.0f);
            EndEpisode();
        }
        
    }

    IEnumerator RevertMaterial()
    {
        yield return new WaitForSeconds(0.3f);  // ������ �ð�

        // Floor & Target & Obstacle Rendering Initialize
        renderTarget.material.color = Color.blue;
    }
}
