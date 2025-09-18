using UnityEngine;
using Unity.MLAgents;
using UnityEngine.PlayerLoop;
using Unity.MLAgents.Actuators;
using System;
using Unity.MLAgents.Sensors;
public class driverStig : Agent
{
    private PrometeoCarController carController;
    Vector3 StartPos;
    GameObject[] OGcones;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        Debug.Log("Heuristic called");

        // Action[0] = forward/reverse
        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 1;   // GoForward
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2;   // GoReverse
        else
            discreteActions[0] = 0;   // no throttle

        // Action[1] = steering
        if (Input.GetKey(KeyCode.D))
            discreteActions[1] = 1;   // TurnRight
        else if (Input.GetKey(KeyCode.A))
            discreteActions[1] = 2;   // TurnLeft
        else
            discreteActions[1] = 0;   // straight

        // Action[2] = throttle off
        discreteActions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        // Action[3] = brakes
        discreteActions[3] = Input.GetKey(KeyCode.B) ? 1 : 0;
    }

    public override void Initialize()
    {
        carController = GetComponent<PrometeoCarController>();
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    void Start()
    {
        StartPos = transform.position;
        OGcones = GameObject.FindGameObjectsWithTag("Cone");
        Reset();
    }

    void Reset()
    {
        transform.position = StartPos;
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

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     sensor.AddObservation(carController.carSpeed);
    //     sensor.AddObservation(transform.forward);
    //     sensor.AddObservation(transform.right);


    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, transform.forward, out hit, 20f))
    //     {
    //         sensor.AddObservation(hit.distance / 20f);
    //     }
    //     else
    //     {
    //         sensor.AddObservation(1f);
    //     }
    // }

    public void Update()
    {
        // Debug.Log(carController.carSpeed);
        // Debug.Log(carController.isDrifting);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cone"))
        {
            AddReward(-1f);
            EndEpisode();
            // Debug.Log("fail");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            AddReward(1f);
            EndEpisode();
            // Debug.Log("win (trigger)");
        }
    }

}
