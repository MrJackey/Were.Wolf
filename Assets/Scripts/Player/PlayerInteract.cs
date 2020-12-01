using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour {
	[SerializeField] private PlayerHandDetection playerHandDetection;

	private PlayerCarrying playerCarrying;
	private PlayerController playerController;

	private void Start() {
		playerCarrying = GetComponent<PlayerCarrying>();
		playerController = GetComponent<PlayerController>();
	}

	public void OnInteract(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started && ctx.started)
			OnInteractDown();
	}

	private void OnInteractDown() {
		if (!playerController.AllowControls) return;

		if (playerCarrying.IsCarryingItem)
			playerCarrying.DropItem();
		else if (playerHandDetection.DetectedInteractItem != null)
			playerHandDetection.DetectedInteractItem.Interact(gameObject);
	}
}