using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputActionEvent : MonoBehaviour {
	[SerializeField] private InputActionProperty action = default;
	[SerializeField] private UnityEvent onStarted = null;
	[SerializeField] private UnityEvent onPerformed = null;


	private void OnEnable() {
		action.action.started += OnStarted;
		action.action.performed += OnPerformed;
		action.action.Enable();
	}

	private void OnDisable() {
		action.action.started -= OnStarted;
		action.action.performed -= OnPerformed;
	}

	private void OnStarted(InputAction.CallbackContext ctx) {
		onStarted.Invoke();
	}

	private void OnPerformed(InputAction.CallbackContext ctx) {
		onPerformed.Invoke();
	}
}
