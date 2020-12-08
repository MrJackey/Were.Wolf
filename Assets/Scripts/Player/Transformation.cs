using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Transformation : MonoBehaviour {
	[Header("Values")]
	[SerializeField] private float transDuration = 1.2f;
	[SerializeField] private float humanFormDuration = 5f;
	[SerializeField, Range(0, 1)]
	private float cancelThreshold = 0.5f;
	[SerializeField] private float transformCooldownDuration = 5f;

	[Header("Colliders")]
	[SerializeField] private BoxCollider2D hitCollider = null;
	[SerializeField] private BoxCollider2D groundCollider = null;
	[SerializeField] private BoxCollider2D wolfHitCollider = null;
	[SerializeField] private BoxCollider2D wolfGroundCollider = null;
	[SerializeField] private BoxCollider2D humanHitCollider = null;
	[SerializeField] private BoxCollider2D humanGroundCollider = null;

	[Header("Particles")]
	[SerializeField] private ParticleSystem transformParticle = null;
	[SerializeField, Range(0, 1)] private float toWolfIndication = 0.75f;
	[SerializeField] private float humanEmissionRate = 5f;

	[Header("Events")]
	[SerializeField] private UnityEvent onTransformStart = null;
	[SerializeField] private UnityEvent<float> onTransformInterrupt = null;
	[SerializeField] private UnityEvent onTransformEnd = null;

	private PlayerController playerController;
	private TransformationState oldState;
	private TransformationState state;
	private Coroutine humanFormDurationCoroutine;
	private float transformationCooldownTimer;
	private bool transformationIsBlocked = false;

	private ParticleSystem particleEffect;
	private float wolfEmissionRate;

	private bool transformInputDown, transformInputUp;

	public TransformationState OldState => oldState;
	public TransformationState State {
		get => state;
		set => state = value;
	}

	public UnityEvent<float> OnTransformInterrupt => onTransformInterrupt;
	public bool IsHuman => state == TransformationState.Human;
	public bool IsTransforming => state == TransformationState.Transforming;
	public float TransformDuration => transDuration;

	public float TransformCooldownDuration => transformCooldownDuration;

	public float TransformationCooldown {
		get => transformationCooldownTimer;
		set => transformationCooldownTimer = value;
	}

	public bool AllowTransformation { get; set; } = true;

	private void Start() {
		playerController = GetComponent<PlayerController>();
		particleEffect = Instantiate(transformParticle, transform);
		wolfEmissionRate = particleEffect.emission.rateOverTime.constant;
		particleEffect.Stop();
	}

	private void Update() {
		if (transformationIsBlocked && playerController.IsClearAbove) {
			transformationIsBlocked = false;
			TransformToWolf();
		}

		transformationCooldownTimer = Mathf.Max(transformationCooldownTimer - Time.deltaTime, 0);
	}

	private void LateUpdate() {
		// Must be done in LateUpdate because it is used in a coroutine and they are resumed after Update.
		transformInputUp = false;
	}

	public void OnTransform(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started) {
			if (!playerController.AllowControls || !AllowTransformation) return;
			if (state == TransformationState.Wolf) {
				if (transformationCooldownTimer <= 0f)
					TransformToHuman();
			}
			else if (state == TransformationState.Human) {
				StartCoroutine(CoManualTransformToWolf());
			}
		}
		else if (ctx.phase == InputActionPhase.Canceled) {
			transformInputUp = ctx.canceled;
		}
	}

	private IEnumerator CoTransforming(TransformationState newState, float startTime = 0) {
		if (playerController.IsCrouched && !playerController.IsClearAbove) {
			transformationIsBlocked = true;
			yield break;
		}

		oldState = state;
		state = TransformationState.Transforming;
		PlayParticles();
		onTransformStart.Invoke();

		for (float transTimer = startTime; transTimer < transDuration; transTimer += Time.deltaTime) {
			UpdateHitboxes(newState, transTimer / transDuration);

			if (transformInputUp && oldState == TransformationState.Wolf && transTimer / transDuration < cancelThreshold) {
				state = oldState;
				onTransformInterrupt.Invoke(transTimer);

				yield break;
			}
			yield return null;
		}

		if (newState == TransformationState.Human) {
			if (humanFormDurationCoroutine != null)
				StopCoroutine(humanFormDurationCoroutine);

			humanFormDurationCoroutine = StartCoroutine(CoHumanFormDuration());
		}
		state = newState;

		particleEffect.Stop();
		onTransformEnd.Invoke();
	}

	private IEnumerator CoHumanFormDuration() {
		yield return new WaitForSeconds(humanFormDuration * toWolfIndication);

		PlayParticles();

		yield return new WaitForSeconds(humanFormDuration * (1 - toWolfIndication));
		yield return StartCoroutine(CoTransforming(TransformationState.Wolf));
		transformationCooldownTimer = transformCooldownDuration;
	}

	private void PlayParticles() {
		ParticleSystem.EmissionModule emission = particleEffect.emission;
		float rate = state == TransformationState.Human ? humanEmissionRate : wolfEmissionRate;
		emission.rateOverTime = rate;
		particleEffect.Play();
	}

	public void TransformToHuman() {
		StartCoroutine(CoTransforming(TransformationState.Human));
	}

	public void TransformToWolf(float startTime = 0) {
		StartCoroutine(CoTransforming(TransformationState.Wolf, startTime));
	}

	private IEnumerator CoManualTransformToWolf() {
		if (humanFormDurationCoroutine != null) {
			StopCoroutine(humanFormDurationCoroutine);
			humanFormDurationCoroutine = null;
		}

		yield return StartCoroutine(CoTransforming(TransformationState.Wolf));
		transformationCooldownTimer = transformCooldownDuration;
	}

	private void UpdateHitboxes(TransformationState newState, float transformationTime) {
		Vector2 newHitSize;
		Vector2 newHitOffset;
		Vector2 newGroundSize;
		Vector2 newGroundOffset;

		if (newState == TransformationState.Human) {
			newHitSize = Vector2.Lerp(wolfHitCollider.size, humanHitCollider.size, transformationTime);
			newHitOffset = Vector2.Lerp(wolfHitCollider.offset, humanHitCollider.offset, transformationTime);
			newGroundSize = Vector2.Lerp(wolfGroundCollider.size, humanGroundCollider.size, transformationTime);
			newGroundOffset = Vector2.Lerp(wolfGroundCollider.offset, humanGroundCollider.offset, transformationTime);
		}
		else {
			newHitSize = Vector2.Lerp(humanHitCollider.size, wolfHitCollider.size, transformationTime);
			newHitOffset = Vector2.Lerp(humanHitCollider.offset, wolfHitCollider.offset, transformationTime);
			newGroundSize = Vector2.Lerp(humanGroundCollider.size, wolfGroundCollider.size, transformationTime);
			newGroundOffset = Vector2.Lerp(humanGroundCollider.offset, wolfGroundCollider.offset, transformationTime);
		}

		hitCollider.size = newHitSize;
		hitCollider.offset = newHitOffset;
		groundCollider.size = newGroundSize;
		groundCollider.offset = newGroundOffset;
	}
}

public enum TransformationState {
	Wolf,
	Transforming,
	Human,
}
