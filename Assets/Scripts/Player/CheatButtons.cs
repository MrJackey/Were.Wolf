using UnityEngine;
using UnityEngine.InputSystem;

public class CheatButtons : MonoBehaviour {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[SerializeField] private SceneHelper sceneHelper;

	private Health health;
	private Transformation transformation;
	private PlayerController playerController;

	private bool godMode;

	private void Start() {
		health = GetComponent<Health>();
		transformation = GetComponent<Transformation>();
		playerController = GetComponent<PlayerController>();
	}

	private void Update() {
		Keyboard kb = Keyboard.current;

		// Noclip
		if (kb.f1Key.wasPressedThisFrame) {
			playerController.NoClip = !playerController.NoClip;
			if (godMode)
				health.IsInvincible = true;
		}

		// Restore HP and transformation cooldown
		if (kb.f2Key.wasPressedThisFrame) {
			health.RestoreHealth();
			transformation.TransformationCooldown = 0f;
		}

		// Godmode
		if (kb.f3Key.wasPressedThisFrame) {
			godMode = !godMode;
			health.IsInvincible = godMode;
		}

		// Level switching
		if (kb.pageUpKey.wasPressedThisFrame) {
			Time.timeScale = 1;
			sceneHelper.LoadPreviousLevel();
		}
		else if (kb.pageDownKey.wasPressedThisFrame) {
			if (sceneHelper.CurrentLevel != sceneHelper.Levels.Length - 1) {
				Time.timeScale = 1;
				sceneHelper.LoadNextLevel();
			}
		}
	}

	private void OnGUI() {
		if (!(godMode || playerController.NoClip)) return;

		GUILayout.BeginArea(new Rect(Screen.width - 120, 0, 120, Screen.height));
		GUILayout.BeginVertical("Box");

		if (playerController.NoClip)
			GUILayout.Label("Noclip: ON");

		if (godMode)
			GUILayout.Label("God mode: ON");

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
#endif
}