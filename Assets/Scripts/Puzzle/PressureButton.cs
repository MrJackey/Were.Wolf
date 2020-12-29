using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureButton : SignalEmitter {
	private static readonly int pressHash = Animator.StringToHash("press");
	private static readonly int releaseHash = Animator.StringToHash("release");

	[SerializeField] private AudioSource audioSource;
	[SerializeField] private Animator animator;

	public List<Collider2D> overlappingColliders = new List<Collider2D>();

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
		if (IsActivated) return;
		PlaySound();
		animator.SetTrigger(pressHash);
		IsActivated = true;
	}

	private void PressureButtonUp() {
		if (!IsActivated) return;
		PlaySound();
		animator.SetTrigger(releaseHash);
		IsActivated = false;
	}

	public void PlaySound() {
		audioSource.Play();
	}
}
