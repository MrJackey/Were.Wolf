using System;
using UnityEngine;

public class HUDValueSetter : MonoBehaviour {
	[SerializeField] private UIValueBar healthBar;

	private Health health;

	private void OnEnable() {
		GameObject go = GameObject.FindWithTag("Player");
		if (go == null) return;

		health = go.GetComponent<Health>();
		healthBar.MaxValue = health.MaxValue;
		healthBar.Value = health.Value;
		health.OnValueChange.AddListener(OnHealthChange);
	}

	private void OnDisable() {
		if (health != null)
			health.OnValueChange.RemoveListener(OnHealthChange);
	}

	private void OnHealthChange() {
		healthBar.Value = health.Value;
	}
}