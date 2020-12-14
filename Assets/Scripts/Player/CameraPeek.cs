using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPeek : MonoBehaviour {
	[SerializeField, Range(0, 12)]
	private float peekDistance;

	private SnappingCamera snappingCamera;

	private void Start() {
		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	public void OnPeekInput(InputAction.CallbackContext ctx) {
		float value = ctx.ReadValue<float>();
		snappingCamera.PeekOffset = value != 0
			? new Vector3(0, peekDistance * value, 0)
			: Vector3.zero;
	}
}