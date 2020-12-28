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

	public static float Angle(Vector2 vec) {
		return Mathf.Atan2(vec.y, vec.x);
	}

	public static float Remap(float value, float min1, float max1, float min2, float max2) {
		return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
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


	// https://gist.github.com/cjddmut/d789b9eb78216998e95c
	public static float EaseInQuad(float start, float end, float value) {
		end -= start;
		return end * value * value + start;
	}

	public static float EaseOutQuad(float start, float end, float value) {
		end -= start;
		return -end * value * (value - 2) + start;
	}

	public static float EaseInOutQuad(float start, float end, float value) {
		value /= .5f;
		end -= start;
		if (value < 1) return end * 0.5f * value * value + start;
		value--;
		return -end * 0.5f * (value * (value - 2) - 1) + start;
	}


	//  20 dB <=> 10
	//   0 dB <=> 1
	// -80 dB <=> 0.0001
	// https://en.wikipedia.org/wiki/Decibel
	public static float DecibelsToLinear(float db) {
		return Mathf.Pow(10, db / 20);
	}

	public static float LinearToDecibels(float linear) {
		const float min = 0.0001f;
		if (linear < min)
			linear = min;

		return 20 * Mathf.Log10(linear);
	}
}
