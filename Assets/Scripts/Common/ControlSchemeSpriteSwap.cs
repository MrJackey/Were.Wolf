using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeSpriteSwap : MonoBehaviour {
	[SerializeField] private Sprite keyboardSprite;
	[SerializeField] private Sprite gamepadSprite;

	private SpriteRenderer spriteRenderer;
	private PlayerInput playerInput;
	private string oldControlScheme;

	private void OnEnable() {
		spriteRenderer = GetComponent<SpriteRenderer>();

		GameObject player = GameObject.FindWithTag("Player");
		playerInput = player.GetComponent<PlayerInput>();

		if (playerInput != null) {
			ControlSchemeManager.Instance.ControlSchemeChanged += OnControlSchemeChanged;
			ControlSchemeManager.SetControlScheme(playerInput, ControlSchemeManager.Instance.LastUsedControlScheme);
			OnControlSchemeChanged(playerInput.currentControlScheme);
		}
	}

	private void OnDisable() {
		// Sometimes null on disable??
		if (ControlSchemeManager.Instance != null)
			ControlSchemeManager.Instance.ControlSchemeChanged -= OnControlSchemeChanged;
	}

	private void OnControlSchemeChanged(string scheme) {
		spriteRenderer.sprite = scheme == "Keyboard" ? keyboardSprite : gamepadSprite;
	}
}