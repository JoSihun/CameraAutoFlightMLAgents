using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DroneAgent : Agent
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
    private float rayAngle = 10.0f;         // ���� LiDAR ����
    private float rayDistance = 10.0f;      // �����¿� LiDAR ����
    private float rayDistance2 = 25.0f;     // ���� LiDAR ����
    private float rayDiameter = 10.0f;      // ���� LiDAR ����

    private Renderer renderGround;
    private Renderer renderTarget;

    // �ʱ�ȭ �۾��� ���� �ѹ� ȣ��Ǵ� �޼ҵ�
    public override void Initialize()
    {
        MaxStep = 2000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();

        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
        tfObstacles = transform.parent.Find("Obstacles").gameObject.GetComponent<Transform>();


        // �÷�������, �׽�Ʈȯ�濡�� �����Ǵ� �ڵ�
        renderGround = transform.parent.Find("Ground").gameObject.GetComponent<Renderer>();
        renderTarget = transform.parent.Find("Target").gameObject.GetComponent<Renderer>();
    }

    // ���Ǽҵ尡 ������ ������ ȣ��
    public override void OnEpisodeBegin()
    {
        // Agent ������ �� ȸ���� �ʱ�ȭ
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;
        tfAgent.eulerAngles = Vector3.zero;

        // Agnet & Target ��ġ ��������
        //tfAgent.localPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(-50.0f, 50.0f));
        //tfTarget.localPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(-50.0f, 50.0f));
        tfAgent.localPosition = new Vector3(0.0f, 50.0f, -25.0f);
        tfTarget.localPosition = new Vector3(0.0f, 50.0f, 1025.0f);

        /*
        // Obstacle ��ġ ��������
        for (int i = 0; i < tfObstacles.childCount; i++)
        {
            int selectRotation = Random.Range(1, 4);
            while (true)
            {
                Vector3 randomPosition = new Vector3();
                if (selectRotation == 1)
                {
                    // ��ֹ��� ������ ��
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 0);
                    randomPosition = new Vector3(Random.Range(-50.0f, 50.0f), 50.0f, Random.Range(0.0f, 900.0f));
                }
                if (selectRotation == 2)
                {
                    // ��ֹ��� ��������� ��
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(90, 0, 0);
                    randomPosition = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(0.0f, 100.0f), Random.Range(50.0f, 900.0f));
                }
                if (selectRotation == 3)
                {
                    // ��ֹ��� ��������� ��
                    tfObstacles.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 90);
                    randomPosition = new Vector3(0.0f, Random.Range(0.0f, 100.0f), Random.Range(50.0f, 900.0f));
                }

                // Agent & Target�� ��ħ ����
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


        // ���� ����� ���� Agent�� Target�� ���⺤�� �� �ʱ�Ÿ�
        distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);


        // �׽�Ʈȯ�濡�� �����Ǵ� �ڵ�
        // Material �÷� �ʱ�ȭ, ����Ȯ���� ���� �ڷ�ƾ ������, 
        StartCoroutine(RevertMaterial());
    }

    // ȯ������ ���� �� ����, ��å������ ���� �극�ο� ����
    public override void CollectObservations(VectorSensor sensor)
    {
        // �� ������ 12 + 9 = 21��
        sensor.AddObservation(tfAgent.localPosition);       // ������ 3��(x, y, z), Agent Position
        sensor.AddObservation(tfTarget.localPosition);      // ������ 3��(x, y, z), Target Position

        sensor.AddObservation(rbAgent.velocity);            // ������ 3��(x, y, z), Agent Velocity
        sensor.AddObservation(rbAgent.angularVelocity);     // ������ 3��(x, y, z), Agent Angular Velocity

        /*
        sensor.AddObservation(RayObservation(-90f, 0f, 0f, rayDistance));          // ������ 1��(float), Detected Obeject Distance for Upward
        sensor.AddObservation(RayObservation(90f, 0f, 0f, rayDistance));           // ������ 1��(float), Detected Obeject Distance for Downward
        sensor.AddObservation(RayObservation(0f, -90f, 0f, rayDistance));          // ������ 1��(float), Detected Obeject Distance for Leftward
        sensor.AddObservation(RayObservation(0f, 90f, 0f, rayDistance));           // ������ 1��(float), Detected Obeject Distance for Rightward

        sensor.AddObservation(RayObservation(0f, 0f, 0f, rayDistance2));            // ������ 1��(float), Detected Obeject Distance for Forward
        sensor.AddObservation(RayObservation(-rayAngle, 0f, 0f, rayDistance2));     // ������ 1��(float), Detected Obeject Distance for ForwardU
        sensor.AddObservation(RayObservation(rayAngle, 0f, 0f, rayDistance2));      // ������ 1��(float), Detected Obeject Distance for ForwardD
        sensor.AddObservation(RayObservation(0f, -rayAngle, 0f, rayDistance2));     // ������ 1��(float), Detected Obeject Distance for ForwardL
        sensor.AddObservation(RayObservation(0f, rayAngle, 0f, rayDistance2));      // ������ 1��(float), Detected Obeject Distance for ForwardR
        */

        //sensor.AddObservation(RayObservation(-90f, 0f, 0f));     // ������ 1��(float), Detected Obeject Distance for Upward
        //sensor.AddObservation(RayObservation(90f, 0f, 0f));      // ������ 1��(float), Detected Obeject Distance for Downward
        //sensor.AddObservation(RayObservation(0f, -90f, 0f));     // ������ 1��(float), Detected Obeject Distance for Leftward
        //sensor.AddObservation(RayObservation(0f, 90f, 0f));      // ������ 1��(float), Detected Obeject Distance for Rightward

        //sensor.AddObservation(RayObservation(0f, 0f, 0f));       // ������ 1��(float), Detected Obeject Distance for Forward
        //sensor.AddObservation(RayObservation(-25f, 0f, 0f));     // ������ 1��(float), Detected Obeject Distance for ForwardU
        //sensor.AddObservation(RayObservation(25f, 0f, 0f));      // ������ 1��(float), Detected Obeject Distance for ForwardD
        //sensor.AddObservation(RayObservation(0f, -25f, 0f));     // ������ 1��(float), Detected Obeject Distance for ForwardL
        //sensor.AddObservation(RayObservation(0f, 25f, 0f));      // ������ 1��(float), Detected Obeject Distance for ForwardR

    }

    // �극��(��å)���κ��� ���޹��� �ൿ�� �����ϴ� �޼ҵ�
    public override void OnActionReceived(ActionBuffers actions)
    {
        // �̵� �ൿ ����
        float positionX = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        float positionY = Mathf.Clamp(actions.ContinuousActions[1], -1.0f, 1.0f);
        float positionZ = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);
        //float rotationY = Mathf.Clamp(actions.ContinuousActions[2], -1.0f, 1.0f);

        // �̵� ���� ����
        Vector3 directionP = Vector3.right * positionX + Vector3.up * positionY + Vector3.forward * positionZ;      // ������ǥ(�������)
        //Vector3 directionP = tfAgent.right * positionX + tfAgent.up * positionY + tfAgent.forward * positionZ;      // ������ǥ(��б���)

        // �̵� �ൿ ����(�ӵ�����)
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
            // �̵� ����ó��
            distAfter = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);        // �ൿ������ Agent, Target�� �Ÿ�
            AddReward(distBefore - distAfter);
            distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);
        }
        */

        distAfter = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);        // �ൿ������ Agent, Target�� �Ÿ�
        AddReward((distBefore - distAfter) * 10f);
        distBefore = Vector3.Distance(tfAgent.localPosition, tfTarget.localPosition);

        // �������� �ൿ������ ���� �г�Ƽ
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

    // �����ڰ� �����ൿ �׽�Ʈ
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
        yield return new WaitForSeconds(0.3f);  // ������ �ð�

        // Floor & Target & Obstacle Rendering Initialize
        renderGround.material.color = Color.white;
        renderTarget.material.color = Color.blue;

    }

    void Start()
    {

    }

    
    void Update()
    {
        // ���-��ǥ���� ȸ�� �� Ray ǥ��
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
