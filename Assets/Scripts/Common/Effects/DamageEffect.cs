using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffect : MonoBehaviour {
	[SerializeField] private ParticleSystem damageEffect;

	private void OnEnable() {
		GetComponent<DamageArea>().OnDamageEffect.AddListener(Spawn);
	}

	private void OnDisable() {
		GetComponent<DamageArea>().OnDamageEffect.RemoveListener(Spawn);
	}

	private void Spawn(Vector2 position) {
		Instantiate(damageEffect, position, Quaternion.identity);
	}
}
