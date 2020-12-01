using System;
using Extensions;
using UnityEngine;

public class PlayerGroundImpact : MonoBehaviour {
	[SerializeField] private GameObject landingEffectPrefab;
	[SerializeField] private LayerMask groundLayerMask = 1 << 8;
	[SerializeField] private float minImpactVelocity = 8f;
	[SerializeField] private float maxImpactVelocity = 15f;
	[SerializeField] private float minImpactPower = 0f;
	[SerializeField] private float maxImpactPower = 0.1f;
	[SerializeField] private float impactDuration = 0.1f;

	private PlayerController playerController;
	private Transformation transformation;
	private SnappingCamera snappingCamera;

	private void Start() {
		playerController = GetComponent<PlayerController>();
		transformation = GetComponent<Transformation>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (!playerController.IsGrounded || !groundLayerMask.IncludesLayer(other.gameObject.layer) || snappingCamera == null) return;
		if (transformation.State == TransformationState.Wolf && other.relativeVelocity.y > minImpactVelocity) {
			float power = MathX.EaseInQuad(minImpactPower, maxImpactPower,
			                               Mathf.InverseLerp(0, maxImpactVelocity, other.relativeVelocity.y));
			snappingCamera.Impact(Vector3.down, power, impactDuration);

			Vector2 effectPoint = GetAverageContact(other);
			effectPoint.x = transform.position.x;
			Instantiate(landingEffectPrefab, effectPoint, Quaternion.identity);
		}
	}

	private static Vector2 GetAverageContact(Collision2D collision) {
		Vector2 sum = default;
		for (int i = 0; i < collision.contactCount; i++) {
			ContactPoint2D contact = collision.GetContact(i);
			sum += contact.point;
		}

		return sum / collision.contactCount;
	}
}