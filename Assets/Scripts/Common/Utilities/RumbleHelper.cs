using UnityEngine;

public class RumbleHelper : MonoBehaviour {
	[SerializeField, Range(0, 1)] private float lowFrequency;
	[SerializeField, Range(0, 1)] private float highFrequency;

	[SerializeField, Min(0)] private float duration;

	public void StartRumble() {
		if (duration > 0)
			RumbleManager.Instance.StartRumble(lowFrequency, highFrequency, duration);
		else
			RumbleManager.Instance.StartRumble(lowFrequency, highFrequency);
	}

	public void StopRumble() {
		RumbleManager.Instance.StopRumble();
	}
}