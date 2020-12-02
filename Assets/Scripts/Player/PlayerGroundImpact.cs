using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGroundImpact : MonoBehaviour {
	[SerializeField] private LayerMask groundLayerMask = 1 << 8;
	[SerializeField] private float minImpactVelocity = 8f;
	[SerializeField] private float maxImpactVelocity = 15f;
	[SerializeField] private float minImpactPower = 0f;
	[SerializeField] private float maxImpactPower = 0.1f;
	[SerializeField] private float impactDuration = 0.1f;
	[SerializeField] private GameObject landingEffectPrefab;
	[Space]
	[SerializeField] private UnityEvent onImpact;

	private Transformation transformation;
	private SnappingCamera snappingCamera;

	private void Start() {
		transformation = GetComponent<Transformation>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (!groundLayerMask.IncludesLayer(other.gameObject.layer) || snappingCamera == null ||
			transformation.State != TransformationState.Wolf) return;

		GetAverageContact(other, out Vector2 point, out Vector2 normal);
		float impactVelocity = Vector2.Dot(other.relativeVelocity, normal);

		if (impactVelocity >= minImpactVelocity)
			OnImpact(point, normal, impactVelocity);
	}

	private void OnImpact(Vector2 point, Vector2 normal, float velocity) {
		float power = MathX.EaseInQuad(minImpactPower, maxImpactPower,
		                               Mathf.InverseLerp(0, maxImpactVelocity, velocity));
		snappingCamera.Impact(-normal, power, impactDuration);

		if (Vector2.Dot(-normal, Vector2.up) <= 0.7f)
			Instantiate(landingEffectPrefab, point, Quaternion.LookRotation(Vector3.forward, normal));
		onImpact.Invoke();
	}

	private static void GetAverageContact(Collision2D collision, out Vector2 point, out Vector2 normal) {
		point = new Vector2();
		normal = new Vector2();
		for (int i = 0; i < collision.contactCount; i++) {
			ContactPoint2D contact = collision.GetContact(i);
			point += contact.point;
			normal += contact.normal;
		}

		point /= collision.contactCount;
		normal /= collision.contactCount;
		normal.Normalize();
	}
}