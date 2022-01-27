using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class LidarAgent : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;
    private Transform tfObstacle1;
    private Transform tfObstacle2;
    private Transform tfObstacle3;

    private float addForce = 25.0f;
    private float rotSpeed = 25.0f;

    private float distAfter;
    private float distBefore;

    private RaycastHit rayHit;
    public float rayDistance = 15.0f;

    private Renderer renderFloor;
    private Renderer renderTarget;
    private Renderer renderObstacle1;
    private Renderer renderObstacle2;
    private Renderer renderObstacle3;


    public override void Initialize()
    {
        MaxStep = 1000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();

        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        tfObstacle1 = transform.parent.Find("Obstacle1").gameObject.GetComponent<Transform>();
        tfObstacle2 = transform.parent.Find("Obstacle2").gameObject.GetComponent<Transform>();
        tfObstacle3 = transform.parent.Find("Obstacle3").gameObject.GetComponent<Transform>();

        renderFloor = transform.parent.Find("Ground").gameObject.GetComponent<Renderer>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();
        renderObstacle1 = transform.parent.Find("Obstacle1").gameObject.GetComponent<Renderer>();
        renderObstacle2 = transform.parent.Find("Obstacle2").gameObject.GetComponent<Renderer>();
        renderObstacle3 = transform.parent.Find("Obstacle3").gameObject.GetComponent<Renderer>();

    }

    public override void OnEpisodeBegin()
    {
        // Agent 물리력 초기화
        tfAgent.localEulerAngles = new Vector3(0, 0, 0);
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;

        // Agent & Target & Obstacle 초기위치 무작위 설정
        tfAgent.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(05.0f, 95.0f), Random.Range(-45.0f, 45.0f));
        tfTarget.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(05.0f, 95.0f), Random.Range(-45.0f, 45.0f));
        tfObstacle1.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(05.0f, 95.0f), Random.Range(-45.0f, 45.0f));
        tfObstacle2.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(05.0f, 95.0f), Random.Range(-45.0f, 45.0f));
        tfObstacle3.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(05.0f, 95.0f), Random.Range(-45.0f, 45.0f));

        // 보상값 계산을 위한 Agent와 Target간 초기거리
        distBefore = (tfAgent.localPosition - tfTarget.localPosition).magnitude;

        // Material 컬러 초기화, 육안확인을 위한 코루틴 딜레이
        StartCoroutine(RevertMaterial());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(tfTarget.localPosition);                              // 관측값 3개, Target 로컬좌표값(x, y, z) 3개 
        sensor.AddObservation(tfAgent.localPosition);                               // 관측값 3개, Agent 로컬좌표값(x, y, z) 3개 
        sensor.AddObservation(tfAgent.localPosition - tfTarget.localPosition);      // 관측값 3개, Agent로부터 Target으로의 거리방향벡터(x, y, z) 3개   

        //sensor.AddObservation(tfAgent.eulerAngles.x);                               // 관측값 1개, Agent 오일러각값(x) 1개
        //sensor.AddObservation(tfAgent.eulerAngles.y);                               // 관측값 1개, Agent 오일러각값(y) 1개
        //sensor.AddObservation(tfAgent.eulerAngles.z);                               // 관측값 1개, Agent 오일러각값(z) 1개

        sensor.AddObservation(rbAgent.velocity.x);                                  // 관측값 1개, Agent x축 방향 물리력 1개
        sensor.AddObservation(rbAgent.velocity.y);                                  // 관측값 1개, Agent y축 방향 물리력 1개
        sensor.AddObservation(rbAgent.velocity.z);                                  // 관측값 1개, Agent z축 방향 물리력 1개
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float upDown = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float backForth = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float leftRight = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);
        //float rotationX = Mathf.Clamp(actions.ContinuousActions[3], -1.0f, 1.0f);
        //float rotationY = Mathf.Clamp(actions.ContinuousActions[4], -1.0f, 1.0f);
        //float rotationZ = Mathf.Clamp(actions.ContinuousActions[5], -1.0f, 1.0f);

        Vector3 direction = Vector3.up * upDown + Vector3.forward * backForth + Vector3.right * leftRight;
        //Vector3 rotation = new Vector3(rotationX, rotationY, rotationZ);
        rbAgent.AddForce(direction.normalized * addForce);
        //tfAgent.Rotate(rotation.normalized * rotSpeed * Time.deltaTime);

        // Agent Rotation 보상설계
        //Vector3 rotAgent = tfAgent.eulerAngles;
        //Vector3 rotTarget = Quaternion.LookRotation(new Vector3(tfTarget.position.x - tfAgent.position.x, 0, tfTarget.position.z - tfAgent.position.z)).eulerAngles;
        //AddReward(-Mathf.Abs(rotAgent.x));
        //AddReward(-Mathf.Abs(rotAgent.y - rotTarget.y));
        //AddReward(-Mathf.Abs(rotAgent.z));

        // Raycast 충돌 시 패널티부여, EndEpisode X
        if (Physics.Raycast(transform.position, transform.forward, out rayHit, rayDistance))
        {
            //Debug.Log("Forward Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        if (Physics.Raycast(transform.position, -transform.forward, out rayHit, rayDistance))
        {
            //Debug.Log("Backward Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        if (Physics.Raycast(transform.position, transform.right, out rayHit, rayDistance))
        {
            //Debug.Log("Right Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        if (Physics.Raycast(transform.position, -transform.right, out rayHit, rayDistance))
        {
            //Debug.Log("Left Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        if (Physics.Raycast(transform.position, transform.up, out rayHit, rayDistance))
        {
            //Debug.Log("Up Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        if (Physics.Raycast(transform.position, -transform.up, out rayHit, rayDistance))
        {
            //Debug.Log("Down Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            if (rayHit.collider.gameObject.name.Equals("Target"))
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
            else
            {
                transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.yellow;
                AddReward(-rayDistance / ((rayHit.collider.gameObject.transform.localPosition - tfAgent.localPosition).magnitude + rayDistance));
            }
        }

        // 보상처리
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;         // 행동수행후 Agent, Target간 거리
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;     // 행동수행후 UI 거리표기 갱신

        AddReward(distBefore - distAfter);      // 현재거리가 이전거리보다 짧으면 +보상, 멀면 -패널티
        distBefore = distAfter;                 // 다음행동을 위한 이전거리 갱신
        AddReward(-0.1f);                       // 지속적인 행동선택을 위한 패널티

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
        // RayCast Lidar Drawing
        Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.blue);
        Debug.DrawRay(transform.position, transform.forward * -rayDistance, Color.blue);
        Debug.DrawRay(transform.position, transform.right * rayDistance, Color.red);
        Debug.DrawRay(transform.position, transform.right * -rayDistance, Color.red);
        Debug.DrawRay(transform.position, transform.up * rayDistance, Color.green);
        Debug.DrawRay(transform.position, transform.up * -rayDistance, Color.green);

        /*
        if (Physics.Raycast(transform.position, transform.forward, out rayHit, rayDistance))
        {
            Debug.Log("Forward Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
            // if (rayHit.collider.gameObject.name.Equals("Obstacle1"))
            //    transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;                
        }

        if (Physics.Raycast(transform.position, -transform.forward, out rayHit, rayDistance))
        {
            Debug.Log("Backward Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
        }

        if (Physics.Raycast(transform.position, transform.right, out rayHit, rayDistance))
        {
            Debug.Log("Right Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
        }

        if (Physics.Raycast(transform.position, -transform.right, out rayHit, rayDistance))
        {
            Debug.Log("Left Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
        }

        if (Physics.Raycast(transform.position, transform.up, out rayHit, rayDistance))
        {
            Debug.Log("Up Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
        }

        if (Physics.Raycast(transform.position, -transform.up, out rayHit, rayDistance))
        {
            Debug.Log("Down Hit Collider GameObject Name = " + rayHit.collider.gameObject.name);
            transform.parent.Find(rayHit.collider.gameObject.name).GetComponent<Renderer>().material.color = Color.red;
        }
        */

        /*
        // Heuristic User Control Up Down
        if (Input.GetKey(KeyCode.W))
            rbAgent.AddForce(Vector3.forward * moveForce);
        if (Input.GetKey(KeyCode.S))
            rbAgent.AddForce(Vector3.back * moveForce);

        // Heuristic User Control Left Right
        if (Input.GetKey(KeyCode.D))
            rbAgent.AddForce(Vector3.right * moveForce);
        if (Input.GetKey(KeyCode.A))
            rbAgent.AddForce(Vector3.left * moveForce);

        // Heuristic User Control Back Forth
        if (Input.GetKey(KeyCode.UpArrow))
            rbAgent.AddForce(Vector3.up * moveForce);
        if (Input.GetKey(KeyCode.DownArrow))
            rbAgent.AddForce(Vector3.down * moveForce);
        */
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Collider 충돌 시 패널티부여, EndEpisode O
        //Debug.Log("Collision Occured!!! Collision Name = " + collision.collider.name);
        if (collision.collider.name.Equals("Target"))
        {
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount += 1;
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().successCount += 1;

            transform.parent.Find(collision.collider.name).GetComponent<Renderer>().material.color = Color.green;
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

            transform.parent.Find(collision.collider.name).GetComponent<Renderer>().material.color = Color.red;
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
        renderObstacle1.material.color = Color.white;
        renderObstacle2.material.color = Color.white;
        renderObstacle3.material.color = Color.white;
    }
}
