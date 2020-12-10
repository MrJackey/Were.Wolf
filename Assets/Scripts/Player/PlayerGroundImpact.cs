using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerGroundImpact : MonoBehaviour {
	[SerializeField] private LayerMask groundLayerMask = 1 << 8;
	[SerializeField] private float minWeakImpactVelocity = 3f;
	[SerializeField] private float minImpactVelocity = 8f;
	[SerializeField] private float maxImpactVelocity = 15f;
	[SerializeField] private float minImpactPower = 0f;
	[SerializeField] private float maxImpactPower = 0.1f;
	[SerializeField] private float impactDuration = 0.1f;
	[SerializeField] private GameObject landingEffectPrefab;

	[Space]
	[SerializeField] private UnityEvent onHeavyImpact;
	[SerializeField] private UnityEvent onWeakLanding;
	[SerializeField] private UnityEvent onHeavyLanding;

	private Transformation transformation;
	private SnappingCamera snappingCamera;

	private void Start() {
		transformation = GetComponent<Transformation>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (!groundLayerMask.IncludesLayer(other.gameObject.layer) || snappingCamera == null)
			return;

		GetAverageContact(other, out Vector2 point, out Vector2 normal);
		float impactVelocity = Vector2.Dot(other.relativeVelocity, normal);
		float downVelocity = other.relativeVelocity.y;

		if (impactVelocity >= minImpactVelocity)
			OnHeavyImpact(point, normal, impactVelocity);

		if (downVelocity >= minImpactVelocity)
			onHeavyLanding.Invoke();
		else if (downVelocity >= minWeakImpactVelocity)
			onWeakLanding.Invoke();
	}

	private void OnHeavyImpact(Vector2 point, Vector2 normal, float velocity) {
		if (transformation.State != TransformationState.Wolf)
			return;

		float power = MathX.EaseInQuad(minImpactPower, maxImpactPower,
		                               Mathf.InverseLerp(0, maxImpactVelocity, velocity));
		snappingCamera.Impact(-normal, power, impactDuration);

		if (Vector2.Dot(-normal, Vector2.up) <= 0.7f)
			Instantiate(landingEffectPrefab, point, Quaternion.LookRotation(Vector3.forward, normal));
		onHeavyImpact.Invoke();
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