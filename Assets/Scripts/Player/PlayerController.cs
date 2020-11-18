using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	[Header("Movement")]
	[SerializeField] private float acceleration = 25;
	[SerializeField] private float deacceleration = 5;
	[SerializeField] private float maxSpeed = 3;

	[Header("Jumping")]
	[SerializeField] private float jumpVelocity = 7.5f;

	private Rigidbody2D rb2D;
	private BoxCollider2D boxCollider;
	private LayerMask groundLayer;
	private Vector2 velocity;

	private bool doJump = false;
	private bool isGrounded = false;

	public bool IsGrounded => isGrounded;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		groundLayer = LayerMask.GetMask("Ground");
	}

	private void Update() {
		isGrounded = CheckIfGrounded();
		float xInput = Input.GetAxisRaw("Horizontal");

		if (rb2D.velocity.x == 0)
			velocity.x = 0;

		if (xInput != 0) {
			float newVelocityX = velocity.x + xInput * acceleration * Time.deltaTime;
			velocity.x = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
		} else {
			velocity.x -= velocity.x * deacceleration * Time.deltaTime;
		}

		if (Input.GetButtonDown("Jump") && isGrounded)
			doJump = true;
	}

	private void FixedUpdate() {
		rb2D.velocity = new Vector2(velocity.x, rb2D.velocity.y);

		if (doJump) {
			rb2D.velocity = new Vector2(velocity.x, jumpVelocity);
			doJump = false;
		}
	}

	private bool CheckIfGrounded() {
		Vector3 position = transform.position;

		Vector2 rayStartPosition = new Vector2(position.x, position.y - boxCollider.bounds.extents.y);
		float rayDistance = 0.05f;

		RaycastHit2D hit = Physics2D.Raycast(rayStartPosition,Vector2.down, rayDistance, groundLayer);

		return hit.collider != null;
	}
}
