using System;
using UnityEngine;

public class HUDValueSetter : MonoBehaviour {
	[SerializeField] private UIValueBar healthBar;
	[SerializeField] private UIValueBar tpBar;

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
		tpBar.MaxValue = transformation.MaxPoints;
		tpBar.Value = transformation.Points;
		transformation.OnPointsChanged.AddListener(OnTransformationPointsChange);
	}

	private void OnDisable() {
		if (health != null)
			health.OnValueChange.RemoveListener(OnHealthChange);
		if (transformation != null)
			transformation.OnPointsChanged.RemoveListener(OnTransformationPointsChange);
	}

	private void OnHealthChange() {
		healthBar.Value = health.Value;
	}

	private void OnTransformationPointsChange() {
		tpBar.Value = transformation.Points;
	}
}