using UnityEngine;

public static class MathX {
	public static Vector2 Floor(Vector2 vec) {
		return new Vector2(Mathf.Floor(vec.x), Mathf.Floor(vec.y));
	}

	public static Vector2 Ceil(Vector2 vec) {
		return new Vector2(Mathf.Ceil(vec.x), Mathf.Ceil(vec.y));
	}
}