using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : SignalEmitter {
	[SerializeField] private LeverType leverType = LeverType.Toggle;
	[SerializeField] float timerEnd = 5f, fastTimeAudioStart;
	[SerializeField] private AudioSource pullLeverAudio, slowTimeAudio, fastTimeAudio;

	private Animator animator;
	private float leverTimer, timerStart = 0f;
	private bool hasStartedFastTimeSound;

	private void Start() {
		animator = GetComponent<Animator>();
		fastTimeAudioStart = timerEnd * 0.75f;
		hasStartedFastTimeSound = false;
	}

	private void Update() {
		if (IsActivated && leverType == LeverType.Timed) {
			leverTimer += Time.deltaTime;
			
			if (leverTimer >= timerEnd)
				Deactivate();
		}

		if (!hasStartedFastTimeSound && leverType == LeverType.Timed   // Should this maybe be reordered to test for the leverType first instead? maybe it acts like a boolean?
								&& leverTimer >= fastTimeAudioStart) {
			slowTimeAudio.Stop();
			TimedLeverPlaySound();
			hasStartedFastTimeSound = true;
		}
	}

	public void ToggleActivation() {
		PullLeverPlaySound();
		if (IsActivated) 
			Deactivate();
		else
			Activate();
	}

	public void Activate() {
		if (leverType == LeverType.Timed)
			TimedLeverPlaySound();

		animator.SetBool("Activation", true);
		IsActivated = true; 
	}

	public void Deactivate() {
		animator.SetBool("Activation", false);
		IsActivated = false;

		if (leverType == LeverType.Timed) {
			slowTimeAudio.Stop();
			fastTimeAudio.Stop();
			leverTimer = timerStart;
			hasStartedFastTimeSound = false;
		}
	}

	private void PullLeverPlaySound() {
		pullLeverAudio.Play();
	}

	private void TimedLeverPlaySound() {
		if (leverTimer < fastTimeAudioStart)
			slowTimeAudio.Play();
		else if (leverTimer >= fastTimeAudioStart)
			fastTimeAudio.Play();
	}
}

public enum LeverType {
	Timed,
	Toggle,
}