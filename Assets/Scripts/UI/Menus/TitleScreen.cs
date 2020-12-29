using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TitleScreen : MonoBehaviour {
	private static bool hasShown;

	[SerializeField] private UnityEvent onDismiss;

	private void Start() {
		if (hasShown)
			Hide();

		hasShown = true;
	}

	private void Update() {
		if (IsAnyButtonDown())
			Hide();
	}

	private static bool IsAnyButtonDown() {
		return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
		       Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
		       Gamepad.current != null && Gamepad.current.allControls.Any(control => control is ButtonControl button &&
		                                                                             button.wasPressedThisFrame);
	}

	private void Hide() {
		gameObject.SetActive(false);
		onDismiss.Invoke();
	}
}