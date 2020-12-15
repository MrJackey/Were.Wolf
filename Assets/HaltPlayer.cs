using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltPlayer : MonoBehaviour {
	[SerializeField] private PlayerController playerController;
	[SerializeField] private Rigidbody2D playerRb2d;
	[SerializeField] private Animator playerAnimator;

	[SerializeField] private bool hasMetOnce = false;
	[SerializeField] private bool doStopAnimator = false;
	[SerializeField] private bool mustBeGrounded = true;

	private void Start() {
		hasMetOnce = false;
	}

	private void OnTriggerStay2D(Collider2D other) {

	//####### Options for how to hold the player: ##############
		if (mustBeGrounded) {
			if (other.CompareTag("PlayerCollider") && playerController.IsGrounded && !hasMetOnce) {
				playerRb2d.velocity = Vector2.zero;
				playerController.enabled = false;
				hasMetOnce = true;
			
				if (doStopAnimator) {
					playerAnimator.enabled = false;
				}			
				else {
					playerAnimator.SetFloat("speed", 0.0f);
					playerAnimator.SetBool("isDashing", false);
					playerAnimator.SetFloat("speedY", 0.0f);
				}

				this.enabled = false;
			}		
		}
		// #########################################################	
		else {
			if (other.CompareTag("PlayerCollider") && !hasMetOnce) {
				playerRb2d.velocity = Vector2.zero;
				playerController.enabled = false;
				hasMetOnce = true;

				if (doStopAnimator) {
					playerAnimator.enabled = false;
				}
				else {
					playerAnimator.SetFloat("speed", 0.0f);
					playerAnimator.SetBool("isDashing", false);
					playerAnimator.SetFloat("speedY", 0.0f);
				}	
				
				this.enabled = false;
			}
		}
	}
}
