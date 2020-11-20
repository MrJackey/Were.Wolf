using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Transformation : MonoBehaviour {
	// TODO: Find out a good way to get this value.
	private const float TransformationAnimationLength = 1.5f;

	private static readonly int transformSpeedId = Animator.StringToHash("transformSpeed");
	private static readonly int isTransformingId = Animator.StringToHash("isTransforming");
	private static readonly int isHumanId = Animator.StringToHash("isHuman");

	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	[Header("Events")]
	[SerializeField] private UnityEvent onTransformStart;

	[SerializeField] private UnityEvent onTransformInterrupt;

	[SerializeField] private UnityEvent onTransformEnd;


	private PlayerController playerController;

	private TransformationStates transformationState;

	private Coroutine humanFormDurationCoroutine;

	public TransformationStates TransformationState => transformationState;

	public float TransformDuration => transDuration;


	private void Start() {
		playerController = GetComponent<PlayerController>();
	}


	private void Update() {
		if (Input.GetButtonDown("Transformation") && playerController.IsGrounded && transformationState == TransformationStates.Wolf) {
			TransformToHuman();
		}
	}


	private IEnumerator CoTransforming(TransformationStates newState) {
		onTransformStart.Invoke();

		for (float transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetButtonUp("Transformation") && transformationState == TransformationStates.Wolf) {
				onTransformInterrupt.Invoke();

				yield break;
			}
			yield return null;
		}

		if (newState == TransformationStates.Human) {
			if (humanFormDurationCoroutine != null)
				StopCoroutine(humanFormDurationCoroutine);

			humanFormDurationCoroutine = StartCoroutine(CoHumanFormDuration());
		}
		transformationState = newState;
		onTransformEnd.Invoke();
	}


	private IEnumerator CoHumanFormDuration() {
		yield return new WaitForSeconds(humanFormDuration);

		StartCoroutine(CoTransforming(TransformationStates.Wolf));
	}


	public void TransformToHuman() {
		StartCoroutine(CoTransforming(TransformationStates.Human));
	}


	public void TransformToWolf() {
		StartCoroutine(CoTransforming(TransformationStates.Wolf));
	}
}

public enum TransformationStates {
	Wolf,
	Human,
}
