using UnityEngine;
using Unity.MLAgents;
using UnityEngine.PlayerLoop;
using Unity.MLAgents.Actuators;
using System;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
public class driverStig : Agent
{
    private PrometeoCarController carController;
    Vector3 StartPos;
    Quaternion StartRotation;

    GameObject[] OGcones;
    GameObject[] checkPoints;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2;
        else
            discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.D))
            discreteActions[1] = 1;
        else if (Input.GetKey(KeyCode.A))
            discreteActions[1] = 2;
        else
            discreteActions[1] = 0;

        discreteActions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        discreteActions[3] = Input.GetKey(KeyCode.B) ? 1 : 0;
    }

    public override void Initialize()
    {
        carController = GetComponent<PrometeoCarController>();
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }

    void Start()
    {
        StartPos = transform.position;
        StartRotation = Quaternion.identity;
        OGcones = GameObject.FindGameObjectsWithTag("Cone");
        checkPoints = GameObject.FindGameObjectsWithTag("Goal");
        //ResetAgent();
    }

    void ResetAgent()
    {
        //Debug.Log("Reset");
        transform.position = StartPos;
        transform.rotation = StartRotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ResetCones();
    }

    void ResetCones()
    {
        GameObject[] cones = GameObject.FindGameObjectsWithTag("Cone");
        for (int i = 0; i < cones.Length && i < OGcones.Length; i++)
        {
            cones[i].transform.position = OGcones[i].transform.position;
            cones[i].transform.rotation = OGcones[i].transform.rotation;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnActionReceived(ActionBuffers actions)
    {

        if (carController.carSpeed < 0.1f)
            AddReward(-0.0005f);

        var action = actions.DiscreteActions;

        if (action[0] == 1)
        {
            carController.CancelInvoke("DecelerateCar");
            carController.deceleratingCar = false;
            carController.GoForward();
        }
        if (action[0] == 2)
        {
            carController.CancelInvoke("DecelerateCar");
            carController.deceleratingCar = false;
            carController.GoReverse();
        }
        if (action[1] == 1)
            carController.TurnRight();
        if (action[1] == 2)
            carController.TurnLeft();

        if (action[2] == 1)
            carController.ThrottleOff();
        if (action[3] == 1)
            carController.Brakes();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.y);
        sensor.AddObservation(transform.forward.z);

        sensor.AddObservation(distanceToNextCheckpoint(0));
    }

    float distanceToNextCheckpoint(int CHKPT)
    {
        float currentDistance = Vector3.Distance(transform.position, checkPoints[CHKPT].transform.position);
        AddReward(-currentDistance * 0.0001f);
        Debug.Log(-currentDistance * 0.0001f);
        return currentDistance;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cone"))
        {
            AddReward(-1f);
            EndEpisode();
            //Debug.Log("fail");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            AddReward(1f);
            EndEpisode();
            //Debug.Log("win (trigger)");
        }
    }

}
