using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureButton : SignalEmitter {
	[SerializeField] BoxCollider2D colliderUp;
	[SerializeField] private AudioSource audioSource;

	private Animator animator;

	public float delayTimer, delayEnd = 0.5f;
	private float delayStart = 0;
	private bool isDelayButtonUp = false, isDelayButtonDown = false;

	public List<Collider2D> overlappingColliders = new List<Collider2D>();


	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void Update() {
		if (isDelayButtonDown && !IsActivated) {
			delayTimer += Time.deltaTime;
			PressureButtonDown();
		}
		else if (isDelayButtonUp && IsActivated) {
			delayTimer += Time.deltaTime;
			PressureButtonUp();
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {	
		if (!other.isTrigger && (other.attachedRigidbody.CompareTag("Player") || other.attachedRigidbody.CompareTag("Box"))) {
			overlappingColliders.Add(other);
			delayTimer = delayStart;
			isDelayButtonUp = false;
			isDelayButtonDown = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		overlappingColliders.Remove(other);
	
		if (overlappingColliders.Count == 0) {
			delayTimer = delayStart;
			isDelayButtonDown = false;
			isDelayButtonUp = true;
		}
	}

	private void PressureButtonDown() {
		if (delayTimer > delayEnd) {
			PlaySound();
			colliderUp.enabled = false;
			animator.SetBool("IsPressed", true);
			IsActivated = true;
			isDelayButtonDown = false;
		}
	}

	private void PressureButtonUp() {
		if (delayTimer > delayEnd) {
			PlaySound();
			colliderUp.enabled = true;
			animator.SetBool("IsPressed", false);
			IsActivated = false;
			isDelayButtonUp = false;
		}
	}

	public void PlaySound() {
		audioSource.Play();
	}
}