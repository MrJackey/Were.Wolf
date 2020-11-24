using System;
using UnityEngine;

public class Patrolling : MonoBehaviour {
	[SerializeField] private Transform point1 = null, point2 = null;
	[SerializeField, Range(0, 20)]
	private float speed = 5f;

	private float facing = 1;

	public Vector2 Velocity { get; private set; }

	private void OnDisable() {
		Velocity = Vector2.zero;
	}

	private void Update() {
		facing = Mathf.Sign(transform.localScale.x);

		Vector3 position = transform.position;
		Vector3 p1 = point1.position;
		Vector3 p2 = point2.position;

		Velocity = new Vector2(facing * speed, 0);
		position.x += Velocity.x * Time.deltaTime;

		if (facing > 0) {
			if (position.x >= p2.x)
				FlipDirection();
		}
		else {
			if (position.x <= p1.x)
				FlipDirection();
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
		if (enabled && point1 != null && point2 != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(point1.position, point2.position);
		}
	}
}