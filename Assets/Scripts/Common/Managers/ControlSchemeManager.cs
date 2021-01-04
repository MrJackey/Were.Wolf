using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlSchemeManager : SingletonBehaviour<ControlSchemeManager> {
	[SerializeField] private string defaultControlScheme = "Keyboard";

	private PlayerInput playerInput;

	public string LastUsedControlScheme { get; private set; }

	public bool IsUsingKeyboard => LastUsedControlScheme == "Keyboard";

	public bool IsUsingGamepad => LastUsedControlScheme == "Gamepad";

	public event Action<string> ControlSchemeChanged;

	private void OnEnable() {
		DontDestroyOnLoad(gameObject);

		LastUsedControlScheme = defaultControlScheme;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start() {
		playerInput = PlayerInput.GetPlayerByIndex(0);
	}

	private void Update() {
		if (playerInput == null) return;

		CheckControlSchemeChanged();
	}

	private void CheckControlSchemeChanged() {
		string currentScheme = playerInput.currentControlScheme;
		if (currentScheme != LastUsedControlScheme) {
			LastUsedControlScheme = currentScheme;
			ControlSchemeChanged?.Invoke(currentScheme);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		playerInput = PlayerInput.GetPlayerByIndex(0);
		if (playerInput != null)
			SetControlScheme(playerInput, LastUsedControlScheme);
	}


	public static void SetControlScheme(PlayerInput playerInput, string controlScheme) {
		if (controlScheme == "Keyboard" && Keyboard.current != null)
			playerInput.SwitchCurrentControlScheme(controlScheme, Keyboard.current);
		else if (controlScheme == "Gamepad" && Gamepad.current != null)
			playerInput.SwitchCurrentControlScheme(controlScheme, Gamepad.current);
		else
			throw new ArgumentException("Unknown control scheme.", nameof(controlScheme));

		if (Instance != null && Instance.playerInput != null)
			Instance.CheckControlSchemeChanged();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnLoad() {
		_ = new GameObject("Control Scheme Manager", typeof(ControlSchemeManager));
	}
}