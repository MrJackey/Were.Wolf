using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour {

	[SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

	private SpriteRenderer mySpriteRend;

	private TransformationStates transStates;

	private Color humanColor = Color.white, wolfColor = Color.black, transformingColor = Color.grey;

	private float transTimer, humanFormTimer;

	private bool isTransforming = false;


	private void Start() {
		mySpriteRend = gameObject.GetComponent<SpriteRenderer>();
	}


	private void Update() {
		if (Input.GetKeyDown(KeyCode.R) && transStates == TransformationStates.Wolf) {
			StartCoroutine(CoTransforming());
		}

		if (transStates == TransformationStates.Human) {
			mySpriteRend.color = humanColor;
		}
		else if (isTransforming) {
			mySpriteRend.color = transformingColor;
		}
		else if (transStates == TransformationStates.Wolf) {
			mySpriteRend.color = wolfColor;
		}
	}


	private IEnumerator CoTransforming() {
		isTransforming = true;
	
		for (transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetKeyUp(KeyCode.R) && isTransforming) {
				isTransforming = false;
				transStates = TransformationStates.Wolf;
				yield break;
			}
			yield return null;
		}

		isTransforming = false;

		if (transStates == TransformationStates.Wolf) {
			TransformToHuman();
			StartCoroutine(CoHumanFormDuration());
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
	}

	public void TransformToWolf() {
		transStates = TransformationStates.Wolf;
	}
}

enum TransformationStates {
	Wolf,
	Transforming,
	Human,	
}