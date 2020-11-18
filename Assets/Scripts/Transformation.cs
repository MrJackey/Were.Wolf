using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour {

	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	private SpriteRenderer mySpriteRend;

	private TransformationStates transStates;

	private Coroutine humanFormDurationCoroutine;

	private Color humanColor = Color.white, wolfColor = Color.black, transformingColor = Color.grey;

	private float transTimer, humanFormTimer;


	private void Start() {
		mySpriteRend = gameObject.GetComponent<SpriteRenderer>();

		mySpriteRend.color = wolfColor;
	}


	private void Update() {
		if (Input.GetKeyDown(KeyCode.R)) {
			StartCoroutine(CoTransforming());
		}
	}


	private IEnumerator CoTransforming() {
		mySpriteRend.color = transformingColor;

		for (transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetKeyUp(KeyCode.R)) {
				if (transStates == TransformationStates.Wolf) {
					mySpriteRend.color = wolfColor;
				}
				else if (transStates == TransformationStates.Human) {
					mySpriteRend.color = humanColor;
				}
				yield break;
			}
			yield return null;
		}

		if (humanFormDurationCoroutine != null) {
			StopCoroutine(humanFormDurationCoroutine);
		}

		if (transStates == TransformationStates.Wolf) {
			TransformToHuman();
			humanFormDurationCoroutine = StartCoroutine(CoHumanFormDuration());
		}
		else if (transStates == TransformationStates.Human) {
			TransformToWolf();
		}
	}


	private IEnumerator CoHumanFormDuration() {
		yield return new WaitForSeconds(humanFormDuration);

		TransformToWolf();
	}


	public void TransformToHuman() {
		transStates = TransformationStates.Human;
		mySpriteRend.color = humanColor;
	}


	public void TransformToWolf() {
		transStates = TransformationStates.Wolf;
		mySpriteRend.color = wolfColor;
	}
}

enum TransformationStates {
	Wolf,
	Human,	
}