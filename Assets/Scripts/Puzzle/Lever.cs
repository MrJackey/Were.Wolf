using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : SignalEmitter
{
	private Animator animator;

	public bool isActive = false;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	public void ActivateLever() {
		isActive = true; // <-------------------OBS! check this, believe this can be removed
		animator.SetBool("Activation", true);
		IsActivated = true; 
	}

	public void DeactivateLever() {
		isActive = false; // <-------------------OBS! check this, believe this can be removed
		animator.SetBool("Activation", false);
		IsActivated = false;
	}
}
