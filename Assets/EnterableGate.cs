﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnterableGate : Gate {
	[SerializeField] private InputActionReference enterActionReference;
	[SerializeField, Range(0.125f, 1f)]
	private float enterThreshold = 0.5f;
	[SerializeField] private UnityEvent onEnter;

	private bool canEnter = false;
	private bool isEntering = false;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger)
			canEnter = true;
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger)
			canEnter = false;
	}

	private void OnEnable() {
		if (enterActionReference == null) return;

		enterActionReference.action.performed += OnEnterInput;
	}

	private void OnDisable() {
		if (enterActionReference == null) return;

		enterActionReference.action.performed -= OnEnterInput;
	}

	private void OnEnterInput(InputAction.CallbackContext ctx) {
		if (!canEnter) return;

		Vector2 moveInput = ctx.ReadValue<Vector2>();
		if (moveInput.y > enterThreshold) {
			if (!isEntering) {
				isEntering = true;
				onEnter.Invoke();
			}
		}
		else {
			isEntering = false;
		}
	}
}
