using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class StereoAgent : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;

    private float distAfter;
    private float distBefore;
    private float addForce = 25.0f;

    private Renderer renderFloor;
    private Renderer renderTarget;

    public override void Initialize()
    {
        MaxStep = 1000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();
        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        renderFloor = transform.parent.Find("Ground").gameObject.GetComponent<Renderer>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();

    }

    public override void OnEpisodeBegin()
    {
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

        sensor.AddObservation(rbAgent.velocity.x);                                  // ������ 1��, Agent x�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.y);                                  // ������ 1��, Agent y�� ���� ������ 1��
        sensor.AddObservation(rbAgent.velocity.z);                                  // ������ 1��, Agent z�� ���� ������ 1��
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float upDown = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float backForth = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float leftRight = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);

        Vector3 direction = Vector3.up * upDown + Vector3.forward * backForth + Vector3.right * leftRight;
        rbAgent.AddForce(direction.normalized * addForce);

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

        // Stereo ī�޶� �����п� ���� �Ÿ�����
        float limitDistance = 15.0f;
        Vector2 center = new Vector2(75.0f, 75.0f);     // ī�޶� ��Ŀ��ǥ �߽�
        float f = 225.0f;                               // �����Ÿ�
        float W = 150.0f;                               // ī�޶� ǥ���� �� �ִ� �ִ�ʺ�        
        float B = 1.0f;                                 // ī�޶� �̰ݰŸ�
        for (int i = 0; i < 50; i++)
        {
            GameObject childL1 = GameObject.Find("Result1_L").transform.GetChild(i).gameObject;
            GameObject childR1 = GameObject.Find("Result1_R").transform.GetChild(i).gameObject;

            GameObject childL2 = GameObject.Find("Result2_L").transform.GetChild(i).gameObject;
            GameObject childR2 = GameObject.Find("Result2_R").transform.GetChild(i).gameObject;

            GameObject childL3 = GameObject.Find("Result3_L").transform.GetChild(i).gameObject;
            GameObject childR3 = GameObject.Find("Result3_R").transform.GetChild(i).gameObject;

            GameObject childL4 = GameObject.Find("Result4_L").transform.GetChild(i).gameObject;
            GameObject childR4 = GameObject.Find("Result4_R").transform.GetChild(i).gameObject;

            GameObject childL5 = GameObject.Find("Result5_L").transform.GetChild(i).gameObject;
            GameObject childR5 = GameObject.Find("Result5_R").transform.GetChild(i).gameObject;

            GameObject childL6 = GameObject.Find("Result6_L").transform.GetChild(i).gameObject;
            GameObject childR6 = GameObject.Find("Result6_R").transform.GetChild(i).gameObject;

            // ù��° ī�޶� ����
            if (childL1.activeSelf && childR1.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL1.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR1.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float widthL = childL1.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL1.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR1.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR1.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                float posLX = childL1.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL1.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR1.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR1.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ                

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)
                
                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("UPCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("UPCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);
                
                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // �ι�° ī�޶� ����
            if (childL2.activeSelf && childR2.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL2.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR2.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float posLX = childL2.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL2.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR2.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR2.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ

                float widthL = childL2.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL2.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR2.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR2.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)

                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("DownCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("DownCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // ����° ī�޶� ����
            if (childL3.activeSelf && childR3.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL3.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR3.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float posLX = childL3.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL3.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR3.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR3.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ

                float widthL = childL3.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL3.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR3.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR3.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)

                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("FrontCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("FrontCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // �׹�° ī�޶� ����
            if (childL4.activeSelf && childR4.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL4.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR4.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float posLX = childL4.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL4.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR4.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR4.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ

                float widthL = childL4.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL4.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR4.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR4.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)

                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("BackCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("BackCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // �ټ���° ī�޶� ����
            if (childL5.activeSelf && childR5.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL5.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR5.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float posLX = childL5.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL5.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR5.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR5.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ

                float widthL = childL5.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL5.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR5.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR5.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)

                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("LeftCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("LeftCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // ������° ī�޶� ����
            if (childL6.activeSelf && childR6.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string textL = childL6.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�L
                string textR = childR6.GetComponentInChildren<Text>().text;              // ���ⰴü�̸�R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // ����ŷڵ�L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // ����ŷڵ�R

                float posLX = childL6.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüL x��ǥ
                float posLY = childL6.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüL y��ǥ
                float posRX = childR6.GetComponent<RectTransform>().anchoredPosition.x;  // ���ⰴüR x��ǥ
                float posRY = childR6.GetComponent<RectTransform>().anchoredPosition.y;  // ���ⰴüR y��ǥ

                float widthL = childL6.GetComponent<RectTransform>().rect.width;         // ���ⰴüL �ʺ�
                float heightL = childL6.GetComponent<RectTransform>().rect.height;       // ���ⰴüL ����
                float widthR = childR6.GetComponent<RectTransform>().rect.width;         // ���ⰴüR �ʺ�
                float heightR = childR6.GetComponent<RectTransform>().rect.height;       // ���ⰴüR ����

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // ���ⰴüL ��Ŀ��ǥ��ȯ
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // ���ⰴüR ��Ŀ��ǥ��ȯ

                // ���׷��� �����п� ���� �Ÿ�����
                float w = (widthL + widthR) / 2;                                        // ��ü�� �ʺ�
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // ī�޶��� ���ͷκ��� ��ü������ �Ÿ�(��������)

                float D1 = (B * f) / b;                                                 // ��ü������ �Ÿ�1
                float D2 = (W * f) / w;                                                 // ��ü������ �Ÿ�2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // Ȯ�ο�, ���� ������ �ڵ�
                Debug.Log("RightCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("RightCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }



        }

        /*
        foreach (Transform child in GameObject.Find("Result3_L").transform)
        {
            if (child.gameObject.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float posX = child.GetComponent<RectTransform>().anchoredPosition.x;
                float posY = child.GetComponent<RectTransform>().anchoredPosition.y;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;

                float areaThreshold = 100.0f;
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

                Debug.Log("UpCamera_L Text = " + text + " | posX = " + posX + " | posY = " + posY + " | width = " + width + " | height = " + height);
            }
        }

        foreach (Transform child in GameObject.Find("Result3_R").transform)
        {
            if (child.gameObject.activeSelf)
            {
                // ����� ��ü�̸� & ���ⰴü �ŷڵ�
                string text = child.GetComponentInChildren<Text>().text;
                float percent = float.Parse(Regex.Replace(text, @"\D", "")) / 100;

                float posX = child.GetComponent<RectTransform>().position.x;
                float posY = child.GetComponent<RectTransform>().position.y;
                float width = child.GetComponent<RectTransform>().rect.width;
                float height = child.GetComponent<RectTransform>().rect.height;

                Debug.Log("UpCamera_R Text = " + text + " | posX = " + posX + " | posY = " + posY + " | width = " + width + " | height = " + height);
            }

        }
        */

        // ����ó��
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;                 // �ൿ������ Agent, Target�� �Ÿ�
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;      // �ൿ������ UI �Ÿ�ǥ�� ����

        AddReward(distBefore - distAfter);                                                      // ����Ÿ��� �����Ÿ����� ª���� +����, �ָ� -�г�Ƽ
        distBefore = distAfter;                                                                 // �����ൿ�� ���� �����Ÿ� ����
        AddReward(-0.1f);                                                                       // �������� �ൿ������ ���� �г�Ƽ

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
            Debug.Log("Total = " + total + " | Success = " + success + " | Failed = " + failed + " | Accuracy = " + accuracy.ToString("P")
                + " | Distance = " + distance.ToString("F") + "m");

            AddReward(10.0f);
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
            Debug.Log("Total = " + total + " | Success = " + success + " | Failed = " + failed + " | Accuracy = " + accuracy.ToString("P")
                + " | Distance = " + distance.ToString("F") + "m");

            AddReward(-10.0f);
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
