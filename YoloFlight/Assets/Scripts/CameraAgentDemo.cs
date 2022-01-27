using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CameraAgentDemo : Agent
{
    private Transform tfAgent;
    private Rigidbody rbAgent;
    private Transform tfTarget;

    private float addForce = 25.0f;
    private float rotSpeed = 25.0f;

    private float distAfter;
    private float distBefore;
    private float limitDistance = 15.0f;

    private Renderer renderTarget;

    private Camera camera1;
    private Camera camera2;
    private Camera camera3;
    private Camera camera4;
    private Camera camera5;
    private Camera camera6;

    // Start is called before the first frame update
    public override void Initialize()
    {
        MaxStep = 1000;
        tfAgent = GetComponent<Transform>();
        rbAgent = GetComponent<Rigidbody>();
        tfTarget = transform.parent.Find("Target").gameObject.GetComponent<Transform>();
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
        // Agent ������ �ʱ�ȭ
        rbAgent.velocity = Vector3.zero;
        rbAgent.angularVelocity = Vector3.zero;
        tfAgent.localEulerAngles = new Vector3(0, 0, 0);

        // Agent & Target & Obstacle �ʱ���ġ ������ ����
        tfAgent.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        tfTarget.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));
        //foreach (Transform child in GameObject.Find("Obstacles").transform)
        //    child.localPosition = new Vector3(Random.Range(-45.0f, 45.0f), Random.Range(10.0f, 90.0f), Random.Range(-45.0f, 45.0f));

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

        foreach (Transform child in GameObject.Find("Map").transform)
        {
            // ù��° ī�޶� ���� ������Ʈ
            Vector3 viewPos1 = camera1.WorldToViewportPoint(child.position);
            if (0 <= viewPos1.x && viewPos1.x <= 1 && 0 <= viewPos1.y && viewPos1.y <= 1 && 0 < viewPos1.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }
            // �ι�° ī�޶� ���� ������Ʈ
            Vector3 viewPos2 = camera2.WorldToViewportPoint(child.position);
            if (0 <= viewPos2.x && viewPos2.x <= 1 && 0 <= viewPos2.y && viewPos2.y <= 1 && 0 < viewPos2.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }
            // ����° ī�޶� ���� ������Ʈ
            Vector3 viewPos3 = camera3.WorldToViewportPoint(child.position);
            if (0 <= viewPos3.x && viewPos3.x <= 1 && 0 <= viewPos3.y && viewPos3.y <= 1 && 0 < viewPos3.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }
            // �׹�° ī�޶� ���� ������Ʈ
            Vector3 viewPos4 = camera4.WorldToViewportPoint(child.position);
            if (0 <= viewPos4.x && viewPos4.x <= 1 && 0 <= viewPos4.y && viewPos4.y <= 1 && 0 < viewPos4.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }
            // �ټ���° ī�޶� ���� ������Ʈ
            Vector3 viewPos5 = camera5.WorldToViewportPoint(child.position);
            if (0 <= viewPos5.x && viewPos5.x <= 1 && 0 <= viewPos5.y && viewPos5.y <= 1 && 0 < viewPos5.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }
            // ������° ī�޶� ���� ������Ʈ
            Vector3 viewPos6 = camera6.WorldToViewportPoint(child.position);
            if (0 <= viewPos6.x && viewPos6.x <= 1 && 0 <= viewPos6.y && viewPos6.y <= 1 && 0 < viewPos6.z)
            {
                // �����Ÿ����� ������ ����/���Ƽ
                float distance = (tfAgent.localPosition - GameObject.Find(child.name).transform.localPosition).magnitude;
                if (distance < limitDistance)
                    AddReward((limitDistance - distance) / limitDistance);
            }

        }

        // ����ó��
        distAfter = (tfAgent.localPosition - tfTarget.localPosition).magnitude;         // �ൿ������ Agent, Target�� �Ÿ�
        GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().distance = distAfter;     // �ൿ������ UI �Ÿ�ǥ�� ����

        AddReward((distBefore - distAfter) * 10.0f);    // ����Ÿ��� �����Ÿ����� ª���� +����, �ָ� -�г�Ƽ
        distBefore = distAfter;                         // �����ൿ�� ���� �����Ÿ� ����
        AddReward(-0.01f);                              // �������� �ൿ������ ���� �г�Ƽ
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

    private void OnCollisionEnter(Collision collision)
    {
        // Collider �浹 �� �г�Ƽ�ο�, EndEpisode O
        Debug.Log("Collision Occured!!! Collision Name = " + collision.collider.name);
        if (collision.collider.name.Equals("Target"))
        {
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().totalCount += 1;
            GameObject.Find("MonitoringUI").GetComponent<MonitoringUI>().successCount += 1;

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
        renderTarget.material.color = Color.blue;
    }
}
