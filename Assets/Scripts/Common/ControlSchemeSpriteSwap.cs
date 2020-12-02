using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeSpriteSwap : MonoBehaviour {
	[SerializeField] private Sprite keyboardSprite;
	[SerializeField] private Sprite gamepadSprite;

	private SpriteRenderer spriteRenderer;
	private PlayerInput playerInput;

	private void OnEnable() {
		spriteRenderer = GetComponent<SpriteRenderer>();

		GameObject player = GameObject.FindWithTag("Player");
		if (player != null) {
			playerInput = player.GetComponent<PlayerInput>();
			playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
			OnControlsChanged(playerInput);
		}
	}

	private void OnDisable() {
		if (playerInput != null)
			playerInput.controlsChangedEvent.RemoveListener(OnControlsChanged);
	}

	private void OnControlsChanged(PlayerInput _) {
		spriteRenderer.sprite = playerInput.currentControlScheme == "Keyboard" ? keyboardSprite : gamepadSprite;
	}
}