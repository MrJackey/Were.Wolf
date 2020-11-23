using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Transformation : MonoBehaviour {
	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	[Header("Events")]
	[SerializeField] private UnityEvent onTransformStart;

	[SerializeField] private UnityEvent onTransformInterrupt;

	[SerializeField] private UnityEvent onTransformEnd;


	private PlayerController playerController;
	private TransformationState oldState;
	private TransformationState state;

	private Coroutine humanFormDurationCoroutine;

	public TransformationState OldState => oldState;
	public TransformationState State => state;

	public float TransformDuration => transDuration;

	private void Start() {
		playerController = GetComponent<PlayerController>();
	}


	private void Update() {
		if (Input.GetButtonDown("Transformation") && playerController.IsGrounded && state == TransformationState.Wolf) {
			TransformToHuman();
		}
	}


	private IEnumerator CoTransforming(TransformationState newState) {
		oldState = state;
		state = TransformationState.Transforming;
		onTransformStart.Invoke();

		for (float transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetButtonUp("Transformation") && oldState == TransformationState.Wolf) {
				state = oldState;
				onTransformInterrupt.Invoke();

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


	public void TransformToWolf() {
		StartCoroutine(CoTransforming(TransformationState.Wolf));
	}
}

public enum TransformationState {
	Wolf,
	Transforming,
	Human,
}
