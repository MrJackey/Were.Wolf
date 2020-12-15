using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPeek : MonoBehaviour {
	[SerializeField, Range(0, 12)]
	private float peekDistance;

	private SnappingCamera snappingCamera;
	private float peekOffsetY;

	private void Start() {
		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void Update() {
		if (Time.timeScale == 0 || peekOffsetY == 0)
			snappingCamera.PeekOffset = Vector3.zero;
		else
			snappingCamera.PeekOffset = new Vector3(0, peekOffsetY, 0);
	}

	public void OnPeekInput(InputAction.CallbackContext ctx) {
		float value = ctx.ReadValue<float>();
		peekOffsetY = value != 0 ? peekDistance * value : 0;
	}
}