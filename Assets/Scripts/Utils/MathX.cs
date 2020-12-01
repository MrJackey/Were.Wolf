using UnityEngine;

public static class MathX {
	public static Vector2 Floor(Vector2 vec) {
		return new Vector2(Mathf.Floor(vec.x), Mathf.Floor(vec.y));
	}

	public static Vector2 Ceil(Vector2 vec) {
		return new Vector2(Mathf.Ceil(vec.x), Mathf.Ceil(vec.y));
	}

	/// <summary>
	/// Rotate a vector by an angle (in radians).
	/// </summary>
	public static Vector2 Rotate(Vector2 vec, float angle) {
		float cos = Mathf.Cos(angle);
		float sin = Mathf.Sin(angle);
		return new Vector2(cos * vec.x - sin * vec.y,
		                   sin * vec.x + cos * vec.y);
	}

	public static float Remap(float value, float min1, float max1, float min2, float max2) {
		return (value - min2) * (max2 - min2) / (max1 - min1) + min2;
	}

	public static Vector2 ClosestPointOnLineSegment(Vector2 point, Vector2 start, Vector2 end) {
		Vector2 direction = end - start;
		float length = direction.magnitude;
		direction /= length; // normalize

		float projectedLength = Vector2.Dot(point - start, direction);
		projectedLength = Mathf.Clamp(projectedLength, 0f, length);
		return start + direction * projectedLength;
	}

	public static float InverseLerp(Vector2 point, Vector2 start, Vector2 end) {
		Vector2 direction = end - start;
		float length = direction.magnitude;
		direction /= length; // normalize

		float projectedLength = Vector2.Dot(point - start, direction);
		projectedLength = Mathf.Clamp(projectedLength, 0f, length);
		return projectedLength / length;
	}
}
