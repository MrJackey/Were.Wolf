using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandDetection : MonoBehaviour {
	[SerializeField] private GameObject interactArrowPrefab;
	private Interactable detectedInteractItem;
	private List<Interactable> interactableList = new List<Interactable>();
	private GameObject interactArrow;
	private InteractArrow interactArrowScript;
	private float lowestDistance = 0;

	public Interactable DetectedInteractItem => detectedInteractItem;

	private void Start() {
		interactArrow = Instantiate(interactArrowPrefab);
		interactArrowScript = interactArrow.GetComponent<InteractArrow>();
		interactArrow.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		Interactable interactable = other.GetComponent<Interactable>();
		if (interactable != null) {
			interactableList.Add(interactable);
			interactArrow.SetActive(true);
		}
	}

	private void Update() {
		if (interactableList.Count == 0)
			return;

		SetClosestItem();
	}

	private void OnTriggerExit2D(Collider2D other) {
		Interactable interactable = other.GetComponent<Interactable>();
		if (interactable != null) {
			interactableList.Remove(interactable);	
			
			if (interactableList.Count == 0) {
				detectedInteractItem = null;
				lowestDistance = 0;
				interactArrow.SetActive(false);
			}
			else {
				detectedInteractItem = null;
				SetClosestItem();
			}
		}	
	}

	private void SetClosestItem() {
		lowestDistance = 0;
		Interactable closestItem = detectedInteractItem;
		for (int i = 0; i < interactableList.Count; i++) {
			Vector3 vectDist = transform.position - interactableList[i].transform.position;
			float distance = vectDist.sqrMagnitude;

			Debug.DrawLine(interactableList[i].transform.position, transform.position);

			if (distance < lowestDistance || lowestDistance == 0) {
				lowestDistance = distance;
				closestItem = interactableList[i];
			}
		}
		if (closestItem == detectedInteractItem) 
			return;

		interactArrow.transform.position = closestItem.transform.position + new Vector3(0, closestItem.InteractableArrowHeight, 0);
		interactArrowScript.Initialize();
		detectedInteractItem = closestItem;
	}
}