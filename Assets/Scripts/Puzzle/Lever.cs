using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : SignalEmitter {
	[SerializeField] private LeverType leverType = LeverType.Toggle;
	[SerializeField] float timerEnd = 5f;

	private Animator animator;
	private float leverTimer, timerStart = 0f;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void Update() {
		if (IsActivated && leverType == LeverType.Timed) {
			leverTimer += Time.deltaTime;
			
			if (leverTimer >= timerEnd)
				Deactivate();
		}
	}

	public void ToggleActivation() {
		if (!IsActivated) 
			Activate();
		else
			Deactivate();
	}

	public void Activate() {
		animator.SetBool("Activation", true);
		IsActivated = true; 

		if (leverType == LeverType.Timed) {
			leverTimer = timerStart;
		}
	}

	public void Deactivate() {
		animator.SetBool("Activation", false);
		IsActivated = false;
	}
}

public enum LeverType {
	Timed,
	Toggle,
}