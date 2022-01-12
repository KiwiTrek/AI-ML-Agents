using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BallAgent : Agent
{
    public Rigidbody rBody;
    public Transform Target;
	public float forceMultiplier = 8;
	public GameObject ray;

	public GameObject[] checkPoints;
	float lolipop;
	float timer;
	void Start()
	{
		checkPoints = GameObject.FindGameObjectsWithTag("Checkpoint");
	}
	public override void OnEpisodeBegin()
    {
		lolipop = 0.0f;
		timer = 0.0f;

		foreach(var c in checkPoints)
        {
			c.SetActive(true);
        }

		// If the Agent fell, zero its momentum
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(-0.0f, 2.0f, -30.0f);

	}
	
	public override void CollectObservations(VectorSensor sensor)
	{
		// Target and Agent positions
		sensor.AddObservation(Target.localPosition);
		sensor.AddObservation(this.transform.localPosition);
		// Agent velocity
		sensor.AddObservation(rBody.velocity.x);
		sensor.AddObservation(rBody.velocity.z);
	}

    public void FixedUpdate()
    {
		Vector3 position = Vector3.zero;
		position.y -= 0.2f;
		ray.transform.localPosition = position;
		ray.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
	}

	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// Actions, size = 2
		Vector3 controlSignal = Vector3.zero;
		controlSignal.x = actionBuffers.ContinuousActions[0];
		controlSignal.z = actionBuffers.ContinuousActions[1];
		rBody.AddForce(controlSignal * forceMultiplier);

		// Fell off platform
		if (this.transform.localPosition.y < 0)
        {
			AddReward(-5.0f);
			Done();
        }

		else if (StepCount >= MaxStep)
        {
			Done();
        }
	}

    public void OnTriggerEnter(Collider other)
    {
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		if (other.gameObject.CompareTag("Finish"))
        {
			Done();
        }
		else if (other.gameObject.CompareTag("Checkpoint"))
        {
			lolipop++;
			timer = 0.0f;
			SetReward(lolipop * 0.2f);
			other.gameObject.SetActive(false);
        }
		else if (other.gameObject.CompareTag("Labyrinth"))
        {
			timer += (Time.deltaTime * 5);
			float rewardAdd = distanceToTarget * 0.2f;
			AddReward(-0.5f - rewardAdd - timer);
		}
	}

	public void Done()
    {
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		if (distanceToTarget < 4.5f)
		{
			Debug.Log("Reached Goal!");
			AddReward(10.0f);
		}

		Debug.Log("Current Reward: " + GetCumulativeReward());

		//Debug.Log("Distance: " + distanceToTarget);

		EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActionsOut = actionsOut.ContinuousActions;
		continuousActionsOut[0] = Input.GetAxis("Horizontal");
		continuousActionsOut[1] = Input.GetAxis("Vertical");
	}
}
