using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour {
	[SerializeField] private GameObject playerHand;

	private PlayerHandDetection playerHandDetection;
	private GameObject interactItem;
	private GameObject carriedItem;

	private bool isCarryingItem = false;


	private void Start() {
		playerHandDetection = playerHand.GetComponent<PlayerHandDetection>();
	}

	// Lever does not change animation-clip
	private void Update() {
		if (Input.GetButtonDown("Interact")) {
			if (isCarryingItem) {
				DropItem();	
			}
			else if (playerHandDetection.detectedInteractItem != null) {
				
				interactItem = playerHandDetection.detectedInteractItem;

				if (interactItem.CompareTag("Box")) {
					PickUpItem();
				}
				else if (interactItem.CompareTag("Lever")) {
					Lever lever = interactItem.GetComponent<Lever>();

					if (lever.IsActivated == false) 
						lever.Activate();
					else
						lever.Deactivate();
				}
			}
		}
	}

	private void PickUpItem() {
		isCarryingItem = true;
		carriedItem = interactItem;
		interactItem = null;
		carriedItem.GetComponent<Box>().boxCollider.enabled = false;
		carriedItem.GetComponent<Rigidbody2D>().isKinematic = true;
		carriedItem.transform.parent = playerHand.transform;
		carriedItem.transform.localPosition = Vector3.zero;
	}

	private void DropItem() {
		isCarryingItem = false;
		carriedItem.GetComponent<Box>().boxCollider.enabled = true;
		carriedItem.GetComponent<Rigidbody2D>().isKinematic = false;
		carriedItem.transform.parent = null;
		carriedItem = null;
	}
}