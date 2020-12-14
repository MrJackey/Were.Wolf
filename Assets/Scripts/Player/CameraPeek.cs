using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPeek : MonoBehaviour {
	[SerializeField, Range(0.01f, 1f)]
	private float inputThreshold = 0.9f;
	[SerializeField] private float peekDistance;

	private SnappingCamera snappingCamera;

	private void Start() {
		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	public void OnMoveInput(InputAction.CallbackContext ctx) {
		float value = ctx.ReadValue<Vector2>().y;
		snappingCamera.PeekOffset = Mathf.Abs(value) >= inputThreshold
			? new Vector3(0, peekDistance * Mathf.Sign(value), 0)
			: Vector3.zero;
	}
}