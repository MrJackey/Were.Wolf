using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEvents : MonoBehaviour {
	[Header("Filters")]
	[SerializeField, Tag] private string targetTag = "Player";
	[SerializeField] private LayerMask layerMask = -1;
	[SerializeField] private bool ignoreTriggers = true;

	[Header("Events")]
	[SerializeField] private UnityEvent onTriggerEnter;
	[SerializeField] private UnityEvent onTriggerExit;
	[SerializeField] private UnityEvent onCollisionEnter;
	[SerializeField] private UnityEvent onCollisionExit;

	private void OnTriggerEnter2D(Collider2D other) {
		if (CheckFilter(other))
			onTriggerEnter.Invoke();
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (CheckFilter(other))
			onTriggerExit.Invoke();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (CheckFilter(other.collider))
			onCollisionEnter.Invoke();
	}

	private void OnCollisionExit2D(Collision2D other) {
		if (CheckFilter(other.collider))
			onCollisionExit.Invoke();
	}

	private bool CheckFilter(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return false;
		if (!string.IsNullOrEmpty(targetTag) && !other.attachedRigidbody.CompareTag(targetTag)) return false;
		return layerMask.IncludesLayer(other.gameObject.layer);
	}
}