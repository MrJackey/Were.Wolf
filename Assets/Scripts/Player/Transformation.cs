using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Transformation : MonoBehaviour {
	[Header("Values")]
	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	[Header("Colliders")]
	[SerializeField] private GameObject wolfColliders;
	[SerializeField] private Collider2D wolfGroundCollider;
	[SerializeField] private GameObject humanColliders;
	[SerializeField] private Collider2D humanGroundCollider;

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
		UpdateHitboxes();
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

	private void UpdateHitboxes() {
		if (state == TransformationState.Wolf) {
			wolfColliders.SetActive(true);
			playerController.GroundedCollider = wolfGroundCollider;
			humanColliders.SetActive(false);
		}
		else {
			humanColliders.SetActive(true);
			playerController.GroundedCollider = humanGroundCollider;
			wolfColliders.SetActive(false);
		}
	}
}

public enum TransformationState {
	Wolf,
	Transforming,
	Human,
}
