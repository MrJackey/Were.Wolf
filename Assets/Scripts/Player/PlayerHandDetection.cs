﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandDetection : MonoBehaviour {
	[SerializeField] private GameObject interactArrowPrefab;
	[SerializeField] private PlayerCarrying playerCarrying;
	[SerializeField] private PlayerController playerController;
	[SerializeField, Tag] private string priorityInteractableTag;

	private Interactable detectedInteractItem;
	private List<Interactable> interactableList = new List<Interactable>();
	private GameObject interactArrow;
	private InteractArrow interactArrowScript;

	public Interactable DetectedInteractItem => detectedInteractItem;

	private void Start() {
		interactArrow = Instantiate(interactArrowPrefab);
		interactArrowScript = interactArrow.GetComponent<InteractArrow>();
		interactArrow.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		Interactable interactable = other.GetComponent<Interactable>();
		if (interactable != null)
			interactableList.Add(interactable);
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

			detectedInteractItem = null;
			if (interactableList.Count == 0)
				interactArrow.SetActive(false);
		}
	}

	private void SetClosestItem() {
		if (playerCarrying.IsCarryingItem || playerController.IsCrouched) {
			detectedInteractItem = null;
			interactArrow.SetActive(false);
			return;
		}

		if (detectedInteractItem != null && !detectedInteractItem.IsInteractable) {
			detectedInteractItem = null;
			interactArrow.SetActive(false);
		}

		float lowestDistance = float.PositiveInfinity;
		Interactable closestItem = null;

		foreach (Interactable interactable in interactableList) {
			float distance = (transform.position - interactable.transform.position).sqrMagnitude;

			if (interactable.IsInteractable && (distance < lowestDistance ||
				!string.IsNullOrEmpty(priorityInteractableTag) && interactable.CompareTag(priorityInteractableTag))) {
				lowestDistance = distance;
				closestItem = interactable;
			}
		}

		if (closestItem == null || closestItem == detectedInteractItem)
			return;

		interactArrow.transform.parent = closestItem.transform;
		interactArrow.transform.localPosition = new Vector3(0, closestItem.InteractableArrowHeight, 0);
		interactArrowScript.Initialize();
		detectedInteractItem = closestItem;
		interactArrow.SetActive(true);
	}
}
