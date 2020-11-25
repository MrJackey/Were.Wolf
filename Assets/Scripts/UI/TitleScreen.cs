﻿using System;
using UnityEngine;

public class TitleScreen : MonoBehaviour {
	[SerializeField] private MenuScreens mainMenu;
	[SerializeField] private GameObject anyKeyPrompt;

	private void Update() {
		if (Input.anyKey) {
			enabled = false;
			anyKeyPrompt.SetActive(false);
			mainMenu.IsVisible = true;
		}
	}
}