using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
	private void OnEnable() {
		Cursor.visible = true;
	}

	private void OnDisable() {
		Cursor.visible = false;
	}

	public void Exit() {
	#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode();
	#else
		Application.Quit();
	#endif
	}
}