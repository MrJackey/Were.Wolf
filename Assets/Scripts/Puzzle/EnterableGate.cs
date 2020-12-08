using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnterableGate : Gate {
	[SerializeField] private InputActionReference enterActionReference;
	[SerializeField, Range(0.125f, 1f)]
	private float enterThreshold = 0.5f;
	[SerializeField] private UnityEvent onEnter;
	[SerializeField] private UnityEvent onApproach;
	[SerializeField] private UnityEvent onLeave;

	private bool canEnter = false;
	private bool isEntering = false;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger) {
			canEnter = true;
			onApproach.Invoke();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger) {
			canEnter = false;
			onLeave.Invoke();
		}
	}

	private void OnEnable() {
		base.AddInternalListeners();
		if (enterActionReference == null) return;

		enterActionReference.action.performed += OnEnterInput;
	}

	private void OnDisable() {
		base.RemoveInternalListeners();
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
