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
        // Agent 물리력 초기화
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;
        tfAgent.localEulerAngles = new Vector3(0, 0, 0);

        // Agent & Target & Obstacle 초기위치 무작위 설정
        tfAgent.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        tfTarget.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        foreach (Transform child in GameObject.Find("Obstacles").transform)
            child.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));

        // 보상값 계산을 위한 Agent와 Target간 초기거리
        distBefore = (tfAgent.localPosition - tfTarget.localPosition).magnitude;

        // Material 컬러 초기화, 육안확인을 위한 코루틴 딜레이
        StartCoroutine(RevertMaterial());

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(tfAgent.localPosition - tfTarget.localPosition);      // 관측값 3개, Agent로부터 Target으로의 거리방향벡터(x, y, z) 3개
        sensor.AddObservation(tfTarget.localPosition);                              // 관측값 3개, Target 로컬좌표값(x, y, z) 3개 
        sensor.AddObservation(tfAgent.localPosition);                               // 관측값 3개, Agent 로컬좌표값(x, y, z) 3개 

        sensor.AddObservation(rbAgent.velocity.x);                                  // 관측값 1개, Agent x축 방향 물리력 1개
        sensor.AddObservation(rbAgent.velocity.y);                                  // 관측값 1개, Agent y축 방향 물리력 1개
        sensor.AddObservation(rbAgent.velocity.z);                                  // 관측값 1개, Agent z축 방향 물리력 1개
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float upDown = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float backForth = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float leftRight = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);

        Vector3 direction = Vector3.up * upDown + Vector3.forward * backForth + Vector3.right * leftRight;
        rbAgent.AddForce(direction.normalized * addForce);

        // 수정작업필요
        // 라벨에 따라 보상/패널티 배수 결정
        // RectSize에 따라 보상/패널티 결정
        string[] labels_high = { "Person", "Bus", "Car", "Motorbike", "Bird", "Cat", "Dog", "Train", "Bicycle" };
        string[] labels_middle = { "Plane", "Table", "Chair", "Sofa", "TV", "Bottle", "Plant" };
        string[] labels_low = { "Boat", "Cow", "Horse", "Sheep" };
        var list_labels_high = new List<string>();
        var list_labels_middle = new List<string>();
        var list_labels_low = new List<string>();
        list_labels_high.AddRange(labels_high);
        list_labels_middle.AddRange(labels_middle);
        list_labels_low.AddRange(labels_low);

        // Stereo 카메라 기하학에 따른 거리측정
        float limitDistance = 15.0f;
        Vector2 center = new Vector2(75.0f, 75.0f);     // 카메라 앵커좌표 중심
        float f = 225.0f;                               // 초점거리
        float W = 150.0f;                               // 카메라가 표시할 수 있는 최대너비        
        float B = 1.0f;                                 // 카메라 이격거리
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

            // 첫번째 카메라 보상
            if (childL1.activeSelf && childR1.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL1.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR1.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float widthL = childL1.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL1.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR1.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR1.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                float posLX = childL1.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL1.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR1.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR1.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표                

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)
                
                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("UPCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("UPCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);
                
                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // 두번째 카메라 보상
            if (childL2.activeSelf && childR2.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL2.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR2.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float posLX = childL2.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL2.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR2.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR2.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표

                float widthL = childL2.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL2.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR2.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR2.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("DownCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("DownCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // 세번째 카메라 보상
            if (childL3.activeSelf && childR3.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL3.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR3.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float posLX = childL3.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL3.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR3.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR3.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표

                float widthL = childL3.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL3.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR3.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR3.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("FrontCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("FrontCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // 네번째 카메라 보상
            if (childL4.activeSelf && childR4.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL4.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR4.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float posLX = childL4.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL4.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR4.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR4.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표

                float widthL = childL4.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL4.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR4.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR4.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("BackCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("BackCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // 다섯번째 카메라 보상
            if (childL5.activeSelf && childR5.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL5.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR5.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float posLX = childL5.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL5.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR5.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR5.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표

                float widthL = childL5.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL5.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR5.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR5.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("LeftCamera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("LeftCamera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);

                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

            // 여섯번째 카메라 보상
            if (childL6.activeSelf && childR6.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL6.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR6.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float posLX = childL6.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL6.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR6.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR6.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표

                float widthL = childL6.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL6.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR6.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR6.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                AddReward(-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
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
                // 검출된 객체이름 & 검출객체 신뢰도
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
                // 검출된 객체이름 & 검출객체 신뢰도
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

        // 보상처리
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;                 // 행동수행후 Agent, Target간 거리
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;      // 행동수행후 UI 거리표기 갱신

        AddReward(distBefore - distAfter);                                                      // 현재거리가 이전거리보다 짧으면 +보상, 멀면 -패널티
        distBefore = distAfter;                                                                 // 다음행동을 위한 이전거리 갱신
        AddReward(-0.1f);                                                                       // 지속적인 행동선택을 위한 패널티

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
        // Collider 충돌 시 패널티부여, EndEpisode O
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
        yield return new WaitForSeconds(0.3f);  // 딜레이 시간

        // Floor & Target & Obstacle Rendering Initialize
        renderFloor.material.color = Color.white;
        renderTarget.material.color = Color.blue;
    }
}
