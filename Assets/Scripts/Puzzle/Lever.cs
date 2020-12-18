using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : SignalEmitter {
	[SerializeField] private LeverType leverType = LeverType.Toggle;
	[SerializeField] private float timerEnd = 5f;
	[SerializeField, Range(0f, 1f)] private float fastTimeAudioStartAdjust = 0.75f;
	[SerializeField] private AudioSource pullLeverAudio, slowTimeAudio, fastTimeAudio;

	private Animator animator;
	private float leverTimer, timerStart = 0f, fastTimeAudioStart;
	private bool hasStartedFastTimeSound;

	private void Start() {
		animator = GetComponent<Animator>();
		fastTimeAudioStart = timerEnd * fastTimeAudioStartAdjust;
	}

	private void Update() {
		if (IsActivated && leverType == LeverType.Timed) {
			leverTimer += Time.deltaTime;
			
			if (leverTimer >= timerEnd)
				Deactivate();
		}

		if (!hasStartedFastTimeSound && leverType == LeverType.Timed && leverTimer >= fastTimeAudioStart) {
			slowTimeAudio.Stop();
			PlayTimedLeverSound();
			hasStartedFastTimeSound = true;
		}
	}

	public void ToggleActivation() {
		PlayPullLeverSound();
		if (IsActivated) 
			Deactivate();
		else
			Activate();
	}

	public void Activate() {
		if (leverType == LeverType.Timed)
			PlayTimedLeverSound();

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

	private void PlayPullLeverSound() {
		pullLeverAudio.Play();
	}

	private void PlayTimedLeverSound() {
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
