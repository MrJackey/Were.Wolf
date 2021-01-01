using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : SingletonBehaviour<RumbleManager> {
	private Gamepad currentGamepad;
	private bool isRumbling;
	private bool isEndlessRuble;

	protected override void Awake() {
		DontDestroyOnLoad(this);
		base.Awake();
	}

	public void StartRumble(float lowFrequency, float highFrequency, float duration) {
		if (isRumbling) {
			if (isEndlessRuble) return; // Endless takes priority
			StopRumble();
		}

		currentGamepad = Gamepad.current;
		if (currentGamepad == null) return;

		isRumbling = true;
		isEndlessRuble = false;
		StartCoroutine(CoRumbleDuration(lowFrequency, highFrequency, duration));
	}

	public void StartRumble(float lowFrequency, float highFrequency) {
		StopRumble();
		currentGamepad = Gamepad.current;
		if (currentGamepad == null) return;

		isRumbling = true;
		isEndlessRuble = true;
		currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
	}

	public void StopRumble() {
		if (!isRumbling) return;
		if (!isEndlessRuble)
			StopAllCoroutines();
		currentGamepad?.ResetHaptics();
		isRumbling = isEndlessRuble = false;
	}

	private IEnumerator CoRumbleDuration(float lowFrequency, float highFrequency, float duration) {
		currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
		yield return new WaitForSecondsRealtime(duration);
		currentGamepad.ResetHaptics();
	}


	[RuntimeInitializeOnLoadMethod]
	private static void OnRuntimeLoad() {
		_ = new GameObject("Rumble Manager", typeof(RumbleManager));
	}
}