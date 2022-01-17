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

	Vector3 lastPosition;
	Vector3 currentDirection;
    public void Start()
	{
		Debug.Log("Started Simulator");
	}
	public override void OnEpisodeBegin()
	{
		this.rBody.angularVelocity = Vector3.zero;
		this.rBody.velocity = Vector3.zero;

		int posA = Random.Range(-3, 3);
		int posB = Random.Range(-3, 3);

		//Position Correction
		if (posA == 2) posA = 3;
		else if (posA == -2) posA = -3;

		if (posB == 0) posB = -1;
		else if (posB == -2) posB = -3;
		else if (posB == 2) posB = 3;

		if (posA == 0 && posB == 1) posA = 1;
		else if (posA == -1 && posB == 1) posA = -3;

		this.transform.localPosition = new Vector3(posA * 5.0f, 1.5f, posB * 5.0f);

		this.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		lastPosition = this.transform.localPosition;
		currentDirection = Vector3.zero;
		ray.transform.rotation = Quaternion.LookRotation(currentDirection);
	}

	public override void CollectObservations(VectorSensor sensor)
	{
        // Agent positions
        sensor.AddObservation(this.transform.localPosition);
		sensor.AddObservation(Target.localPosition);

		// Agent velocity
		sensor.AddObservation(rBody.velocity.x);
		sensor.AddObservation(rBody.velocity.z);
	}

	public void FixedUpdate()
	{
		currentDirection = (this.transform.localPosition - lastPosition).normalized;
		lastPosition = this.transform.localPosition;
		if (currentDirection != Vector3.zero)
		{
			ray.transform.rotation = Quaternion.Slerp(ray.transform.rotation, Quaternion.LookRotation(currentDirection), Time.deltaTime * 75);
        }

		if (StepCount >= MaxStep)
		{
			Debug.Log("Max Step Reached");
			EndEpisode();
		}
	}

	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// Actions, size = 2
		Vector3 controlSignal = Vector3.zero;
		controlSignal.x = actionBuffers.ContinuousActions[0];
		controlSignal.z = actionBuffers.ContinuousActions[1];
		rBody.AddForce(controlSignal * forceMultiplier);

		SetReward(-1 / MaxStep);

		if (StepCount >= MaxStep)
		{
			Debug.Log("Max Step Reached");
			SetReward(-1.0f);
			EndEpisode();
		}
		else if (this.transform.localPosition.y < 0)
		{
			SetReward(-1.0f);
			EndEpisode();
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		if (other.gameObject.CompareTag("Finish"))
		{
			SetReward(1.0f);
			EndEpisode();
		}
        else if (other.gameObject.CompareTag("Labyrinth"))
		{
			Debug.Log("Wall collision");
			SetReward(-1.0f);
		}
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActionsOut = actionsOut.ContinuousActions;
		continuousActionsOut[0] = Input.GetAxis("Horizontal");
		continuousActionsOut[1] = Input.GetAxis("Vertical");
	}
}
