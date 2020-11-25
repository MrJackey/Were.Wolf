using System;
using UnityEngine;

public class TempHealthView : MonoBehaviour {
	private Health health;
	private GUIStyle style;

	private void Start() {
		health = GetComponent<Health>();
		style = new GUIStyle {
			fontSize = 24,
			normal = new GUIStyleState {
				textColor = Color.white
			}
		};
	}

	private void OnGUI() {
		GUILayout.Label($"Health: {health.Value}", style);
	}
}