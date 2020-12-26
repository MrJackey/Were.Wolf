using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDetectionCone : MonoBehaviour {
	[SerializeField] private float visionConeAngle = 30f;
	[SerializeField] private float visionDistance = 4f;
	[SerializeField, Range(1, 15)]
	private int visionRayCount = 3;
	[SerializeField] private LayerMask raycastLayers = -1;
	[SerializeField] private bool ignoreTriggers = true;

	[Header("Events")]
	[SerializeField] private UnityEvent onBecomeVisible;
	[SerializeField] private UnityEvent onEndVisible;

	private bool isPlayerVisible;

	public bool IsPlayerVisible => isPlayerVisible;

	public float Rotation {
		get => transform.localEulerAngles.z;
		set => transform.localEulerAngles = new Vector3(0, 0, value);
	}

	public UnityEvent OnBecomeVisible => onBecomeVisible;

	public UnityEvent OnEndVisible => onEndVisible;

	private void OnDisable() {
		if (isPlayerVisible) {
			isPlayerVisible = false;
			onEndVisible.Invoke();
		}
	}

	private void Update() {
		if (isPlayerVisible == (isPlayerVisible = CheckPlayerVisible())) return;
		if (isPlayerVisible)
			onBecomeVisible.Invoke();
		else
			onEndVisible.Invoke();
	}

	private bool CheckPlayerVisible() {
		float facing = Mathf.Sign(transform.lossyScale.x);

		Vector2 eyePosition = transform.position;
		Vector2 forward = transform.right * facing;
		float angleStep = visionConeAngle / (visionRayCount - 1);

		for (int i = 0; i < visionRayCount; i++) {
			float angle = -visionConeAngle / 2f + angleStep * i;
			Vector2 direction = MathX.Rotate(forward, angle * Mathf.Deg2Rad);

			if (DoSingleRaycast(eyePosition, direction)) {
			#if UNITY_EDITOR
				if (!Application.isPlaying) continue;
			#endif

				return true;
			}
		}

		return false;
	}

	private bool DoSingleRaycast(Vector2 origin, Vector2 direction) {
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, visionDistance, raycastLayers);

	#if UNITY_EDITOR
		float distance = hit.collider != null ? hit.distance : visionDistance;
		Debug.DrawRay(origin, direction * distance, Color.red);

		if (hit.collider != null)
			Debug.DrawLine(hit.point, hit.point + hit.normal * 0.1f, Color.yellow);
	#endif

		if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player")) {
			if (ignoreTriggers && hit.collider.isTrigger)
				return false;

			Transformation transformation = hit.rigidbody.GetComponent<Transformation>();
			if (transformation == null || transformation.State != TransformationState.Human)
				return true;
		}

		return false;
	}


#if UNITY_EDITOR
	private void OnDrawGizmos() {
		if (!enabled || Application.isPlaying) return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 0.2f);

		CheckPlayerVisible();
	}
#endif
}