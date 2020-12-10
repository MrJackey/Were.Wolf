using System;
using UnityEngine;

[Serializable]
public struct SimpleTimer {
	public float Value { get; private set; }

	public bool Elapsed => Value <= 0f;

	/// <summary>
	/// Ticks the timer and returns whether the tick caused the timer to elapse.
	/// </summary>
	public bool Tick() {
		if (Elapsed) return false;
		Value = Mathf.Max(Value - Time.deltaTime, 0);
		return Elapsed;
	}

	public void Stop() {
		Value = 0;
	}

	public void Reset(float duration) {
		Value = duration;
	}
}