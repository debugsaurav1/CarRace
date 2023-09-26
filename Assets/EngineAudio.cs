using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudio : MonoBehaviour
{
	public AudioSource runningSound;
	public float runningMaxVolume;
	public float runningMaxPitch;

	public AudioSource reverseSound;
	public float reverseMaxVolume;
	public float reverseMaxPitch;

	public AudioSource idleSound;
	public float idleMaxVolume;
	private float speedRatio;
	private CarController carController;

	private float revLimiter;
	public float limiterSound = 1f;
	public float limiterFrequency = 3f;
	public float limiterEngageAt = 0.8f;

	public AudioSource startSound;
	public bool isEngineRunning = false;
	private void Start()
	{
		carController = GetComponent<CarController>();
		idleSound.volume = 0;
		runningSound.volume = 0;
		reverseSound.volume = 0;	
	}

	private void Update()
	{
		float speedSign = 0;
		if (carController)
		{
			speedSign = Mathf.Sign(carController.GetSpeedRatio());
			speedRatio = Mathf.Abs(carController.GetSpeedRatio());
		}
		if (speedRatio > limiterEngageAt)
		{
			revLimiter = (Mathf.Sin(Time.time * limiterFrequency)+1f) * limiterSound *(speedRatio-limiterEngageAt);
		}


		if (isEngineRunning)
		{
			idleSound.volume = Mathf.Lerp(0.1f, idleMaxVolume, speedRatio);
			if (speedSign > 0)
			{
				reverseSound.volume = 0;
				runningSound.volume = Mathf.Lerp(0.3f, runningMaxVolume, speedRatio);
				//runningSound.pitch = Mathf.Lerp(runningSound.pitch, Mathf.Lerp(0.3f, runningMaxPitch, speedRatio) + revLimiter, Time.deltaTime);
				runningSound.pitch = Mathf.Lerp(0.3f, runningMaxPitch, speedRatio);
			}
			else
			{
				runningSound.volume = 0;
				reverseSound.volume = Mathf.Lerp(0f, reverseMaxVolume, speedRatio);
				//reverseSound.pitch = Mathf.Lerp(reverseSound.pitch, Mathf.Lerp(0.2f, reverseMaxPitch, speedRatio) + revLimiter, Time.deltaTime);
				reverseSound.pitch = Mathf.Lerp(0.2f, reverseMaxPitch, speedRatio);
			}
		}
		else 
		{
			idleSound.volume = 0;
			runningSound.volume = 0;
		}
	}
	public IEnumerator startEngine() 
	{
		startSound.Play();
		carController.isEngineRunning = 1;
		yield return new WaitForSeconds(0.6f);
		isEngineRunning = true;
		yield return new WaitForSeconds(0.4f);
		carController.isEngineRunning = 2;
	}
}
