using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
	[SerializeField] private GameObject playerHand;

	private PlayerHandDetection playerHandDetection;
	private GameObject interactItem;
	private GameObject heldBox;

	private bool isCarryingItem = false;


	private void Start() {
		playerHandDetection = playerHand.GetComponent<PlayerHandDetection>();
	}

	// Lever does not change animation-clip
	private void Update() {
		if (Input.GetButtonDown("Interact")) {
			if (playerHandDetection.detectedInteractItem != null && !isCarryingItem) {
				if (playerHandDetection.detectedInteractItem.name != "PressureButton") {
					interactItem = playerHandDetection.detectedInteractItem;

					if (interactItem.tag == "Box") {
						PickUpItem();
					}
					else if (interactItem.tag == "Lever" && interactItem.GetComponent<Lever>().isActive == false) {
						interactItem.GetComponent<Lever>().ActivateLever();
					}
					else if (interactItem.tag == "Lever" && interactItem.GetComponent<Lever>().isActive == true) {
						interactItem.GetComponent<Lever>().DeactivateLever();
					}
				}
			}
			else if (isCarryingItem) {
				DropItem();	
				print("dropping item");

			}
			
			print(interactItem);
		}
	}

	private void PickUpItem() {
		isCarryingItem = true;
		interactItem.GetComponent<Box>().boxCollider.enabled = false;
		interactItem.GetComponent<Rigidbody2D>().isKinematic = true;
		interactItem.transform.parent = playerHand.transform;
		interactItem.transform.localPosition = Vector3.zero;
	}

	private void DropItem() {
		isCarryingItem = false;
		interactItem.GetComponent<Box>().boxCollider.enabled = true;
		interactItem.GetComponent<Rigidbody2D>().isKinematic = false;
		heldBox = interactItem;
		heldBox.transform.parent = null;
		interactItem = null;
		heldBox = null;
	}
}