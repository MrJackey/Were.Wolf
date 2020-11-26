using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandDetection : MonoBehaviour {
	public GameObject detectedInteractItem;

	public void OnTriggerStay2D(Collider2D collider) {
		if (!collider.gameObject.CompareTag("PressureButton")) {
			detectedInteractItem = collider.gameObject;
		}
	}

	public void OnTriggerExit2D(Collider2D collider) {
		detectedInteractItem = null;
	}
}