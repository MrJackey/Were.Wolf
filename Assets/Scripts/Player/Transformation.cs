using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Transformation : MonoBehaviour {
	[SerializeField] private PlayerAnimationController playerAnimationController;

	[Header("Values")]
	[SerializeField] private float transDuration = 1.2f;
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

	[Header("Sounds")]
	[SerializeField] private AudioSource transformToHumanSound = null;
	[SerializeField] private AudioSource transformToWerewolfSound = null;
	[SerializeField] private SoundRandomizer wolfHurtSound;
	[SerializeField] private SoundRandomizer humanHurtSound;

	[Header("Events")]
	[SerializeField] private UnityEvent onTransformStart = null;
	[SerializeField] private UnityEvent<float> onTransformInterrupt = null;
	[SerializeField] private UnityEvent onTransformEnd = null;

	private PlayerController playerController;
	private TransformationState oldState;
	private TransformationState state;
	private Coroutine humanFormDurationCoroutine;
	private bool transformationIsBlocked = false;
	private float transformationCooldownTimer;
	private bool isCoolingDown = true;
	private bool isInterrupted = false;

	private ParticleSystem particleEffect;

	private bool transformInputDown, transformInputUp;

	public TransformationState OldState => oldState;
	public TransformationState State => state;
	public UnityEvent<float> OnTransformInterrupt => onTransformInterrupt;
	public UnityEvent OnTransformEnd => onTransformEnd;
	public bool IsHuman => state == TransformationState.Human ||
	                       (state == TransformationState.Transforming && oldState == TransformationState.Human);
	public bool IsTransforming => state == TransformationState.Transforming;
	public float TransformDuration => transDuration;
	public float TransformCooldownDuration => transformCooldownDuration;

	public float TransformationCooldown {
		get => transformationCooldownTimer;
		set => transformationCooldownTimer = value;
	}

	public bool AllowTransformation { get; set; } = true;

	public BoxCollider2D HitCollider => hitCollider;

	private void Start() {
		playerController = GetComponent<PlayerController>();
		particleEffect = Instantiate(transformParticle, transform);
		particleEffect.Stop();
	}

	private void Update() {
		if (transformationIsBlocked && playerController.IsClearAbove) {
			transformationIsBlocked = false;
			TransformToWolf();
		}

		if (isCoolingDown)
			transformationCooldownTimer = Mathf.Max(transformationCooldownTimer - Time.deltaTime, 0);
	}

	private void LateUpdate() {
		// Must be done in LateUpdate because it is used in a coroutine and they are resumed after Update.
		transformInputUp = false;
	}

	public void OnTransform(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started) {
			if (!playerController.AllowControls || !AllowTransformation) return;

			if (transformationCooldownTimer <= 0f) {
				if (state == TransformationState.Wolf)
					TransformToHuman();
			}

			if (state == TransformationState.Human)
				TransformToWolf();
		}
		else if (ctx.phase == InputActionPhase.Canceled) {
			transformInputUp = ctx.canceled;
		}
	}

	public void TransformToHuman(float startTime = 0) {
		StartCoroutine(CoTransforming(TransformationState.Human, startTime));
	}

	public void TransformToWolf(float startTime = 0) {
		StartCoroutine(CoTransforming(TransformationState.Wolf, startTime));
	}

	private IEnumerator CoTransforming(TransformationState newState, float startTime = 0) {
		PlayTransformationSound(newState, startTime / transDuration);

		if (playerController.IsCrouched && !playerController.IsClearAbove) {
			transformationIsBlocked = true;
			StopSounds();
			yield break;
		}

		oldState = state;
		state = TransformationState.Transforming;
		particleEffect.Play();
		onTransformStart.Invoke();

		isCoolingDown = false;

		for (float transTimer = startTime; transTimer < transDuration; transTimer += Time.deltaTime) {
			float transformationProgress = transTimer / transDuration;

			if (!IsHuman) {
				float cooldownMultiplier = isInterrupted ? (1 - transformationProgress) : transformationProgress;
				transformationCooldownTimer = transformCooldownDuration * cooldownMultiplier;
			}

			UpdateHitboxes(newState, transformationProgress);

			if (transformInputUp && !isInterrupted && transformationProgress < cancelThreshold) {
				isInterrupted = true;

				// Changes the state to newState due to interruption starting another transformation backwards
				state = newState;

				onTransformInterrupt.Invoke(transTimer);

				yield break;
			}
			yield return null;
		}

		UpdateHitboxes(newState, 1);

		if (isInterrupted) {
			transformationCooldownTimer = 0;
			isInterrupted = false;
		}

		if (newState == TransformationState.Human)
			transformationCooldownTimer = transformCooldownDuration;

		isCoolingDown = newState != TransformationState.Human;
		state = newState;

		particleEffect.Stop();
		onTransformEnd.Invoke();
	}

	private void PlayTransformationSound(TransformationState newState, float startTime) {
		StopSounds();
		if (newState == TransformationState.Human) {
			transformToHumanSound.Play();
			transformToHumanSound.time = startTime * transformToHumanSound.clip.length;
		}
		else if (newState == TransformationState.Wolf) {
			transformToWerewolfSound.Play();
			transformToWerewolfSound.time = startTime * transformToWerewolfSound.clip.length;
		}
	}

	private void StopSounds() {
		transformToHumanSound.Stop();
		transformToWerewolfSound.Stop();
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

	public void PlayTakeDamageSound () {
		if (IsHuman)
			humanHurtSound.PlayRandom();
		else
			wolfHurtSound.PlayRandom();
	}

	public void ResetTransformation() {
		state = TransformationState.Wolf;
		oldState = TransformationState.Human;
		transformationCooldownTimer = 0;
		playerAnimationController.Respawn();
	}
}

public enum TransformationState {
	Wolf,
	Transforming,
	Human,
}
