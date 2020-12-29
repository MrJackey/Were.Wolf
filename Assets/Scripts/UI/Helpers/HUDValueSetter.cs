using System;
using UnityEngine;

public class HUDValueSetter : MonoBehaviour {
	[SerializeField] private UIValueBar healthBar;
	[SerializeField] private UIIndexedImage moonImage;


	private Health health;
	private Transformation transformation;

	private void OnEnable() {
		GameObject go = GameObject.FindWithTag("Player");
		if (go == null) return;

		health = go.GetComponent<Health>();
		healthBar.MaxValue = health.MaxValue;
		healthBar.Value = health.Value;
		health.OnValueChange.AddListener(OnHealthChange);

		transformation = go.GetComponent<Transformation>();
	}

	private void OnDisable() {
		if (health != null)
			health.OnValueChange.RemoveListener(OnHealthChange);
	}

	private void Update() {
		moonImage.Value = 1f - transformation.TransformationCooldown / transformation.TransformCooldownDuration;
	}

	private void OnHealthChange() {
		healthBar.Value = health.Value;
	}
}