using UnityEngine;
using UnityEngine.InputSystem;

public class CheatButtons : MonoBehaviour {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[SerializeField] private SceneHelper sceneHelper;

	private Health health;
	private Transformation transformation;

	private void Start() {
		health = GetComponent<Health>();
		transformation = GetComponent<Transformation>();
	}

	private void Update() {
		Keyboard kb = Keyboard.current;

		// Restore HP and TP.
		if (kb.f2Key.wasPressedThisFrame) {
			health.RestoreHealth();
			transformation.Points = transformation.MaxPoints;
		}

		// Level switching.
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
#endif
}