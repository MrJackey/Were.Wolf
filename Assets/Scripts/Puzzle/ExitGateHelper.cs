using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGateHelper : MonoBehaviour {
	[SerializeField] private SpriteRenderer leftEyeSpriteRenderer;
	[SerializeField] private ParticleSystem leftEyeParticleSystem;
	[SerializeField, Range(1, 6)]
	private int leftEyeCondition = 2;

	[Space]
	[SerializeField] private SpriteRenderer rightEyeSpriteRenderer;
	[SerializeField] private ParticleSystem rightEyeParticleSystem;
	[SerializeField, Range(1, 6)]
	private int rightEyeCondition = 1;

	[Space]
	[SerializeField] private SignalReceiver signalReceiver;

	public void AtStateUpdate() {
		if (signalReceiver.IsActivated) {
			ActivateEye(rightEyeSpriteRenderer, rightEyeParticleSystem);
			ActivateEye(leftEyeSpriteRenderer, leftEyeParticleSystem);
			return;
		}

		int activatedEmitterCount = CountActivatedEmitters();

		if (activatedEmitterCount >= rightEyeCondition)
			ActivateEye(rightEyeSpriteRenderer, rightEyeParticleSystem);
		else
			DeactivateEye(rightEyeSpriteRenderer, rightEyeParticleSystem);

		if (activatedEmitterCount >= leftEyeCondition)
			ActivateEye(leftEyeSpriteRenderer, leftEyeParticleSystem);
		else
			DeactivateEye(leftEyeSpriteRenderer, leftEyeParticleSystem);
	}

	private int CountActivatedEmitters() {
		int activatedEmitterCount = 0;
		foreach (SignalEmitter emitter in signalReceiver.Emitters) {
			if (emitter == null) continue;

			activatedEmitterCount += emitter.IsActivated ? 1 : 0;
		}

		return activatedEmitterCount;
	}

	private void ActivateEye(SpriteRenderer sprite, ParticleSystem particles) {
		sprite.enabled = true;
		particles.Play();
	}

	private void DeactivateEye(SpriteRenderer sprite, ParticleSystem particles) {
		sprite.enabled = false;
		particles.Stop();
	}
}
