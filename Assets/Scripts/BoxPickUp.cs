using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPickUp : MonoBehaviour {

	[SerializeField] private BoxCollider2D boxCollider, boxTrigger;
	[SerializeField] private bool readyToPickUp = false; 
	[SerializeField] public GameObject playerHand;
	
	public bool isPickedUp = false;

	private Rigidbody2D rb2d;


	private void Start() {
		rb2d = GetComponent<Rigidbody2D>();
		rb2d.isKinematic = false;

		playerHand = GameObject.FindGameObjectWithTag("PlayerHand");
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.E)) {
			if (readyToPickUp) {
				isPickedUp = true;
				boxCollider.enabled = false;
				boxTrigger.enabled = false;
				rb2d.isKinematic = true;

				transform.parent = playerHand.transform;
				transform.localPosition = Vector3.zero;
			}
			else if (isPickedUp) {
				boxCollider.enabled = true;
				boxTrigger.enabled = true;
				rb2d.isKinematic = false;
				isPickedUp = false;

				transform.parent = null;
			}
		}
    }

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			readyToPickUp = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			readyToPickUp = false;
		}
	}
}
