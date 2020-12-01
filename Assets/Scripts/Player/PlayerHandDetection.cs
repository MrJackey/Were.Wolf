using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandDetection : MonoBehaviour {
	[SerializeField] private GameObject interactArrow;
	private Interactable detectedInteractItem;
	public Interactable DetectedInteractItem => detectedInteractItem;
	private GameObject newInteractArrow;

	private void OnTriggerEnter2D(Collider2D other) {
		Interactable interactable = other.GetComponent<Interactable>();
		if (interactable != null) {
			newInteractArrow = Instantiate(interactArrow);
			newInteractArrow.transform.position = interactable.transform.position + new Vector3(0, interactable.InteractableArrowHeight, 0);
		}
	}

	public void OnTriggerStay2D(Collider2D collider) {
		Interactable interactable = collider.GetComponent<Interactable>();
		if (interactable != null) {
			detectedInteractItem = interactable;
			newInteractArrow.GetComponent<InteractArrow>().BobTheArrow();
		}
	}

	public void OnTriggerExit2D(Collider2D collider) {
		if (collider.GetComponent<Interactable>() != null) {
			detectedInteractItem = null;
			if (newInteractArrow != null)
				Destroy(newInteractArrow);
		}
	}
}