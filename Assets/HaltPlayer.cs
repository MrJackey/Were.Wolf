using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltPlayer : MonoBehaviour {
	[SerializeField] private BoxCollider2D hasMetTrigger;

	private PlayerController playerController;
	private bool hasMetOnce = false;

	private void Start() {
		playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
		hasMetOnce = false;
		hasMetTrigger.enabled = false;
	}

	private void OnTriggerStay2D(Collider2D other) {

		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger && playerController.IsGrounded && !hasMetOnce) {
			playerController.AllowControls = false;
			
			hasMetTrigger.enabled = true;
			hasMetOnce = true;
			this.enabled = false;
		}
	}
}
