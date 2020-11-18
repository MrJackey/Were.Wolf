using UnityEngine;

public class FollowingCamera : MonoBehaviour {
	[SerializeField] private Transform target = null;
	[SerializeField] private float smoothTime = 0.15f;
	[SerializeField] private float maxSpeed = float.PositiveInfinity;

	private Vector2 offset;
	private Vector2 currentVelocity;

	private void Start() {
		offset = transform.position - target.position;
	}

	private void LateUpdate() {
		Vector3 currentPosition = transform.position;
		Vector2 targetPosition = (Vector2)target.position + offset;
		Vector2 newPosition = Vector2.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, smoothTime, maxSpeed);
		transform.position = new Vector3(newPosition.x, newPosition.y, currentPosition.z);
	}
}