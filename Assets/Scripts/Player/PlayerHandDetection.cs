using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandDetection : MonoBehaviour {
	private Interactable detectedInteractItem;
	public Interactable DetectedInteractItem => detectedInteractItem;

	public void OnTriggerStay2D(Collider2D collider) {
		Interactable interactable = collider.GetComponent<Interactable>();
		if (interactable != null) {
			detectedInteractItem = interactable;
		}
	}

	public void OnTriggerExit2D(Collider2D collider) {
		if (collider.GetComponent<Interactable>() != null) {
			detectedInteractItem = null;
		}
	}
}