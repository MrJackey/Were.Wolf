using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltPlayer : MonoBehaviour {
	[SerializeField] private PlayerController playerController;
	[SerializeField] private Rigidbody2D playerRb2d;
	[SerializeField] private Animator playerAnimator;

	[SerializeField] private bool hasMetOnce = false;
	[SerializeField] private bool doStopAnimator = false;

	private void Start() {
		hasMetOnce = false;
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (other.CompareTag("PlayerCollider") && playerController.IsGrounded && !hasMetOnce) {
			playerRb2d.velocity = Vector2.zero;
			playerController.enabled = false;
			hasMetOnce = true;
			
			//####### Options for how to hold the player: ##############
				// ### 1: ###
				if (doStopAnimator) {
					playerAnimator.enabled = false;
				}

				// ### 2: ###
				else {
					playerAnimator.SetFloat("speed", 0.0f);
					playerAnimator.SetBool("isDashing", false);
					playerAnimator.SetFloat("speedY", 0.0f);
				}		
			// #########################################################	

			this.enabled = false;
		}
	}
}
