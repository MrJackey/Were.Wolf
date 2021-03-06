﻿using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private SceneHelper sceneHelper = null;
	[SerializeField] private InputActionReference pauseActionReference = null;
	[SerializeField] private MenuScreens menuScreens;
	[SerializeField] private UnityEvent onPause = null;
	[SerializeField] private UnityEvent onResume = null;

	private bool isPaused;
	private PlayerInput playerInput;
	private InputAction pauseAction;

	private void Awake() {
		pauseAction = pauseActionReference.action.Clone();
		pauseAction.Enable();
	}

	private void OnEnable() {
		if (isPaused)
			Cursor.visible = true;
		pauseAction.started += OnPauseDown;

		GameObject player = GameObject.FindWithTag("Player");
		if (player != null) playerInput = player.GetComponent<PlayerInput>();
	}

	private void OnDisable() {
		Cursor.visible = false;
		pauseAction.started -= OnPauseDown;
	}

	private void OnPauseDown(InputAction.CallbackContext ctx) {
		if (isPaused)
			GoBack();
		else
			Pause();
	}

	public void Pause() {
		if (isPaused || sceneHelper.IsTransitioning || Time.timeScale == 0) return;
		isPaused = true;
		Time.timeScale = 0;
		Cursor.visible = true;
		if (playerInput != null)
			playerInput.DeactivateInput();

		onPause.Invoke();
	}

	public void Resume() {
		if (!isPaused) return;
		isPaused = false;
		Time.timeScale = 1;
		Cursor.visible = false;
		if (playerInput != null)
			playerInput.ActivateInput();

		onResume.Invoke();
	}

	public void RestartLevel() {
		Time.timeScale = 1;
		sceneHelper.ReloadScene();
	}

	public void ExitToMenu() {
		sceneHelper.LoadSceneWithTransition(sceneHelper.MenuScene);
	}

	public void GoBack() {
		if (menuScreens.NumMenusOnStack > 1)
			menuScreens.PopScreen();
		else
			Resume();
	}
}
