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
}