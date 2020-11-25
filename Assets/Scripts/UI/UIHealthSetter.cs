using System;
using UnityEngine;

public class UIHealthSetter : MonoBehaviour {
	private UIValueBar bar;
	private Health playerHealth;

	private void Start() {
		bar = GetComponent<UIValueBar>();
		GameObject go = GameObject.FindWithTag("Player");
		if (go != null)
			playerHealth = go.GetComponent<Health>();

		if (playerHealth != null) {
			bar.MaxValue = playerHealth.MaxValue;
			bar.Value = playerHealth.Value;
		}
	}

	private void Update() {
		if (playerHealth == null) return;

		bar.Value = playerHealth.Value;
	}
}