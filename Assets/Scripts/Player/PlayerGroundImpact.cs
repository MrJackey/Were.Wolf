using Extensions;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(1)]
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

	private PlayerController playerController;
	private bool wasGrounded;

	private void Start() {
		transformation = GetComponent<Transformation>();
		playerController = GetComponent<PlayerController>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void FixedUpdate() {
		wasGrounded = playerController.IsGrounded;
	}

	private void OnCollisionStay2D(Collision2D other) {
		// This is used to cover an edge case where the player is landing while also touching a wall.
		if (!wasGrounded && playerController.IsGrounded)
			OnCollision(other, true);
	}

	private void OnCollisionEnter2D(Collision2D other) {
		OnCollision(other, false);
	}

	private void OnCollision(Collision2D other, bool hitGroundInStay) {
		if (!groundLayerMask.IncludesLayer(other.gameObject.layer) || snappingCamera == null)
			return;

		Vector2 point, normal;
		if (hitGroundInStay) {
			// We hit the ground but are also touching a wall. Instead of using the contact point here we treat things
			// as if we hit just the ground.
			BoxCollider2D hitCollider = transformation.HitCollider;
			point = hitCollider.transform.position + (Vector3)hitCollider.offset;
			point.y -= hitCollider.size.y * 0.5f + hitCollider.edgeRadius;
			normal = Vector2.up;
		}
		else {
			GetAverageContact(other, out point, out normal);
		}

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