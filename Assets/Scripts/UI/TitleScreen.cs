using System;
using UnityEngine;

public class TitleScreen : MonoBehaviour {
	private static bool hasShown;

	[SerializeField] private MenuScreens mainMenu;

	private void Start() {
		if (hasShown)
			Hide();

		hasShown = true;
	}

	private void Update() {
		if (Input.anyKeyDown)
			Hide();
	}

	private void Hide() {
		gameObject.SetActive(false);
		mainMenu.IsVisible = true;
	}
}