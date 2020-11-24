using UnityEngine;

public class LineMover : Mover {
	[SerializeField] private Transform point1 = null, point2 = null;
	[SerializeField, Range(0, 20)]
	private float speed = 5f;
	[SerializeField] private bool snapToLine = false;

	private float facing = 1;
	private Vector2 initialOffsetToLine;
	private float positionOnLine;
	private float lineLength;

	public override float Speed {
		get => speed;
		set => speed = value;
	}

	private void Start() {
		Vector3 position = transform.position;
		Vector3 p1 = point1.position, p2 = point2.position;

		Vector2 pointOnLine = MathX.ClosestPointOnLineSegment(position, p1, p2);
		initialOffsetToLine = (Vector2)position - pointOnLine;
		positionOnLine = MathX.InverseLerp(position, p1, p2);
	}

	private void Update() {
		facing = Mathf.Sign(transform.localScale.x);

		Vector3 p1 = point1.position, p2 = point2.position;
		lineLength = Vector2.Distance(p1, p2);
		positionOnLine += facing * speed * Time.deltaTime / lineLength;

		if (facing > 0) {
			if (positionOnLine >= 1f)
				FlipDirection();
		}
		else {
			if (positionOnLine <= 0f)
				FlipDirection();
		}

		Vector2 position = Vector2.LerpUnclamped(p1, p2, positionOnLine);
		if (!snapToLine) position += initialOffsetToLine;
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