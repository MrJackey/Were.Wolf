using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageArea : MonoBehaviour {
	[SerializeField] private DamageMode mode = DamageMode.Single;
	[SerializeField] private DamageSource damageSource = DamageSource.Generic;
	[SerializeField] private float damage = 10;
	[SerializeField] private bool repeatSingleDamage = false;

	[SerializeField, EnableIf(nameof(repeatSingleDamage))]
	private float repeatDamageTime = 1;

	[SerializeField] private UnityEvent<Vector2> onDamageEffect = null;

	[SerializeField] private bool enableKnockback = true;
	[SerializeField, EnableIf(nameof(enableKnockback))]
	private float knockbackForce = 5;
	[SerializeField, EnableIf(nameof(enableKnockback))]
	private float knockbackDuration = 0.2f;
	[SerializeField, EnableIf(nameof(enableKnockback))]
	private bool overrideKnockbackDirection = false;
	[SerializeField, EnableIf(nameof(enableKnockback))]
	private Vector2 knockbackDirection = Vector2.zero;

	[Header("Filters")]
	[SerializeField] private bool useColliders = true;
	[SerializeField] private bool useTriggers = true;
	[SerializeField] private LayerMask layerMask = -1;
	[SerializeField] private bool ignoreTriggers = true;
	[SerializeField, Tag] private string effectTarget = "Player";

	private readonly ContactPoint2D[] contacts = new ContactPoint2D[10];
	private readonly Dictionary<Collider2D, StayInfo> staying = new Dictionary<Collider2D, StayInfo>();

	public UnityEvent<Vector2> OnDamageEffect => onDamageEffect;

	private void OnTriggerEnter2D(Collider2D other) {
		if (useTriggers) OnCollisionEvent(other, ColliderEvent.Enter);
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (useColliders) OnCollisionEvent(collision.collider, ColliderEvent.Enter);
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (useTriggers) OnCollisionEvent(other, ColliderEvent.Stay);
	}

	private void OnCollisionStay2D(Collision2D collision) {
		if (useColliders) OnCollisionEvent(collision.collider, ColliderEvent.Stay);
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (useTriggers) OnCollisionEvent(other, ColliderEvent.Exit);
	}

	private void OnCollisionExit2D(Collision2D collision) {
		if (useColliders) OnCollisionEvent(collision.collider, ColliderEvent.Exit);
	}

	private void OnCollisionEvent(Collider2D other, ColliderEvent evt) {
		if (CheckFilter(other)) return;

		if (evt == ColliderEvent.Exit) {
			staying.Remove(other);
			return;
		}

		if (mode == DamageMode.Single) {
			bool isNew = false;

			if (!staying.TryGetValue(other, out StayInfo info)) {
				// Track new object.
				info = new StayInfo {
					collider = other,
					damageTimer = repeatDamageTime,
					damagedOnce = false,
				};

				staying.Add(other, info);
				isNew = true;
			}

			// Initial (enter) damage.
			if (!info.damagedOnce)
				info.damagedOnce = DoDamage(info.collider);

			if (!isNew && repeatSingleDamage) {
				// Repeating damage.
				info.damageTimer -= Time.deltaTime;
				if (info.damageTimer <= 0) {
					if (DoDamage(info.collider))
						info.damageTimer = repeatDamageTime;
				}
			}
		}
		else { // DamageMode.Continuous
			if (evt == ColliderEvent.Stay)
				DoDamage(other);
		}
	}

	private bool DoDamage(Collider2D other) {
		Vector2 avgContactPoint = GetAvgContactPoint(other);

		Health healthComponent = other.attachedRigidbody.GetComponent<Health>();
		if (healthComponent != null) {
			// cooldownTimer = damageCooldown;
			Health health = healthComponent;

			if (health.TakeDamage(mode == DamageMode.Single ? damage : damage * Time.deltaTime, damageSource)) {
				if (effectTarget.Length != 0 && other.attachedRigidbody.CompareTag(effectTarget))
					onDamageEffect.Invoke(avgContactPoint);
			}
			else {
				return false;
			}
		}

		if (enableKnockback)
			DoKnockback(other, avgContactPoint);

		return true;
	}

	private bool CheckFilter(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return true;
		if (layerMask != -1 && ((1 << other.gameObject.layer) & layerMask.value) == 0) return true;
		return false;
	}

	private void DoKnockback(Collider2D other, Vector2 point) {
		Knockbackable knockbackComponent = other.attachedRigidbody.GetComponent<Knockbackable>();
		if (knockbackComponent != null) {
			Vector2 direction;
			if (overrideKnockbackDirection) {
				Vector2 scale = transform.lossyScale;
				direction = knockbackDirection * new Vector2(Mathf.Sign(scale.x), Mathf.Sign(scale.y));
			}
			else {
				direction = (Vector2)other.transform.position - point;
			}

			knockbackComponent.Knockback(direction.normalized, knockbackForce, knockbackDuration);
		}
	}

	private Vector2 GetAvgContactPoint(Collider2D other) {
		int contactCount = other.GetContacts(contacts);
		Vector2 avg = new Vector2();
		int num = 0;

		for (int i = 0; i < contactCount; i++) {
			avg += contacts[i].point;
			num++;
		}

		if (num > 0)
			avg /= num;

		return avg;
	}


	private enum DamageMode {
		Single,
		Continuous,
	}

	enum ColliderEvent {
		Enter,
		Stay,
		Exit,
	}

	private class StayInfo {
		public Collider2D collider;
		public float damageTimer;
		public bool damagedOnce;
	}
}
