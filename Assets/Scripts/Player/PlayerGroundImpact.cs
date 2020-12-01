using System;
using Extensions;
using UnityEngine;

public class PlayerGroundImpact : MonoBehaviour {
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
			float power = MathX.Remap(other.relativeVelocity.y,
			                          minImpactVelocity, maxImpactVelocity,
			                          minImpactPower, maxImpactPower);
			snappingCamera.Impact(Vector3.down, power, impactDuration);
		}
	}
}