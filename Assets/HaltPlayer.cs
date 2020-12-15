using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltPlayer : MonoBehaviour {
	[SerializeField] private PlayerController playerController;
	[SerializeField] private Rigidbody2D playerRb2d;
	[SerializeField] private Animator playerAnimator;
	[SerializeField] private BoxCollider2D hasMetTrigger;

	[SerializeField] private bool hasMetOnce = false;

	private void Start() {
		hasMetOnce = false;
		hasMetTrigger.enabled = false;
	}

	private void OnTriggerStay2D(Collider2D other) {

		if (other.CompareTag("PlayerCollider") && playerController.IsGrounded && !hasMetOnce) {
			playerRb2d.velocity = Vector2.zero;
			playerController.enabled = false;
		
			playerAnimator.SetFloat("speed", 0.0f);
			playerAnimator.SetBool("isDashing", false);
			playerAnimator.SetFloat("speedY", 0.0f);
			
			hasMetTrigger.enabled = true;
			hasMetOnce = true;
			this.enabled = false;
		}
	}
}
