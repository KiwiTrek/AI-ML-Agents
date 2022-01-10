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
	public float forceMultiplier = 10;
	public float time = 0.0f;


	private GameObject[] checkpoints;
	private int checkCounter = 0;
	private int bestCheckCounter = 0;
	private int hit = 0;

	void Start()
    {
		checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
		checkCounter = 0;
		bestCheckCounter = 0;
		hit = 0;
	}
    public override void OnEpisodeBegin()
    {
		// If the Agent fell, zero its momentum
		SetReward(0.0f);

		time = 0.0f;

        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3( -15.0f, 1.0f, -35.0f );

		foreach(var g in checkpoints)
        {
			g.SetActive(true);
        }

		checkCounter = 0;
		time = 0;
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
	
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// Actions, size = 2
		Vector3 controlSignal = Vector3.zero;
		controlSignal.x = actionBuffers.ContinuousActions[0];
		controlSignal.z = actionBuffers.ContinuousActions[1];
		rBody.AddForce(controlSignal * forceMultiplier);

		// Rewards
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		time += Time.deltaTime;

		// Reached target
		if (distanceToTarget < 1.42f)
		{
			Done();
		}

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
			Done();
        }

		else if (time >= 10)
        {
			SetReward(-time);
			Done();
        }
	}

    public void OnTriggerEnter(Collider other)
    {
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		if (other.gameObject.CompareTag("Labyrinth"))
        {
			float reward = -10.0f + hit;
			SetReward(reward);
			hit++;
			Done();
        }
		else if (other.gameObject.CompareTag("Checkpoint"))
        {
			hit = 0;
			time = 0;
			other.gameObject.SetActive(false);

			checkCounter++;
			float reward = 1.0f;
			reward *= checkCounter;
			SetReward(reward);

			if (bestCheckCounter < checkCounter)
            {
				bestCheckCounter = checkCounter;
            }

			Debug.Log("Barrier Counter: " + checkCounter
				+ " || High Score: " + bestCheckCounter);
        }
    }

	public void Done()
    {
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		if (distanceToTarget < 1.42f)
		{
			SetReward(250.0f);
		}

		Debug.Log("Distance: " + distanceToTarget);

		EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActionsOut = actionsOut.ContinuousActions;
		continuousActionsOut[0] = Input.GetAxis("Horizontal");
		continuousActionsOut[1] = Input.GetAxis("Vertical");
	}
}
