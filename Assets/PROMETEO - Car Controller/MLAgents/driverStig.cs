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

    int currentCheckpoint = 0;
    float maxDistanceToCheckpoint = 10f;

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

        Debug.Log("angular" + carController.carAngularVelocity);
        Debug.Log("distance" + distanceToNextCheckpoint(currentCheckpoint));
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

        float dist = distanceToNextCheckpoint(currentCheckpoint);
        AddReward(-dist * 0.0001f);

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
        //3 obv
        sensor.AddObservation(carController.carVelocity / carController.maxSpeed);
        //3 obv
        sensor.AddObservation(carController.carAngularVelocity / 10);

        //1 obv
        sensor.AddObservation(distanceToNextCheckpoint(currentCheckpoint) / maxDistanceToCheckpoint);
        //3 obv
        sensor.AddObservation(directionToNextCheckpoint(currentCheckpoint).normalized);
    }

    float distanceToNextCheckpoint(int CHKPT)
    {
        float currentDistance = Vector3.Distance(transform.position, checkPoints[CHKPT].transform.position);
        Debug.Log(currentDistance);
        return currentDistance;
    }
    Vector3 directionToNextCheckpoint(int CHKPT)
    {
        Vector3 checkpointDir = transform.InverseTransformPoint(checkPoints[CHKPT].transform.position);
        return checkpointDir;
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
