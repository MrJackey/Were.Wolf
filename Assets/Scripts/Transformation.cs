using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour
{
    //[SerializeField] private bool isTransformed = false;

    [SerializeField] private float transDuration = 1.2f, humanFormDuration = 5f;

    private SpriteRenderer mySpriteRend;

	private TransformationStates transStates;

    private Color humanColor = Color.white, wolfColor = Color.black;

	private float transTimer, humanFormTimer;


    private void Start() {
		mySpriteRend = gameObject.GetComponent<SpriteRenderer>();
    }


    private void Update() {
		if (Input.GetKeyDown(KeyCode.R) && transStates == TransformationStates.Wolf) {
            StartCoroutine(CoTransforming());
			Debug.Log("Transformation process Charging...");
        }
		else if (Input.GetKeyDown(KeyCode.R) && transStates != TransformationStates.Wolf) {
			Debug.Log("Already transformed to Human - cannot cancel human-form at the moment.");
		}

        if (transStates == TransformationStates.Human) {
			  mySpriteRend.color = humanColor;
        }
        else if (transStates == TransformationStates.Wolf) {
			mySpriteRend.color = wolfColor;
        }
    }


	private IEnumerator CoTransforming() {
    
		for (transTimer = transDuration; transTimer > 0 ; transTimer -= Time.deltaTime) {
			if (Input.GetKeyUp(KeyCode.R) && transStates == TransformationStates.Wolf) {
				Debug.Log("Transformation process interrupted");
				yield break;
			}
			yield return null;
		}

    	if (transStates == TransformationStates.Wolf) {
			TransformToHuman();
			StartCoroutine(HumanFormDuration());
		}
		else if (transStates == TransformationStates.Human) {
			TransformToWolf();
		}
	}


	private IEnumerator HumanFormDuration() {
		Debug.Log("Starting countdown for the human-form duration " + humanFormDuration);

		yield return new WaitForSeconds(humanFormDuration);

		TransformToWolf();
	}


	public void TransformToHuman() {
		transStates = TransformationStates.Human;
		Debug.Log("Transforming to Human");
	}

	public void TransformToWolf() {
		transStates = TransformationStates.Wolf;
		Debug.Log("Transforming back to Wolf");
	}
}

enum TransformationStates {
	Wolf,
	Human,	
}