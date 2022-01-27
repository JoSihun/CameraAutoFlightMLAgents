using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DroneAgent_Bak1 : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;
    private Transform tfObstacles;

    private float maxSpeed = 50.0f;
    private float addForce = 25.0f;
    private float rotSpeed = 10.0f;

    private float distAfter;
    private float distBefore;
    
    private RaycastHit rayHit;
    private float rayAngle = 10.0f;         // 전면 LiDAR 각도
    private float rayDistance = 10.0f;      // 상하좌우 LiDAR 범위
    private float rayDistance2 = 25.0f;     // 전면 LiDAR 범위
    private float rayDiameter = 10.0f;      // 전면 LiDAR 직경

    private Renderer renderGround;
    private Renderer renderTarget;

    // 초기화 작업을 위해 한번 호출되는 메소드
    public override void Initialize()
    {
        MaxStep = 2000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();

        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        tfObstacles = transform.parent.Find("Obstacles").gameObject.GetComponent<Transform>();


        // 컬러렌더링, 테스트환경에서 삭제되는 코드
        renderGround = transform.parent.Find("Ground").gameObject.GetComponent<Renderer>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();
    }

    // 에피소드가 시작할 때마다 호출
    public override void OnEpisodeBegin()
    {
        // Agent 물리력 및 회전력 초기화
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;
        tfAgent.eulerAngles = Vector3.zero;

        // Agnet & Target 위치 랜덤생성
        //tfAgent.localPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(-50.0f, 50.0f));
        //tfTarget.localPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(-50.0f, 50.0f));
        tfAgent.localPosition = new Vector3(0.0f, 50.0f, -25.0f);
        tfTarget.localPosition = new Vector3(0.0f, 50.0f, 1025.0f);

        /*
        // Obstacle 위치 랜덤생성
        for (int i = 0; i < tfObstacles.childCount; i++)
        {
            int selectRotation = Random.Range(1, 4);
            while (true)
            {
                Vector3 randomPosition = new Vector3();
                if (selectRotation == 1)
                {
                    // 장애물이 수직일 때
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 0);
                    randomPosition = new Vector3(Random.Range(-50.0f, 50.0f), 50.0f, Random.Range(0.0f, 900.0f));
                }
                if (selectRotation == 2)
                {
                    // 장애물이 전면수평일 때
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(90, 0, 0);
                    randomPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(50.0f, 900.0f));
                }
                if (selectRotation == 3)
                {
                    // 장애물이 측면수평일 때
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 90);
                    randomPosition = new Vector3(0.0f, Random.Range(0.0f, 100.0f), Random.Range(50.0f, 900.0f));
                }

                // Agent & Target과 겹침 방지
                float distance1 = Vector3.Distance(tfAgent.localPosition, randomPosition);
                float distance2 = Vector3.Distance(tfTarget.localPosition, randomPosition);
                if (distance1 > 10.0f && distance2 > 10.0f)
                {
                    tfObstacles.GetChild(i).transform.localPosition = randomPosition;
                    break;
                }
            }
        }
        */

        // 보상값 계산을 위한 Agent와 Target간 방향벡터 및 초기거리
        distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);


        // 테스트환경에서 삭제되는 코드
        // Material 컬러 초기화, 육안확인을 위한 코루틴 딜레이, 
        StartCoroutine(RevertMaterial());
    }

    // 환경정보 관측 및 수집, 정책결정을 위한 브레인에 전달
    public override void CollectObservations(VectorSensor sensor)
    {
        // 총 관측값 12 + 9 = 21개
        sensor.AddObservation(tfAgent.localPosition);       // 관측값 3개(x, y, z), Agent Position
        sensor.AddObservation(tfTarget.localPosition);      // 관측값 3개(x, y, z), Target Position

        sensor.AddObservation(rbAgent.velocity);            // 관측값 3개(x, y, z), Agent Velocity
        sensor.AddObservation(rbAgent.angularVelocity);     // 관측값 3개(x, y, z), Agent Angular Velocity

        sensor.AddObservation(CameraObservation("Result1_L", "Result1_R"));


        /*
        sensor.AddObservation(RayObservation(-90f, 0f, 0f, rayDistance));          // 관측값 1개(float), Detected Obeject Distance for Upward
        sensor.AddObservation(RayObservation(90f, 0f, 0f, rayDistance));           // 관측값 1개(float), Detected Obeject Distance for Downward
        sensor.AddObservation(RayObservation(0f, -90f, 0f, rayDistance));          // 관측값 1개(float), Detected Obeject Distance for Leftward
        sensor.AddObservation(RayObservation(0f, 90f, 0f, rayDistance));           // 관측값 1개(float), Detected Obeject Distance for Rightward

        sensor.AddObservation(RayObservation(0f, 0f, 0f, rayDistance2));            // 관측값 1개(float), Detected Obeject Distance for Forward
        sensor.AddObservation(RayObservation(-rayAngle, 0f, 0f, rayDistance2));     // 관측값 1개(float), Detected Obeject Distance for ForwardU
        sensor.AddObservation(RayObservation(rayAngle, 0f, 0f, rayDistance2));      // 관측값 1개(float), Detected Obeject Distance for ForwardD
        sensor.AddObservation(RayObservation(0f, -rayAngle, 0f, rayDistance2));     // 관측값 1개(float), Detected Obeject Distance for ForwardL
        sensor.AddObservation(RayObservation(0f, rayAngle, 0f, rayDistance2));      // 관측값 1개(float), Detected Obeject Distance for ForwardR
        */


        //sensor.AddObservation(RayObservation(-90f, 0f, 0f));     // 관측값 1개(float), Detected Obeject Distance for Upward
        //sensor.AddObservation(RayObservation(90f, 0f, 0f));      // 관측값 1개(float), Detected Obeject Distance for Downward
        //sensor.AddObservation(RayObservation(0f, -90f, 0f));     // 관측값 1개(float), Detected Obeject Distance for Leftward
        //sensor.AddObservation(RayObservation(0f, 90f, 0f));      // 관측값 1개(float), Detected Obeject Distance for Rightward

        //sensor.AddObservation(RayObservation(0f, 0f, 0f));       // 관측값 1개(float), Detected Obeject Distance for Forward
        //sensor.AddObservation(RayObservation(-25f, 0f, 0f));     // 관측값 1개(float), Detected Obeject Distance for ForwardU
        //sensor.AddObservation(RayObservation(25f, 0f, 0f));      // 관측값 1개(float), Detected Obeject Distance for ForwardD
        //sensor.AddObservation(RayObservation(0f, -25f, 0f));     // 관측값 1개(float), Detected Obeject Distance for ForwardL
        //sensor.AddObservation(RayObservation(0f, 25f, 0f));      // 관측값 1개(float), Detected Obeject Distance for ForwardR

    }

    // 브레인(정책)으로부터 전달받은 행동을 실행하는 메소드
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 이동 행동 선택
        float positionX = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float positionY = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float positionZ = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);
        //float rotationY = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);

        // 이동 벡터 결정
        Vector3 directionP = Vector3.right * positionX + Vector3.up * positionY + Vector3.forward * positionZ;      // 월드좌표(월드기준)
        //Vector3 directionP = tfAgent.right * positionX + tfAgent.up * positionY + tfAgent.forward * positionZ;      // 로컬좌표(드론기준)

        // 이동 행동 수행(속도제한)
        //rbAgent.AddTorque(directionR.normalized * rotSpeed);
        if (rbAgent.velocity.magnitude < maxSpeed) rbAgent.AddForce(directionP.normalized * addForce);

        /*
        if (RayObservation(-90f, 0f, 0f, rayDistance) >= 0) AddReward(RayObservation(-90f, 0f, 0f, rayDistance) - 1f);             // Detected Obeject Distance for Upward
        if (RayObservation(90f, 0f, 0f, rayDistance) >= 0) AddReward(RayObservation(90f, 0f, 0f, rayDistance) - 1f);               // Detected Obeject Distance for Downward
        if (RayObservation(0f, -90f, 0f, rayDistance) >= 0) AddReward(RayObservation(0f, -90f, 0f, rayDistance) - 1f);             // Detected Obeject Distance for Leftward
        if (RayObservation(0f, 90f, 0f, rayDistance) >= 0) AddReward(RayObservation(0f, 90f, 0f, rayDistance) - 1f);               // Detected Obeject Distance for Rightward

        if (RayObservation(0f, 0f, 0f, rayDistance) >= 0) AddReward(RayObservation(0f, 0f, 0f, rayDistance) - 1f);                 // Detected Obeject Distance for Forward
        if (RayObservation(-rayAngle, 0f, 0f, rayDistance) >= 0) AddReward(RayObservation(-rayAngle, 0f, 0f, rayDistance) - 1f);   // Detected Obeject Distance for ForwardU
        if (RayObservation(rayAngle, 0f, 0f, rayDistance) >= 0) AddReward(RayObservation(rayAngle, 0f, 0f, rayDistance) - 1f);     // Detected Obeject Distance for ForwardD
        if (RayObservation(0f, -rayAngle, 0f, rayDistance) >= 0) AddReward(RayObservation(0f, -rayAngle, 0f, rayDistance) - 1f);   // Detected Obeject Distance for ForwardL
        if (RayObservation(0f, rayAngle, 0f, rayDistance) >= 0) AddReward(RayObservation(0f, rayAngle, 0f, rayDistance) - 1f);     // Detected Obeject Distance for ForwardR
        */

        /*
        if (Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition) < 5f)
        {
            GameObject.Find("TestDirector").GetComponent<TestDirector>().IncreaseSuccess();
            //GameObject.Find("LearningDirector").GetComponent<LearningDirector>().IncreaseSuccess();
            renderGround.material.color = Color.green;
            AddReward(1f);
            EndEpisode();
        }
        //else if (
        //    (0f <= SphereRayObservation(0f, 0f, 0f) * rayDistance && SphereRayObservation(0f, 0f, 0f) * rayDistance <= 0.5f) ||         // Detect Forward
        //    (0f <= SphereRayObservation(25f, 0f, 0f) * rayDistance && SphereRayObservation(25f, 0f, 0f) * rayDistance <= 0.5f) ||       // Detect ForwardU
        //    (0f <= SphereRayObservation(-25f, 0f, 0f) * rayDistance && SphereRayObservation(-25f, 0f, 0f) * rayDistance <= 0.5f) ||     // Detect ForwardD
        //    (0f <= SphereRayObservation(0f, -25f, 0f) * rayDistance && SphereRayObservation(0f, -25f, 0f) * rayDistance <= 0.5f) ||     // Detect ForwardL
        //    (0f <= SphereRayObservation(0f, 25f, 0f) * rayDistance && SphereRayObservation(0f, 25f, 0f) * rayDistance <= 0.5f))         // Detect ForwardR
        else if (0f <= SphereRayObservation(-90f, 0f, 0f, rayDistance) * rayDistance && SphereRayObservation(-90f, 0f, 0f, rayDistance) * rayDistance <= 0.5f ||            // Detect Up
            0f <= SphereRayObservation(90f, 0f, 0f, rayDistance) * rayDistance && SphereRayObservation(90f, 0f, 0f, rayDistance) * rayDistance <= 0.5f ||                   // Detect Down
            0f <= SphereRayObservation(0f, -90f, 0f, rayDistance) * rayDistance && SphereRayObservation(0f, -90f, 0f, rayDistance) * rayDistance <= 0.5f ||                 // Detect Left
            0f <= SphereRayObservation(0f, 90f, 0f, rayDistance) * rayDistance && SphereRayObservation(0f, 90f, 0f, rayDistance) * rayDistance <= 0.5f ||                   // Detect Right
            0f <= SphereRayObservation(0f, 0f, 0f, rayDistance2) * rayDistance2 && SphereRayObservation(0f, 0f, 0f, rayDistance2) * rayDistance2 <= 0.5f ||                   // Detect Forward
            0f <= SphereRayObservation(-rayAngle, 0f, 0f, rayDistance2) * rayDistance2 && SphereRayObservation(-rayAngle, 0f, 0f, rayDistance2) * rayDistance2 <= 0.5f ||     // Detect ForwardU
            0f <= SphereRayObservation(rayAngle, 0f, 0f, rayDistance2) * rayDistance2 && SphereRayObservation(rayAngle, 0f, 0f, rayDistance2) * rayDistance2 <= 0.5f ||       // Detect ForwardD
            0f <= SphereRayObservation(0f, -rayAngle, 0f, rayDistance2) * rayDistance2 && SphereRayObservation(0f, -rayAngle, 0f, rayDistance2) * rayDistance2 <= 0.5f ||     // Detect ForwardL
            0f <= SphereRayObservation(0f, rayAngle, 0f, rayDistance2) * rayDistance2 && SphereRayObservation(0f, rayAngle, 0f, rayDistance2) * rayDistance2 <= 0.5f)         // Detect ForwardR
        {
            GameObject.Find("TestDirector").GetComponent<TestDirector>().IncreaseSuccess();
            //GameObject.Find("LearningDirector").GetComponent<LearningDirector>().IncreaseFailed();
            renderGround.material.color = Color.red;
            SetReward(-1);
            EndEpisode();
        }
        else
        {
            // 이동 보상처리
            distAfter = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);        // 행동수행후 Agent, Target간 거리
            AddReward(distBefore - distAfter);
            distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);
        }
        */

        distAfter = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);        // 행동수행후 Agent, Target간 거리
        AddReward((distBefore - distAfter) * 10f);
        distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);

        // 지속적인 행동선택을 위한 패널티
        // AddReward(-0.01f);

        /*
        Debug.Log("Up Distance = " + RayObservation(-90f, 0f, 0f, rayDistance) * rayDistance);
        Debug.Log("Down Distance = " + RayObservation(90f, 0f, 0f, rayDistance) * rayDistance);
        Debug.Log("Left Distance = " + RayObservation(0f, -90f, 0f, rayDistance) * rayDistance);
        Debug.Log("Right Distance = " + RayObservation(0f, 90f, 0f, rayDistance) * rayDistance);

        Debug.Log("Forward Distance = " + RayObservation(0f, 0f, 0f, rayDistance2) * rayDistance);
        Debug.Log("ForwardUp Distance = " + RayObservation(-rayAngle, 0f, 0f, rayDistance2) * rayDistance);
        Debug.Log("ForwardDown Distance = " + RayObservation(rayAngle, 0f, 0f, rayDistance2) * rayDistance);
        Debug.Log("ForwardLeft Distance = " + RayObservation(0f, -rayAngle, 0f, rayDistance2) * rayDistance);
        Debug.Log("ForwardRight Distance = " + RayObservation(0f, rayAngle, 0f, rayDistance2) * rayDistance);
        */
        
    }

    // 개발자가 직접행동 테스트
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*
        // Heuristic user Control Actions
        var ContinousActionsOut = actionsOut.ContinuousActions;

        // Heuristic User Control PositionX(Left Right)
        if (Input.GetKey(KeyCode.D))
            ContinousActionsOut[0] = 1.0f;
        if (Input.GetKey(KeyCode.A))
            ContinousActionsOut[0] = -1.0f;

        // Heuristic User Control PositionY(Up Down)
        if (Input.GetKey(KeyCode.E))
            ContinousActionsOut[1] = 1.0f;
        if (Input.GetKey(KeyCode.Q))
            ContinousActionsOut[1] = -1.0f;

        // Heuristic User Control PositionZ(Forth Back)
        if (Input.GetKey(KeyCode.W))
            ContinousActionsOut[2] = 1.0f;
        if (Input.GetKey(KeyCode.S))
            ContinousActionsOut[2] = -1.0f;


        // Heuristic User Control RotationX(Forth Back)
        if (Input.GetKey(KeyCode.Keypad8))
            ContinousActionsOut[3] = 1.0f;
        if (Input.GetKey(KeyCode.Keypad5))
            ContinousActionsOut[3] = -1.0f;

        // Heuristic User Control RotationY(Left Right)
        if (Input.GetKey(KeyCode.Keypad6))
            ContinousActionsOut[4] = 1.0f;
        if (Input.GetKey(KeyCode.Keypad4))
            ContinousActionsOut[4] = -1.0f;

        // Heuristic User Control RotationZ(Left Right)
        if (Input.GetKey(KeyCode.Keypad7))
            ContinousActionsOut[5] = 1.0f;
        if (Input.GetKey(KeyCode.Keypad9))
            ContinousActionsOut[5] = -1.0f;

        */
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Target"))
        {
            GameObject.Find("TestDirector").GetComponent<TestDirector>().IncreaseSuccess();
            //GameObject.Find("LearningDirector").GetComponent<LearningDirector>().IncreaseSuccess();
            renderGround.material.color = Color.green;
            AddReward(10f);
            EndEpisode();
        }
        else
        {
            GameObject.Find("TestDirector").GetComponent<TestDirector>().IncreaseFailed();
            //GameObject.Find("LearningDirector").GetComponent<LearningDirector>().IncreaseFailed();
            renderGround.material.color = Color.red;
            SetReward(-10f);
            EndEpisode();
        }
    }

    private float CameraObservation(string result1, string result2)
    {
        // Stereo 카메라 기하학에 따른 거리측정
        float limitDistance = 15.0f;
        Vector2 center = new Vector2(75.0f, 75.0f);     // 카메라 앵커좌표 중심
        float f = 225.0f;                               // 초점거리
        float W = 150.0f;                               // 카메라가 표시할 수 있는 최대너비        
        float B = 1.0f;                                 // 카메라 이격거리
        for (int i = 0; i < 50; i++)
        {
            GameObject childL = GameObject.Find(result1).transform.GetChild(i).gameObject;
            GameObject childR = GameObject.Find(result2).transform.GetChild(i).gameObject;

            // 첫번째 카메라 보상
            if (childL.activeSelf && childR.activeSelf)
            {
                // 검출된 객체이름 & 검출객체 신뢰도
                string textL = childL.GetComponentInChildren<Text>().text;              // 검출객체이름L
                string textR = childR.GetComponentInChildren<Text>().text;              // 검출객체이름R
                float accuracyL = float.Parse(Regex.Replace(textL, @"\D", "")) / 100;   // 검출신뢰도L
                float accuracyR = float.Parse(Regex.Replace(textR, @"\D", "")) / 100;   // 검출신뢰도R

                float widthL = childL.GetComponent<RectTransform>().rect.width;         // 검출객체L 너비
                float heightL = childL.GetComponent<RectTransform>().rect.height;       // 검출객체L 높이
                float widthR = childR.GetComponent<RectTransform>().rect.width;         // 검출객체R 너비
                float heightR = childR.GetComponent<RectTransform>().rect.height;       // 검출객체R 높이

                float posLX = childL.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체L x좌표
                float posLY = childL.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체L y좌표
                float posRX = childR.GetComponent<RectTransform>().anchoredPosition.x;  // 검출객체R x좌표
                float posRY = childR.GetComponent<RectTransform>().anchoredPosition.y;  // 검출객체R y좌표                

                Vector2 posL = new Vector2(posLX - center.x, posLY - center.y);         // 검출객체L 앵커좌표변환
                Vector2 posR = new Vector2(posRX - center.x, posRY - center.y);         // 검출객체R 앵커좌표변환

                // 스테레오 기하학에 의한 거리측정
                float w = (widthL + widthR) / 2;                                        // 물체의 너비
                float b = Vector2.Distance(new Vector2(0.0f, 0.0f), posL - posR); ;     // 카메라의 센터로부터 물체까지의 거리(음양존재)

                float D1 = (B * f) / b;                                                 // 물체까지의 거리1
                float D2 = (W * f) / w;                                                 // 물체까지의 거리2

                return (-limitDistance / (D1 + limitDistance));

                /*
                // 확인용, 추후 삭제할 코드
                Debug.Log("Camera_L Text = " + textL + " | ConvertedPosL = " + posL + " | ConvertedPosR = " + posR);
                Debug.Log("Camera_R Text = " + textR + " | f = " + f + " | b = " + b + " | B = " + B + " | w = " + w + " | W = " + W);
                
                Transform tfBus = GameObject.Find("Bus_green").transform;
                float realDistance = (tfAgent.localPosition - tfBus.localPosition).magnitude;
                Debug.Log("Real Distance = " + realDistance + " | Stereo Distance1(b, B) = " + D1 + " | Stereo Distance2(w, W) = " + D2);
                */
            }

        }
        return -1;
    }

    private float RayObservation(float angleX, float angleY, float angleZ, float limitDistance)
    {
        //Physics.SphereCast(position, rayDiameter / 2.0f, direction, out rayHit, rayDistance);
        //return rayHit.collider.gameObject.transform.localPosition;

        var eulerAngle = Quaternion.Euler(angleX, angleY, angleZ);
        var direction = eulerAngle * tfAgent.forward;

        Physics.Raycast(tfAgent.localPosition, direction, out rayHit, limitDistance);
        return rayHit.distance >= 0f ? rayHit.distance / limitDistance : -1f;
    }

    private float SphereRayObservation(float angleX, float angleY, float angleZ, float limitDistance)
    {
        //Physics.SphereCast(position, rayDiameter / 2.0f, direction, out rayHit, rayDistance);
        //return rayHit.collider.gameObject.transform.localPosition;

        var eulerAngle = Quaternion.Euler(angleX, angleY, angleZ);
        var direction = eulerAngle * tfAgent.forward;

        Physics.SphereCast(tfAgent.localPosition, rayDiameter / 2.0f, direction, out rayHit, limitDistance);
        return rayHit.distance >= 0 ? rayHit.distance / limitDistance : -1f;
    }

    IEnumerator RevertMaterial()
    {
        yield return new WaitForSeconds(0.3f);  // 딜레이 시간

        // Floor & Target & Obstacle Rendering Initialize
        renderGround.material.color = Color.white;
        renderTarget.material.color = Color.blue;

    }

    void Start()
    {

    }

    
    void Update()
    {
        // 드론-목표지점 회전 및 Ray 표시
        tfAgent.LookAt(new Vector3(tfTarget.position.x, tfAgent.position.y, tfTarget.position.z));
        Debug.DrawRay(tfAgent.position, (tfTarget.localPosition - tfAgent.localPosition), Color.black);

        /*
        // RayCast Lidar Drawing
        Debug.DrawRay(tfAgent.position, tfAgent.forward * rayDistance, Color.blue);
        Debug.DrawRay(tfAgent.position, tfAgent.forward * -rayDistance, Color.blue);
        Debug.DrawRay(tfAgent.position, tfAgent.right * rayDistance, Color.red);
        Debug.DrawRay(tfAgent.position, tfAgent.right * -rayDistance, Color.red);
        Debug.DrawRay(tfAgent.position, tfAgent.up * rayDistance, Color.green);
        Debug.DrawRay(tfAgent.position, tfAgent.up * -rayDistance, Color.green);
        */

    }

    private void OnDrawGizmos()
    {
        /*
        // Drawing SphereCast Ray
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * rayDistance);
        Gizmos.DrawRay(transform.position, transform.forward * -rayDistance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * rayDistance, rayDiameter / 2.0f);
        Gizmos.DrawWireSphere(transform.position + transform.forward * -rayDistance, rayDiameter / 2.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * rayDistance);
        Gizmos.DrawRay(transform.position, transform.right * -rayDistance);
        Gizmos.DrawWireSphere(transform.position + transform.right * rayDistance, rayDiameter / 2.0f);
        Gizmos.DrawWireSphere(transform.position + transform.right * -rayDistance, rayDiameter / 2.0f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * rayDistance);
        Gizmos.DrawRay(transform.position, transform.up * -rayDistance);
        Gizmos.DrawWireSphere(transform.position + transform.up * rayDistance, rayDiameter / 2.0f);
        Gizmos.DrawWireSphere(transform.position + transform.up * -rayDistance, rayDiameter / 2.0f);
        */

        
        
        // Drawing SphereCast Ray
        Gizmos.color = Color.green;
        var eulerAngleU = Quaternion.Euler(rayAngle, 0f, 0f) * transform.forward;
        var eulerAngleD = Quaternion.Euler(-rayAngle, 0f, 0f) * transform.forward;
        Gizmos.DrawRay(transform.position, transform.up * rayDistance);                                     // Up
        Gizmos.DrawRay(transform.position, -transform.up * rayDistance);                                    // Down
        Gizmos.DrawRay(transform.position, eulerAngleU * rayDistance2);                                      // ForwardUp
        Gizmos.DrawRay(transform.position, eulerAngleD * rayDistance2);                                      // ForwardDown
        //Gizmos.DrawWireSphere(transform.position + transform.up * rayDistance, rayDiameter / 2.0f);         // Up
        //Gizmos.DrawWireSphere(transform.position + -transform.up * rayDistance, rayDiameter / 2.0f);        // Down
        //Gizmos.DrawWireSphere(transform.position + eulerAngleU * rayDistance2, rayDiameter / 2.0f);          // ForwardUp
        //Gizmos.DrawWireSphere(transform.position + eulerAngleD * rayDistance2, rayDiameter / 2.0f);          // ForwardDown
        
        Gizmos.color = Color.red;
        var eulerAngleL = Quaternion.Euler(0f, -rayAngle, 0f) * transform.forward;
        var eulerAngleR = Quaternion.Euler(0f, rayAngle, 0f) * transform.forward;
        Gizmos.DrawRay(transform.position, -transform.right * rayDistance);                                 // Left
        Gizmos.DrawRay(transform.position, transform.right * rayDistance);                                  // Right
        Gizmos.DrawRay(transform.position, eulerAngleL * rayDistance2);                                      // ForwardLeft
        Gizmos.DrawRay(transform.position, eulerAngleR * rayDistance2);                                      // ForwardRight
        //Gizmos.DrawWireSphere(transform.position + -transform.right * rayDistance, rayDiameter / 2.0f);     // Left
        //Gizmos.DrawWireSphere(transform.position + transform.right * rayDistance, rayDiameter / 2.0f);      // Right
        //Gizmos.DrawWireSphere(transform.position + eulerAngleL * rayDistance2, rayDiameter / 2.0f);          // ForwardLeft
        //Gizmos.DrawWireSphere(transform.position + eulerAngleR * rayDistance2, rayDiameter / 2.0f);          // ForwardRight

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * rayDistance2);                                // Forward
        //Gizmos.DrawWireSphere(transform.position + transform.forward * rayDistance2, rayDiameter / 2.0f);    // Forward


        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward, rayDistance);       
        
    }
}
