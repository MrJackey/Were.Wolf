using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour {
	private enum DamageMode {
		Single,
		Continuous,
	}

	[SerializeField] private DamageMode mode = DamageMode.Single;
	[SerializeField] private float damage = 10;
	[SerializeField] private float damageCooldown = 0;
	[SerializeField] private UnityEvent<Vector2> onDamageEffect;
	[SerializeField] private float knockbackForce = 5;
	[SerializeField] private float knockbackDuration = 0.2f;

	[Header("Filters")]
	[SerializeField] private LayerMask layerMask = -1;
	[SerializeField] private bool ignoreTriggers = true;
	[SerializeField, Tag] private string effectTarget;

	private float cooldownTimer;
	private ContactPoint2D[] contacts = new ContactPoint2D[10];

	public UnityEvent<Vector2> OnDamageEffect => onDamageEffect;

	private void OnTriggerEnter2D(Collider2D other) => OnCollisionEvent(other, true);
	private void OnCollisionEnter2D(Collision2D collision) => OnCollisionEvent(collision.collider, true);
	private void OnTriggerStay2D(Collider2D other) => OnCollisionEvent(other, false);
	private void OnCollisionStay2D(Collision2D collision) => OnCollisionEvent(collision.collider, false);

	private void Update() {
		cooldownTimer = Mathf.Max(0, cooldownTimer - Time.deltaTime);
	}

	private void OnCollisionEvent(Collider2D other, bool isEnter) {
		if (isEnter != (mode == DamageMode.Single)) return;
		if (cooldownTimer != 0) return;
		if (ignoreTriggers && other.isTrigger) return;
		if (layerMask != -1 && ((1 << other.gameObject.layer) & layerMask.value) == 0) return;

		CalculateAvgContact(other, out Vector2 averageContact);

		Health healthComponent = other.attachedRigidbody.GetComponent<Health>();
		if (healthComponent != null) {
			cooldownTimer = damageCooldown;
			Health health = healthComponent;
			health.TakeDamage(isEnter ? damage : damage * Time.deltaTime);

			if (other.attachedRigidbody.CompareTag(effectTarget))
				onDamageEffect.Invoke(averageContact);
		}

		Knockbackable knockbackComponent = other.attachedRigidbody.GetComponent<Knockbackable>();
		if (knockbackComponent != null) {
			Vector2 direction = (Vector2) other.transform.position - averageContact;
			knockbackComponent.Knockback(direction.normalized, knockbackForce, knockbackDuration);
		}
	}

	private void CalculateAvgContact(Collider2D other, out Vector2 avg) {
		int contactCount = other.GetContacts(contacts);
		avg = new Vector2();
		int num = 0;
		if (contactCount < 1) return;

		for (int i = 0; i < contactCount; i++) {
			if (contacts[i].collider.gameObject != gameObject) continue;

			avg += contacts[i].point;
			num++;
		}

		if (num > 0)
			avg /= num;
	}
}
