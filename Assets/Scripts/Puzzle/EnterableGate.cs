using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnterableGate : Gate {
	[SerializeField] private InputActionReference enterActionReference;
	[SerializeField] private UnityEvent onEnter;
	[SerializeField] private UnityEvent onApproach;
	[SerializeField] private UnityEvent onLeave;

	private bool canEnter;

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
		if (enterActionReference == null) return;

		enterActionReference.action.performed += OnEnterInput;
	}

	private void OnDisable() {
		if (enterActionReference == null) return;

		enterActionReference.action.performed -= OnEnterInput;
	}

	private void OnEnterInput(InputAction.CallbackContext ctx) {
		if (!canEnter) return;

		if (ctx.phase == InputActionPhase.Performed && ctx.ReadValueAsButton())
			// Can be invoked multiple times when using an analog stick.
			onEnter.Invoke();
	}
}
