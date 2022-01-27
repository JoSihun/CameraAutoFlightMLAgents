using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class YoloAgent : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;

    private float addForce = 25.0f;
    private float rotSpeed = 25.0f;

    private float distAfter;
    private float distBefore;
    private float sumReward = 0.0f;

    private Renderer renderFloor;
    private Renderer renderTarget;
    
    private Camera camera1;
    private Camera camera2;
    private Camera camera3;
    private Camera camera4;
    private Camera camera5;
    private Camera camera6;

    [SerializeField] private List<GameObject> findList1 = null;
    [SerializeField] private List<GameObject> findList2 = null;
    [SerializeField] private List<GameObject> findList3 = null;
    [SerializeField] private List<GameObject> findList4 = null;
    [SerializeField] private List<GameObject> findList5 = null;
    [SerializeField] private List<GameObject> findList6 = null;


    public override void Initialize()
    {
        MaxStep = 1000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();
        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        renderFloor = transform.parent.Find("Ground").gameObject.GetComponent<Renderer>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();

        camera1 = transform.Find("Camera1").gameObject.GetComponent<Camera>();
        camera2 = transform.Find("Camera2").gameObject.GetComponent<Camera>();
        camera3 = transform.Find("Camera3").gameObject.GetComponent<Camera>();
        camera4 = transform.Find("Camera4").gameObject.GetComponent<Camera>();
        camera5 = transform.Find("Camera5").gameObject.GetComponent<Camera>();
        camera6 = transform.Find("Camera6").gameObject.GetComponent<Camera>();
    }

    public override void OnEpisodeBegin()
    {
        // ���� ���Ǽҵ� ������ �ʱ�ȭ
        sumReward = 0.0f;

        // Agent ������ �ʱ�ȭ
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;
        tfAgent.localEulerAngles = new Vector3(0, 0, 0);

        // Agent & Target & Obstacle �ʱ���ġ ������ ����
        tfAgent.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        tfTarget.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        foreach (Transform child in GameObject.Find("Obstacles").transform)
            child.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));

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
        //sensor.AddObservation(tfAgent.eulerAngles.y);                               // ������ 1��, Agent ���Ϸ�����(y) 1�� 

        sensor.AddObservation(rbAgent.velocity.x);                                  // ������ 1��, Agent x�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.y);                                  // ������ 1��, Agent y�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.z);                                  // ������ 1��, Agent z�� ���� ������ 1��
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float upDown = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float backForth = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float leftRight = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);
        //float rotationY = Mathf.Clamp(actions.ContinuousActions[3], -1.0f, 1.0f);

        Vector3 direction = Vector3.up * upDown + Vector3.forward * backForth + Vector3.right * leftRight;
        //Vector3 rotation = new Vector3(0, rotationY, 0);
        rbAgent.AddForce(direction.normalized * addForce);
        //tfAgent.Rotate(rotation.normalized * rotSpeed * Time.deltaTime);


        // 
        foreach (Transform child in GameObject.Find("Obstacles").transform)
        {
            // ù��° ī�޶� ���� ������Ʈ
            Vector3 viewPos1 = camera1.WorldToViewportPoint(child.position);
            if (0 <= viewPos1.x && viewPos1.x <= 1 && 0 <= viewPos1.y && viewPos1.y <= 1 && 0 < viewPos1.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                Debug.Log("Object Name in Camera1 = " + child.name);
            }

        }

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

        // ù��° ī�޶� ����
        foreach (Transform child in GameObject.Find("Result1").transform)
        {
            if (child.gameObject.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
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
                sumReward += penalty;
                //Debug.Log("UpCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
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
                sumReward += penalty;
                //Debug.Log("DownCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
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
                sumReward += penalty;
                //Debug.Log("FrontCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
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
                sumReward += penalty;
                //Debug.Log("BackCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
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
                sumReward += penalty;
                //Debug.Log("LeftCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
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
                sumReward += penalty;
                //Debug.Log("RightCamera Text = " + text + " | areaWeight = " + areaWeight + " | Penalty = " + penalty);
            }
        }

        // ����ó��
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;         // �ൿ������ Agent, Target�� �Ÿ�
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;     // �ൿ������ UI �Ÿ�ǥ�� ����

        AddReward((distBefore - distAfter) * 50.0f);      // ����Ÿ��� �����Ÿ����� ª���� +����, �ָ� -�г�Ƽ
        sumReward += (distBefore - distAfter) * 50.0f;
        distBefore = distAfter;                 // �����ൿ�� ���� �����Ÿ� ����
        AddReward(-0.01f);                      // �������� �ൿ������ ���� �г�Ƽ
        sumReward += -0.01f;


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
            renderFloor.material.color = Color.green;

            int total = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount;
            int success = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().successCount;
            int failed = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().failedCount;
            double distance = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance;
            double accuracy = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().accuracy;
            sumReward += 50.0f;
            Debug.Log("Total = " + total + " | Success = " + success + " | Failed = " + failed + " | Accuracy = " + accuracy.ToString("P")
                + " | Distance = " + distance.ToString("F") + "m | sumReward = " + sumReward.ToString("F"));

            AddReward(50.0f);
            EndEpisode();
        }
        else
        {
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount += 1;
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().failedCount += 1;
            renderFloor.material.color = Color.red;

            int total = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount;
            int success = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().successCount;
            int failed = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().failedCount;
            double distance = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance;
            double accuracy = GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().accuracy;
            sumReward += -50.0f;
            Debug.Log("Total = " + total + " | Success = " + success + " | Failed = " + failed + " | Accuracy = " + accuracy.ToString("P")
                + " | Distance = " + distance.ToString("F") + "m | sumReward = " + sumReward.ToString("F"));

            AddReward(-50.0f);
            EndEpisode();
        }
        

    }

    IEnumerator RevertMaterial()
    {
        yield return new WaitForSeconds(0.3f);  // ������ �ð�

        // Floor & Target & Obstacle Rendering Initialize
        renderFloor.material.color = Color.white;
        renderTarget.material.color = Color.blue;
    }
}
