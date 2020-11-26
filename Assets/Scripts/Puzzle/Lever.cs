using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : SignalEmitter {
	private Animator animator;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	public void ActivateLever() {
		animator.SetBool("Activation", true);
		IsActivated = true; 
	}

	public void DeactivateLever() {
		animator.SetBool("Activation", false);
		IsActivated = false;
	}
}
