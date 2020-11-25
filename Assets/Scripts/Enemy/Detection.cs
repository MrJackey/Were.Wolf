﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Detection : MonoBehaviour {
	[SerializeField] private Vector2 eyeOffset = Vector2.zero;
	[SerializeField] private float visionConeAngle = 30f;
	[SerializeField] private float visionDistance = 4f;
	[SerializeField, Range(1, 15)]
	private int visionRayCount = 3;
	[SerializeField] private LayerMask raycastLayers = -1;
	[Space]
	[SerializeField] private float damagePerSecond;

	[Header("Events")]
	[SerializeField] private UnityEvent onDetected;
	[SerializeField] private UnityEvent onLost;

	private bool isPlayerVisible;
	private float facing = 1;
	private GameObject playerObject;
	private Health playerHealth;

	private void Update() {
		facing = Mathf.Sign(transform.localScale.x);

		if (isPlayerVisible != (isPlayerVisible = CheckPlayerVisible(out playerObject))) {
			if (isPlayerVisible) {
				playerHealth = playerObject.GetComponent<Health>();
				onDetected.Invoke();
			}
			else {
				onLost.Invoke();
			}
		}

		if (isPlayerVisible)
			playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
	}

	private bool CheckPlayerVisible(out GameObject go) {
		Vector2 eyePosition = transform.TransformPoint(eyeOffset);
		Vector2 forward = transform.right * facing;
		float angleStep = visionConeAngle / (visionRayCount - 1);

		for (int i = 0; i < visionRayCount; i++) {
			float angle = -visionConeAngle / 2f + angleStep * i;
			Vector2 direction = MathX.Rotate(forward, angle * Mathf.Deg2Rad);

			if (DoSingleRaycast(eyePosition, direction, out go)) {
			#if UNITY_EDITOR
				if (!Application.isPlaying) continue;
			#endif

				return true;
			}
		}

		go = null;
		return false;
	}

	private bool DoSingleRaycast(Vector2 origin, Vector2 direction, out GameObject go) {
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, visionDistance, raycastLayers);

	#if UNITY_EDITOR
		float distance = hit.collider != null ? hit.distance : visionDistance;
		Debug.DrawRay(origin, direction * distance, Color.red);
	#endif

		if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player")) {
			Transformation transformation = hit.rigidbody.GetComponent<Transformation>();
			if (transformation == null || transformation.State != TransformationState.Human) {
				go = hit.rigidbody.gameObject;
				return true;
			}
		}

		go = null;
		return false;
	}

	private void OnDrawGizmos() {
		if (!enabled) return;
		Vector3 eyePosition = transform.TransformPoint(eyeOffset);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(eyePosition, 0.2f);

		CheckPlayerVisible(out _);
	}
}