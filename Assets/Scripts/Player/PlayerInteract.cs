﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour {
	[SerializeField] private PlayerHandDetection playerHandDetection;
	private PlayerCarrying playerCarrying;
	
	private void Start() {
		playerCarrying = GetComponent<PlayerCarrying>();
	}

	public void OnInteract(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started && ctx.started)
			OnInteractDown();
	}

	private void OnInteractDown() {
		if (playerCarrying.IsCarryingItem)
			playerCarrying.DropItem();
		else if (playerHandDetection.DetectedInteractItem != null)
			playerHandDetection.DetectedInteractItem.Interact(gameObject);
	}
}