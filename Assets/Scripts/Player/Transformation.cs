using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Transformation : MonoBehaviour {
	[Header("Values")]
	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	[Header("Colliders")]
	[SerializeField] private BoxCollider2D hitCollider;
	[SerializeField] private BoxCollider2D groundCollider;

	[SerializeField] private BoxCollider2D wolfHitCollider;
	[SerializeField] private BoxCollider2D wolfGroundCollider;
	[SerializeField] private BoxCollider2D humanHitCollider;
	[SerializeField] private BoxCollider2D humanGroundCollider;

	[Header("Events")]
	[SerializeField] private UnityEvent onTransformStart;
	[SerializeField] private UnityEvent<float> onTransformInterrupt;
	[SerializeField] private UnityEvent onTransformEnd;

	private PlayerController playerController;
	private TransformationState oldState;
	private TransformationState state;

	private Coroutine humanFormDurationCoroutine;

	public TransformationState OldState => oldState;

	public TransformationState State {
		get => state;
		set => state = value;
	}

	public UnityEvent<float> OnTransformInterrupt => onTransformInterrupt;

	public float TransformDuration => transDuration;

	private void Start() {
		playerController = GetComponent<PlayerController>();
	}

	private void Update() {
		if (Input.GetButtonDown("Transformation") && playerController.IsGrounded && state == TransformationState.Wolf) {
			TransformToHuman();
		}
	}

	private IEnumerator CoTransforming(TransformationState newState, float startTime = 0) {
		oldState = state;
		state = TransformationState.Transforming;
		onTransformStart.Invoke();

		for (float transTimer = startTime; transTimer < transDuration; transTimer += Time.deltaTime) {
			UpdateHitboxes(newState, transTimer / transDuration);

			if (Input.GetButtonUp("Transformation") && oldState == TransformationState.Wolf) {
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
		onTransformEnd.Invoke();
	}

	private IEnumerator CoHumanFormDuration() {
		yield return new WaitForSeconds(humanFormDuration);

		StartCoroutine(CoTransforming(TransformationState.Wolf));
	}

	public void TransformToHuman() {
		StartCoroutine(CoTransforming(TransformationState.Human));
	}

	public void TransformToWolf(float startTime = 0) {
		StartCoroutine(CoTransforming(TransformationState.Wolf, startTime));
	}

	private void UpdateHitboxes(TransformationState newState, float transformationTime) {
		Vector2 newHitSize;
		Vector2 newGroundSize;

		if (newState == TransformationState.Human) {
			newHitSize = Vector2.Lerp(wolfHitCollider.size, humanHitCollider.size, transformationTime);
			newGroundSize = Vector2.Lerp(wolfGroundCollider.size, humanGroundCollider.size, transformationTime);
		}
		else {
			newHitSize = Vector2.Lerp(humanHitCollider.size, wolfHitCollider.size, transformationTime);
			newGroundSize = Vector2.Lerp(humanGroundCollider.size, wolfGroundCollider.size, transformationTime);
		}

		hitCollider.size = newHitSize;
		groundCollider.size = newGroundSize;
	}
}

public enum TransformationState {
	Wolf,
	Transforming,
	Human,
}
