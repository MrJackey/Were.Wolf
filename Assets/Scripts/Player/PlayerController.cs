using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	[Header("Constants")]
	[SerializeField] private float gravity = -9.82f;

	[Header("Movement")]
	[SerializeField] private float acceleration = 25;
	[SerializeField] private float deacceleration = 5;
	[SerializeField] private float maxSpeed = 3;

	[Header("Jumping")]
	[SerializeField] private AnimationCurve jumpCurve = null;

	private Rigidbody2D rb2D;
	private BoxCollider2D boxCollider;
	private LayerMask groundLayer;
	private Vector2 velocity;

	private bool doJump = false;
	private float jumpTimer = 0f;
	private float jumpEndTime;
	private bool isGrounded = false;

	public bool IsGrounded => isGrounded;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		groundLayer = LayerMask.GetMask("Ground");
		jumpEndTime = jumpCurve.keys[jumpCurve.length - 1].time;
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
		if (doJump) {
			Jump();
		}

		if (Mathf.Abs(rb2D.velocity.y) < 0.01f)
			doJump = false;

		if (!doJump)
			rb2D.velocity = new Vector2(velocity.x, rb2D.velocity.y + gravity * Time.deltaTime);
	}

	private bool CheckIfGrounded() {
		Vector3 position = transform.position;

		Vector2 rayStartPosition = new Vector2(position.x, position.y - boxCollider.bounds.extents.y);
		float rayDistance = 0.05f;

		RaycastHit2D hit = Physics2D.Raycast(rayStartPosition,Vector2.down, rayDistance, groundLayer);

		return hit.collider != null;
	}

	private void Jump() {
		jumpTimer += Time.deltaTime;
		float derivative =
			(jumpCurve.Evaluate(jumpTimer + Time.deltaTime) -
			 jumpCurve.Evaluate(jumpTimer - Time.deltaTime)) / (2 * Time.deltaTime);

		rb2D.velocity = new Vector2(velocity.x, derivative);

		if (jumpTimer >= jumpEndTime) {
			doJump = false;
			jumpTimer = 0f;
		}
	}
}
