﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isDashingHash = Animator.StringToHash("isDashing");

	[Header("Constants")]
	[SerializeField] private float gravity = -9.82f;
	[SerializeField] private Animator animator = null;
	[SerializeField] private BoxCollider2D hitCollider;

	[Header("Movement")]
	[SerializeField] private float acceleration = 25;
	[SerializeField] private float deacceleration = 5;
	[SerializeField] private float maxSpeed = 3;
	[SerializeField] private float speedMultiplier = 1;

	[Header("Jumping")]
	[SerializeField] private BoxCollider2D groundedCollider = null;
	[SerializeField] private AnimationCurve jumpCurve = null;
	[SerializeField] private AnimationCurve humanJumpCurve;
	[SerializeField] private int airJumpsAllowed = 1;
	[SerializeField] private bool useSameCurve;
	[SerializeField, EnableIf(nameof(useSameCurve), Invert = true)]
	private AnimationCurve airJumpCurve = null;
	[SerializeField] private float coyoteDuration = 0.15f;
	[SerializeField, Range(0, 1)] private float jumpCancel = 0.5f;

	[Header("Dash")]
	[SerializeField] private float dashSpeed = 10;
	[SerializeField] private float dashDuration = 10f;
	[SerializeField] private float dashCooldown = 0.5f;

	[Header("Crouch")]
	[SerializeField, Range(-1f, -0.1f)]
	private float crouchInputThreshold = -0.5f;
	[SerializeField] private float crouchDownTime = 1f;
	[SerializeField] private float crouchMaxSpeed = 1f;
	[SerializeField] private BoxCollider2D humanCollider;
	[SerializeField] private BoxCollider2D humanGroundCollider;
	[SerializeField] private BoxCollider2D crouchCollider;
	[SerializeField] private BoxCollider2D crouchGroundCollider;

	[Header("Events")]
	[SerializeField] private UnityEvent onJump;
	[SerializeField] private UnityEvent onAirJump;
	[SerializeField] private UnityEvent onDash;
	[SerializeField] private UnityEvent onCrouch;
	[SerializeField] private UnityEvent onCrouchEnd;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[Header("Debug")]
	[SerializeField] private bool noClip;
	[SerializeField] private float noClipSpeed = 5f;
	[SerializeField] private GameObject colliders;
#endif

	private Rigidbody2D rb2D;
	private Knockbackable knockbackable;
	private Transformation transformation;
	private LayerMask groundLayer;
	private Vector2 velocity;
	private int facing = 1;

	private bool allowControls = true;
	private bool doGravity = true;
	private bool isGrounded = false;
	private bool doKnockBack = false;

	private bool doJump = false;
	private float jumpEndTime;
	private float humanJumpEndTime;
	private bool doAirJump = false;
	private int airJumpsUsed = 0;
	private float airJumpEndTime;
	private float jumpTimer = 0f;
	private Coroutine coyoteRoutine = null;

	private bool doDash = false;
	private bool allowDash = true;
	private bool allowDashReset = false;
	private float dashTimer = 0f;
	private float dashResetTimer = 0.5f;

	private bool isCrouching = false;
	private float crouchingTime = float.PositiveInfinity;
	private bool isCrouched = false;

	private Vector2 moveInput;
	private bool jumpInputDown, jumpInputUp;
	private bool dashInputDown;

	public bool AllowControls {
		get => allowControls;
		set => allowControls = value;
	}
	public bool IsGrounded => isGrounded;
	public bool IsCrouched {
		get => isCrouched;
		set => isCrouched = value;
	}

	public bool DoKnockBack {
		set {
			doKnockBack = value;

			if (doKnockBack)
				InterruptJump();
		}
	}
	public float JumpLength => jumpEndTime;
	public float HumanJumpLength => humanJumpEndTime;
	public float AirJumpLength => airJumpEndTime;
	public float SpeedMultiplier { set => speedMultiplier = value; }

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		knockbackable = GetComponent<Knockbackable>();
		transformation = GetComponent<Transformation>();
		groundLayer = LayerMask.GetMask("Ground");
		jumpEndTime = jumpCurve.keys[jumpCurve.length - 1].time;
		humanJumpEndTime = humanJumpCurve.keys[humanJumpCurve.length - 1].time;

		if (useSameCurve)
			airJumpCurve = jumpCurve;
		airJumpEndTime = airJumpCurve.keys[airJumpCurve.length - 1].time;
	}

	private void Update() {
		UpdateMovement();

		jumpInputDown = jumpInputUp = false;
		dashInputDown = false;
	}

	private void UpdateMovement() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (Keyboard.current.f1Key.wasPressedThisFrame) {
			noClip = !noClip;
			GetComponent<Health>().IsInvincible = noClip;
			colliders.SetActive(!noClip);
			rb2D.isKinematic = noClip;
			rb2D.velocity = Vector2.zero;
		}

		if (noClip) {
			float x = moveInput.x;
			float y = moveInput.y;

			transform.Translate(x * noClipSpeed * Time.deltaTime, y * noClipSpeed * Time.deltaTime, 0);
			return;
		}
#endif

		if (isGrounded) {
			airJumpsUsed = 0;
			if (allowDashReset) {
				allowDash = true;
				allowDashReset = false;
			}
		}

		float xInput = allowControls ? moveInput.x : 0;
		if (xInput > 0)
			facing = 1;
		else if (xInput < 0)
			facing = -1;
		transform.localScale = new Vector3(facing, 1, 1);

		if (Mathf.Abs(rb2D.velocity.x) < 0.01f) {
			if (dashTimer > Time.deltaTime) {
				velocity.x = 0;
				InterruptDash();
			}
		}

		// Cancels jumps if head hits roof
		if (Mathf.Abs(rb2D.velocity.y) < 0.01f && !isGrounded && !doDash)
			InterruptJump();

		if (xInput != 0 && !isCrouching) {
			float newVelocityX = velocity.x + xInput * acceleration * Time.deltaTime;
			if (isCrouched)
				velocity.x = Mathf.Clamp(newVelocityX, -crouchMaxSpeed, crouchMaxSpeed);
			else
				velocity.x = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
		}
		else if ((allowControls || transformation.IsTransforming) && !doDash && velocity.x != 0) {
			velocity.x -= velocity.x * deacceleration * Time.deltaTime;
		}

		animator.SetFloat(speedHash, Mathf.Abs(xInput));

		if (isCrouching && crouchingTime <= crouchDownTime) {
			crouchingTime += Time.deltaTime;
			UpdateCrouchColliders();
		}

		if (!allowControls)
			return;

		if (jumpInputUp && (doJump || doAirJump)) {
			InterruptJump();
			rb2D.velocity = new Vector2(velocity.x, velocity.y * jumpCancel);
		}

		if (jumpInputDown) {
			if (isGrounded) {
				BeginJump(onJump);
				doJump = true;
			}
			else if (!transformation.IsHuman && airJumpsUsed < airJumpsAllowed) {
				BeginJump(onAirJump);
				doJump = false;
				airJumpsUsed++;
				doAirJump = true;
			}
		}

		if (!transformation.IsHuman && dashInputDown && allowDash)
			BeginDash(facing);

		if (!isGrounded) return;

		if (transformation.IsHuman && moveInput.y < crouchInputThreshold && !isCrouched)
			StartCrouch();
		else if (transformation.IsHuman && moveInput.y > crouchInputThreshold && isCrouched && CheckUncrouch())
			StopCrouch();
	}

	private void FixedUpdate() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (noClip)
			return;
#endif

		if (doJump) {
			AnimationCurve curve = transformation.IsHuman ? humanJumpCurve : jumpCurve;
			float endTime = transformation.IsHuman ? humanJumpEndTime : jumpEndTime;
			Jump(curve, endTime);
		}
		else if (doAirJump)
			Jump(airJumpCurve, airJumpEndTime);

		if (doDash) {
			Dash();
		}
		else if (!allowDashReset) {
			dashResetTimer += Time.deltaTime;

			if (dashResetTimer >= dashCooldown) {
				allowDashReset = true;
				dashResetTimer = 0f;
			}
		}

		if (doGravity)
			velocity.y = rb2D.velocity.y + gravity * Time.deltaTime;

		if (doKnockBack)
			velocity = knockbackable.Velocity;

		rb2D.velocity = velocity * new Vector2(speedMultiplier, doGravity ? 1 : speedMultiplier);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (groundedCollider.IsTouchingLayers(groundLayer)) {
			allowDashReset = true;
			EndCoyote(true);
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (!groundedCollider.IsTouchingLayers(groundLayer)) {
			if (coyoteRoutine != null)
				StopCoroutine(coyoteRoutine);

			coyoteRoutine = StartCoroutine(CoCoyoteDuration());
		}
	}

	public void OnMove(InputAction.CallbackContext ctx) {
		moveInput = ctx.ReadValue<Vector2>();
		// I had trouble setting up two separate axes for the move input so I'm doing a per axis deadzone check here to
		// work around the fact that a 2D composite input shares its deadzone between both axes.
		const float deadzone = 0.125f;
		if (Mathf.Abs(moveInput.x) < deadzone) moveInput.x = 0;
		if (Mathf.Abs(moveInput.y) < deadzone) moveInput.y = 0;
	}

	public void OnJump(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started)
			jumpInputDown = ctx.started;
		else if (ctx.phase == InputActionPhase.Canceled)
			jumpInputUp = ctx.canceled;
	}

	public void OnDash(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Started)
			dashInputDown = ctx.started;
	}

	private IEnumerator CoCoyoteDuration() {
		yield return new WaitForSeconds(coyoteDuration);

		EndCoyote(false);
	}

	private void EndCoyote(bool newGrounded) {
		if (coyoteRoutine != null)
			StopCoroutine(coyoteRoutine);

		coyoteRoutine = null;
		isGrounded = newGrounded;
	}

	private void BeginJump(UnityEvent e) {
		jumpTimer = 0f;
		e.Invoke();
	}

	private void Jump(AnimationCurve curve, float endTime) {
		isGrounded = false;
		jumpTimer += Time.deltaTime;
		float derivative =
			(curve.Evaluate(jumpTimer + Time.deltaTime) -
			 curve.Evaluate(jumpTimer - Time.deltaTime)) / (2 * Time.deltaTime);

		rb2D.velocity = new Vector2(velocity.x, derivative) * speedMultiplier;

		if (jumpTimer >= endTime) {
			doJump = false;
			doAirJump = false;
		}
	}

	private void InterruptJump() {
		doJump = false;
		doAirJump = false;
		doGravity = true;
	}

	private void BeginDash(int direction) {
		InterruptJump();
		doGravity = false;
		doDash = true;
		dashTimer = 0;
		allowControls = false;
		allowDash = false;
		allowDashReset = false;

		velocity.y = 0;
		velocity.x = dashSpeed * direction;

		animator.SetBool(isDashingHash, true);
		onDash.Invoke();
	}

	private void Dash() {
		if (dashTimer >= dashDuration)
			InterruptDash();

		dashTimer += Time.deltaTime;
	}

	private void InterruptDash() {
		allowControls = true;
		doGravity = true;
		dashTimer = 0;
		doDash = false;
		animator.SetBool(isDashingHash, false);
	}

	private void StartCrouch() {
		isCrouching = true;
		isCrouched = true;
		crouchingTime = 0f;
		velocity.x = 0;
		onCrouch.Invoke();
	}

	private void StopCrouch() {
		isCrouching = true;
		isCrouched = false;
		crouchingTime = 0f;
		velocity.x = 0;
		onCrouchEnd.Invoke();
	}

	private void UpdateCrouchColliders() {
		if (!isCrouching) return;

		float factor = crouchingTime / crouchDownTime;

		if (isCrouched) {
			hitCollider.size = Vector2.Lerp(humanCollider.size, crouchCollider.size, factor);
			hitCollider.offset = Vector2.Lerp(humanCollider.offset, crouchCollider.offset, factor);
			groundedCollider.size = Vector2.Lerp(humanGroundCollider.size, crouchGroundCollider.size, factor);
			groundedCollider.offset = Vector2.Lerp(humanGroundCollider.offset, crouchGroundCollider.offset, factor);
		}
		else {
			hitCollider.size = Vector2.Lerp(crouchCollider.size, humanCollider.size, factor);
			hitCollider.offset = Vector2.Lerp(crouchCollider.offset, humanCollider.offset, factor);
			groundedCollider.size = Vector2.Lerp(crouchGroundCollider.size, humanGroundCollider.size, factor);
			groundedCollider.offset = Vector2.Lerp(crouchGroundCollider.offset, humanGroundCollider.offset, factor);
		}

		if (factor >= 1)
			isCrouching = false;
	}

	public bool CheckUncrouch() {
		Vector2 origin = transform.position;
		Vector2 humanSize = humanCollider.size;

		RaycastHit2D upCheck = Physics2D.Raycast(origin, Vector2.up, humanSize.y, groundLayer);

		return (upCheck.distance == 0 || !isGrounded) &&
		       (upCheck.distance != 0 || isGrounded);
	}
}
