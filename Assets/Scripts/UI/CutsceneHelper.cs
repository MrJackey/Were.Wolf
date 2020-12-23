using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class CutsceneHelper : MonoBehaviour {
	private static readonly int skipHash = Animator.StringToHash("Skip");

	[SerializeField] private Image skipInfo;
	[SerializeField] private float skipStayDuration = 1.5f;
	[Space]
	[SerializeField] private UnityEvent onCutsceneStart;
	[SerializeField] private UnityEvent onCutsceneEnd;

	private SimpleTimer skipInfoFadeTimer;
	private Animator animator;
	private bool isSkipped = false;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void Update() {
		if (isSkipped) return;

		if (IsAnyButtonDown()) {
			skipInfoFadeTimer.Reset(skipStayDuration);
			skipInfo.enabled = true;
		}
		else if (skipInfoFadeTimer.Tick())
			skipInfo.enabled = false;
	}

	private bool IsAnyButtonDown() {
		return Keyboard.current != null && Keyboard.current.anyKey.isPressed ||
		       Mouse.current != null && Mouse.current.leftButton.isPressed ||
		       Gamepad.current != null && Gamepad.current.allControls.Any(control => control is ButtonControl button &&
			       button.isPressed);
	}

	public void HandleSkipCutsceneInput() {
		animator.SetTrigger(skipHash);
		isSkipped = true;
		skipInfo.enabled = false;
	}

	#region Animation events

	private void HandleCutsceneStart() {
		onCutsceneStart.Invoke();
	}

	private void HandleCutsceneEnd() {
		onCutsceneEnd.Invoke();
		Destroy(gameObject);
	}

	#endregion
}
