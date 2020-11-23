using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPickUp : MonoBehaviour {

	[SerializeField] private BoxCollider2D boxCollider, boxTrigger;
	private bool readyToPickUp = false, isPickedUp = false;
	private Vector2 startPos = new Vector2(3, 3);
	private GameObject playerHand;
	private Rigidbody2D rb2d;


	private void Start() {
		rb2d = GetComponent<Rigidbody2D>();
		rb2d.isKinematic = false;

		playerHand = GameObject.FindGameObjectWithTag("PlayerHand");
	}

	private void Update() {
		if (Input.GetButtonDown("Interact")) {
			if (readyToPickUp) {
				PickUpItem();
			}
			else if (isPickedUp) {
				DropItem();
			}
		}
    }

	private void OnTriggerStay2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			readyToPickUp = true;
		}
		else if (collision.CompareTag("Spike")) {
			transform.position = startPos;
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			readyToPickUp = false;
		}
	}

	private void PickUpItem() {
		isPickedUp = true;
		boxCollider.enabled = false;
		boxTrigger.enabled = false;
		rb2d.isKinematic = true;

		transform.parent = playerHand.transform;
		transform.localPosition = Vector3.zero;
	}

	private void DropItem() {
		boxCollider.enabled = true;
		boxTrigger.enabled = true;
		rb2d.isKinematic = false;
		isPickedUp = false;

		transform.parent = null;
	}
}
