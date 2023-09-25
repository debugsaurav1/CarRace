using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
	private Rigidbody playerRB;
	public WheelColliders colliders;
	public WheelMeshes wheelMeshes;
	public WheelParticles wheelParticles;
	public float gasInput;
	public float brakeInput;
	public float steeringInput;
	public GameObject smokePrefab;
	public float motorPower;
	public float brakePower;
	public float slipAngle;
	private float speed;
	private float speedClamped;
	public AnimationCurve steeringCurve;
	public float maxSpeed;

	public float RPM;
	public float idleRPM; //idle state rpm
	public float redLine; // max point to cutoff power for the gear
	public TMP_Text rpmText;
	public TMP_Text gearText;
	public Transform rpmNeedle;
	public int currentGear;
	public float minNeedlerotation;
	public float maxNeedlerotation;


	public int isEngineRunning;
	// Start is called before the first frame update
	void Start()
	{
		playerRB = gameObject.GetComponent<Rigidbody>();
		InstantiateSmoke();
	}

	void InstantiateSmoke()
	{
		wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
			.GetComponent<ParticleSystem>();
		wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
			.GetComponent<ParticleSystem>();
		wheelParticles.RRWheel = Instantiate(smokePrefab, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
			.GetComponent<ParticleSystem>();
		wheelParticles.RLWheel = Instantiate(smokePrefab, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
			.GetComponent<ParticleSystem>();
	}
	// Update is called once per frame

	void Update()
	{
		rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedlerotation, maxNeedlerotation, RPM / redLine));
		rpmText.text = "RPM: "+RPM;
		gearText.text = (currentGear + 1).ToString();
		//speed = playerRB.velocity.magnitude;
		speed = colliders.RRWheel.rpm * colliders.RRWheel.radius * 2f * Mathf.PI / 10f;
		speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);
		CheckInput();
		ApplyMotor();
		ApplySteering();
		ApplyBrake();
		CheckParticles();
		ApplyWheelPositions();
	}

	void CheckInput()
	{
		gasInput = Input.GetAxis("Vertical");
		if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
		{
			StartCoroutine(GetComponent<EngineAudio>().startEngine());
		}
		steeringInput = Input.GetAxis("Horizontal");
		
		slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
		float movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);
		if (movingDirection < -0.5f && gasInput > 0)
		{
			brakeInput = Mathf.Abs(gasInput);
		}
		else if (movingDirection > 0.5f && gasInput < 0)
		{
			brakeInput = Mathf.Abs(gasInput);
		}
		else
		{
			brakeInput = 0;
		}

	}
	void ApplyBrake()
	{
		colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
		colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;

		colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
		colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;


	}
	void ApplyMotor()
	{
		if (isEngineRunning > 1)
		{
			if (Mathf.Abs(speed) < maxSpeed)
			{
				colliders.RRWheel.motorTorque = motorPower * gasInput;
				colliders.RLWheel.motorTorque = motorPower * gasInput;
			}
			else
			{
				colliders.RRWheel.motorTorque = 0f;
				colliders.RLWheel.motorTorque = 0f;
			}
		}	
	}
	void ApplySteering()
	{

		float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);
		if (slipAngle < 120f)
		{
			steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
		}
		steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
		colliders.FRWheel.steerAngle = steeringAngle;
		colliders.FLWheel.steerAngle = steeringAngle;
	}

	void ApplyWheelPositions()
	{
		UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
		UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
		UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
		UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
	}
	void CheckParticles()
	{
		WheelHit[] wheelHits = new WheelHit[4];
		colliders.FRWheel.GetGroundHit(out wheelHits[0]);
		colliders.FLWheel.GetGroundHit(out wheelHits[1]);

		colliders.RRWheel.GetGroundHit(out wheelHits[2]);
		colliders.RLWheel.GetGroundHit(out wheelHits[3]);

		float slipAllowance = 0.5f;
		if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
		{
			wheelParticles.FRWheel.Play();
		}
		else
		{
			wheelParticles.FRWheel.Stop();
		}
		if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
		{
			wheelParticles.FLWheel.Play();
		}
		else
		{
			wheelParticles.FLWheel.Stop();
		}
		if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
		{
			wheelParticles.RRWheel.Play();
		}
		else
		{
			wheelParticles.RRWheel.Stop();
		}
		if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
		{
			wheelParticles.RLWheel.Play();
		}
		else
		{
			wheelParticles.RLWheel.Stop();
		}


	}
	void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
	{
		Quaternion quat;
		Vector3 position;
		coll.GetWorldPose(out position, out quat);
		wheelMesh.transform.position = position;
		wheelMesh.transform.rotation = quat;
	}

	public float GetSpeedRatio()
	{
		var gas = Mathf.Clamp(Mathf.Abs(gasInput), 0.5f, 1f);
		return speedClamped * gas / maxSpeed;
	}
}
[System.Serializable]
public class WheelColliders
{
	public WheelCollider FRWheel;
	public WheelCollider FLWheel;
	public WheelCollider RRWheel;
	public WheelCollider RLWheel;
}
[System.Serializable]
public class WheelMeshes
{
	public MeshRenderer FRWheel;
	public MeshRenderer FLWheel;
	public MeshRenderer RRWheel;
	public MeshRenderer RLWheel;
}
[System.Serializable]
public class WheelParticles
{
	public ParticleSystem FRWheel;
	public ParticleSystem FLWheel;
	public ParticleSystem RRWheel;
	public ParticleSystem RLWheel;

}