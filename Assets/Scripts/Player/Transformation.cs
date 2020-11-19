using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Transformation : MonoBehaviour {

	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	[SerializeField] private UnityEvent onTransform;

	private SpriteRenderer mySpriteRend;

	private PlayerController playerController;

	private TransformationStates transformationState;

	private Coroutine humanFormDurationCoroutine;

	private Color humanColor = Color.white, wolfColor = Color.black, transformingColor = Color.grey;

	public TransformationStates TransformationState => transformationState;


	private void Start() {
		mySpriteRend = gameObject.GetComponent<SpriteRenderer>();
		playerController = GetComponent<PlayerController>();

		mySpriteRend.color = wolfColor;
	}


	private void Update() {
		if (Input.GetButtonDown("Transformation") && playerController.IsGrounded && transformationState == TransformationStates.Wolf) {
			TransformToHuman();
		}
	}


	private IEnumerator CoTransforming(TransformationStates newState, Color newColor) {
		onTransform.Invoke();
		mySpriteRend.color = transformingColor;
		playerController.AllowControls = false;

		for (float transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetButtonUp("Transformation") && transformationState == TransformationStates.Wolf) {
				playerController.AllowControls = true;

				if (transformationState == TransformationStates.Wolf) {
					mySpriteRend.color = wolfColor;
				}
				else if (transformationState == TransformationStates.Human) {
					mySpriteRend.color = humanColor;
				}
				yield break;
			}
			yield return null;
		}

		mySpriteRend.color = newColor;

		if (newState == TransformationStates.Human) {
			if (humanFormDurationCoroutine != null)
				StopCoroutine(humanFormDurationCoroutine);

			humanFormDurationCoroutine = StartCoroutine(CoHumanFormDuration());
		}
		transformationState = newState;
		playerController.AllowControls = true;
	}


	private IEnumerator CoHumanFormDuration() {
		yield return new WaitForSeconds(humanFormDuration);

		StartCoroutine(CoTransforming(TransformationStates.Wolf, wolfColor));
	}


	public void TransformToHuman() {
		StartCoroutine(CoTransforming(TransformationStates.Human, humanColor));
	}


	public void TransformToWolf() {
		StartCoroutine(CoTransforming(TransformationStates.Wolf, wolfColor));
	}
}

public enum TransformationStates {
	Wolf,
	Human,
}
