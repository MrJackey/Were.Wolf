using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputActionEvent : MonoBehaviour {
	[SerializeField] private InputActionProperty action = default;
	[SerializeField] private UnityEvent onStarted = null;

	private void OnEnable() {
		action.action.started += OnStarted;
		action.action.Enable();
	}

	private void OnDisable() {
		action.action.started -= OnStarted;
	}

	private void OnStarted(InputAction.CallbackContext ctx) {
		onStarted.Invoke();
	}
}