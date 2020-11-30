using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private SceneHelper sceneHelper = null;
	[SerializeField] private InputActionReference pauseAction;
	[SerializeField] private UnityEvent onPause = null;
	[SerializeField] private UnityEvent onResume = null;

	private bool isPaused;

	private void OnEnable() {
		if (isPaused)
			Cursor.visible = true;
		pauseAction.action.started += OnPauseDown;
	}

	private void OnDisable() {
		Cursor.visible = false;
		pauseAction.action.started -= OnPauseDown;
	}

	private void OnPauseDown(InputAction.CallbackContext ctx) {
		if (isPaused)
			Resume();
		else
			Pause();
	}

	public void Pause() {
		if (isPaused) return;
		isPaused = true;
		Time.timeScale = 0;
		Cursor.visible = true;
		onPause.Invoke();
	}

	public void Resume() {
		if (!isPaused) return;
		isPaused = false;
		Time.timeScale = 1;
		Cursor.visible = false;
		onResume.Invoke();
	}

	public void ExitToMenu() {
		Time.timeScale = 1;
		sceneHelper.LoadScene(sceneHelper.MenuScene);
	}
}