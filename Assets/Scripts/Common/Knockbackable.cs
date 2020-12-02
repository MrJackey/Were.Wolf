using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Knockbackable : MonoBehaviour {
	[SerializeField] private UnityEvent onKnockback;
	[SerializeField] private UnityEvent onKnockbackEnd;

	private Coroutine knockbackDurationRoutine;
	private bool doKnockback;
	private Vector2 velocity;
	private Rigidbody2D rb2D;

	public Vector2 Velocity => velocity;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
	}

	public void Knockback(Vector2 direction, float force, float duration) {
		onKnockback.Invoke();
		velocity = direction * force;

		if (knockbackDurationRoutine != null)
			StopCoroutine(knockbackDurationRoutine);

		doKnockback = true;
		knockbackDurationRoutine = StartCoroutine(CoKnockbackDuration(duration));
	}

	private void FixedUpdate() {
		if (doKnockback)
			rb2D.velocity = velocity;
}

	private IEnumerator CoKnockbackDuration(float duration) {
		yield return new WaitForSeconds(duration);
		onKnockbackEnd.Invoke();
		knockbackDurationRoutine = null;
		doKnockback = false;
	}
}
