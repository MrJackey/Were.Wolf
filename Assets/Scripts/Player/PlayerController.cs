﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
	[Header("Constants")]
	[SerializeField] private float gravity = -9.82f;

	[Header("Movement")]
	[SerializeField] private float acceleration = 25;
	[SerializeField] private float deacceleration = 5;
	[SerializeField] private float maxSpeed = 3;

	[Header("Jumping")]
	[SerializeField] private float groundedDistance = 0.05f;
	[SerializeField] private AnimationCurve jumpCurve = null;
	[SerializeField] private int airJumpsAllowed = 1;
	[SerializeField] private bool useSameCurve;
	[SerializeField, EnableIf(nameof(useSameCurve), Not = true)]
	private AnimationCurve airJumpCurve = null;

	[Header("Events")]
	[SerializeField] private UnityEvent onJump;
	[SerializeField] private UnityEvent onAirJump;

	private Rigidbody2D rb2D;
	private BoxCollider2D boxCollider;
	private LayerMask groundLayer;
	private Vector2 velocity;

	private bool allowControls = true;
	private bool doJump = false;
	private float jumpEndTime;
	private bool doAirJump = false;
	private int airJumpsUsed = 0;
	private float airJumpEndTime;
	private float jumpTimer = 0f;
	private bool isGrounded = false;

	public bool AllowControls { set => allowControls = value; }
	public bool IsGrounded => isGrounded;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		groundLayer = LayerMask.GetMask("Ground");
		jumpEndTime = jumpCurve.keys[jumpCurve.length - 1].time;

		if (useSameCurve)
			airJumpCurve = jumpCurve;
		airJumpEndTime = airJumpCurve.keys[airJumpCurve.length - 1].time;
	}

	private void Update() {
		isGrounded = CheckIfGrounded();
		if (isGrounded)
			airJumpsUsed = 0;

		float xInput = allowControls ? Input.GetAxisRaw("Horizontal") : 0;

		if (rb2D.velocity.x == 0)
			velocity.x = 0;

		if (xInput != 0) {
			float newVelocityX = velocity.x + xInput * acceleration * Time.deltaTime;
			velocity.x = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
		} else if (allowControls || isGrounded) {
			velocity.x -= velocity.x * deacceleration * Time.deltaTime;
		}

		if (Input.GetButtonDown("Jump") && allowControls) {
			if (isGrounded) {
				BeginJump(onJump);
				doJump = true;
			}
			else if (airJumpsUsed < airJumpsAllowed) {
				BeginJump(onAirJump);
				doJump = false;
				airJumpsUsed++;
				doAirJump = true;
			}
		}
	}

	private void FixedUpdate() {
		if (doJump)
			Jump(jumpCurve, jumpEndTime);
		else if (doAirJump)
			Jump(airJumpCurve, airJumpEndTime);

		if (Mathf.Abs(rb2D.velocity.y) < 0.01f)
			doJump = false;

		if (!doJump || !doAirJump)
			rb2D.velocity = new Vector2(velocity.x, rb2D.velocity.y + gravity * Time.deltaTime);
	}

	private bool CheckIfGrounded() {
		Vector3 position = transform.position;

		Vector2 rayStartPosition = new Vector2(position.x, position.y - boxCollider.bounds.extents.y);

		RaycastHit2D hit = Physics2D.Raycast(rayStartPosition,Vector2.down, groundedDistance, groundLayer);

		return hit.collider != null;
	}

	private void BeginJump(UnityEvent e) {
		jumpTimer = 0f;
		e.Invoke();
	}

	private void Jump(AnimationCurve curve, float endTime) {
		jumpTimer += Time.deltaTime;
		float derivative =
			(curve.Evaluate(jumpTimer + Time.deltaTime) -
			 curve.Evaluate(jumpTimer - Time.deltaTime)) / (2 * Time.deltaTime);

		rb2D.velocity = new Vector2(velocity.x, derivative);

		if (jumpTimer >= endTime) {
			doJump = false;
			doAirJump = false;
		}
	}
}
