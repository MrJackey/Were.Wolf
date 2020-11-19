using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SimpleEnemy : MonoBehaviour {
	[Header("Detection")]
	[SerializeField] private Vector2 eyeOffset = Vector2.zero;
	[SerializeField] private float visionConeAngle = 30f;
	[SerializeField] private float visionDistance = 4f;
	[SerializeField, Range(1, 15)]
	private int visionRayCount = 3;
	[SerializeField] private LayerMask raycastLayers = -1;
	[SerializeField] private bool restartOnDetection = true;

	[Header("Patrolling")]
	[SerializeField] private bool enablePatrolling = false;
	[SerializeField] private Transform point1 = null, point2 = null;
	[SerializeField, Range(0, 20)]
	private float speed = 5f;

	private bool isPlayerInVisionCone;
	private float facing = 1;

	private void Update() {
		Vector3 scale = transform.localScale;
		facing = Mathf.Sign(scale.x);

		if (CheckPlayerVisible()) OnDetected();
		if (enablePatrolling) UpdatePatrolling();
	}

	private bool CheckPlayerVisible() {
		Vector2 eyePosition = transform.TransformPoint(eyeOffset);
		Vector2 forward = transform.right * facing;
		float angleStep = visionConeAngle / visionRayCount;

		for (int i = 0; i < visionRayCount; i++) {
			float angle = -visionConeAngle / 2f + angleStep * i + angleStep / 2f;
			Vector2 direction = Quaternion.AngleAxis(angle, Vector3.forward) * forward;

			if (DoSingleRaycast(eyePosition, direction))
				return true;
		}

		return false;
	}

	private bool DoSingleRaycast(Vector2 origin, Vector2 direction) {
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, visionDistance, raycastLayers);

	#if UNITY_EDITOR
		float distance = hit.collider != null ? hit.distance : visionDistance;
		Debug.DrawRay(origin, direction * distance, Color.red);
	#endif

		if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player")) {
			Transformation transformation = hit.rigidbody.GetComponent<Transformation>();
			if (transformation == null || transformation.TransformationState == TransformationStates.Wolf)
				return true;
		}

		return false;
	}

	private void OnDetected() {
		if (restartOnDetection)
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void UpdatePatrolling() {
		Vector3 position = transform.position;
		Vector3 p1 = point1.position;
		Vector3 p2 = point2.position;

		position.x += facing * speed * Time.deltaTime;

		if (facing > 0) {
			if (position.x >= p2.x) FlipDirection();
		}
		else {
			if (position.x <= p1.x) FlipDirection();
		}

		transform.position = position;
	}

	private void FlipDirection() {
		facing = facing > 0 ? -1f : 1f;

		Vector3 scale = transform.localScale;
		scale.x = facing;
		transform.localScale = scale;
	}

	private void OnDrawGizmos() {
		Vector3 eyePosition = transform.TransformPoint(eyeOffset);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(eyePosition, 0.2f);

		if (enablePatrolling && point1 != null && point2 != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(point1.position, point2.position);
		}
	}
}