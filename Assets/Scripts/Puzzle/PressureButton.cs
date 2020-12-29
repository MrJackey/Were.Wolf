using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureButton : SignalEmitter {
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private Animator animator;
	
	public List<Collider2D> overlappingColliders = new List<Collider2D>();


	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void OnTriggerEnter2D(Collider2D other) {	
		if (!other.isTrigger && (other.attachedRigidbody.CompareTag("Player") || other.attachedRigidbody.CompareTag("Box"))) {
			overlappingColliders.Add(other);
			PressureButtonDown();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		overlappingColliders.Remove(other);
	
		if (overlappingColliders.Count == 0) {
			PressureButtonUp();
		}
	}

	private void PressureButtonDown() {
		PlaySound();
		animator.SetBool("IsPressed", true);
		IsActivated = true;
	}

	private void PressureButtonUp() {
		PlaySound();
		animator.SetBool("IsPressed", false);
		IsActivated = false;
	}

	public void PlaySound() {
		audioSource.Play();
	}
}