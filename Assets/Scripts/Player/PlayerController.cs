using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isDashingHash = Animator.StringToHash("isDashing");

	[Header("Constants")]
	[SerializeField] private float gravity = -9.82f;
	[SerializeField] private Animator animator = null;

	[Header("Movement")]
	[SerializeField] private float acceleration = 25;
	[SerializeField] private float deacceleration = 5;
	[SerializeField] private float maxSpeed = 3;

	[Header("Jumping")]
	[SerializeField] private Collider2D groundedCollider = null;
	[SerializeField] private AnimationCurve jumpCurve = null;
	[SerializeField] private AnimationCurve humanJumpCurve;
	[SerializeField] private int airJumpsAllowed = 1;
	[SerializeField] private bool useSameCurve;
	[SerializeField, EnableIf(nameof(useSameCurve), Not = true)]
	private AnimationCurve airJumpCurve = null;
	[SerializeField] private float coyoteDuration = 0.15f;

	[Header("Dash")]
	[SerializeField] private float dashSpeed = 10;
	[SerializeField] private float dashDuration = 10f;
	[SerializeField] private float dashCooldown = 0.5f;

	[Header("Events")]
	[SerializeField] private UnityEvent onJump;
	[SerializeField] private UnityEvent onAirJump;
	[SerializeField] private UnityEvent onDash;

	private Rigidbody2D rb2D;
	private LayerMask groundLayer;
	private Vector2 velocity;
	private int facing = 1;

	private bool allowControls = true;
	private bool humanControls = false;
	private bool doGravity = true;
	private bool isGrounded = false;

	private bool doJump = false;
	private float jumpEndTime;
	private float humanJumpEndTime;
	private bool doAirJump = false;
	private int airJumpsUsed = 0;
	private float airJumpEndTime;
	private float jumpTimer = 0f;
	private Coroutine coyoteRoutine = null;

	private bool dashHeld = false;
	private bool doDash = false;
	private bool allowDash = true;
	private bool allowDashReset = false;
	private float dashTimer = 0f;
	private float dashResetTimer = 0.5f;

	public bool HumanControls {
		get => humanControls;
		set => humanControls = value;
	}
	public bool AllowControls { set => allowControls = value; }
	public bool IsGrounded => isGrounded;
	public float JumpLength => jumpEndTime;
	public float HumanJumpLength => humanJumpEndTime;
	public float AirJumpLength => airJumpEndTime;
	

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		groundLayer = LayerMask.GetMask("Ground");
		jumpEndTime = jumpCurve.keys[jumpCurve.length - 1].time;
		humanJumpEndTime = humanJumpCurve.keys[humanJumpCurve.length - 1].time;

		if (useSameCurve)
			airJumpCurve = jumpCurve;
		airJumpEndTime = airJumpCurve.keys[airJumpCurve.length - 1].time;
	}

	private void Update() {
		if (isGrounded) {
			airJumpsUsed = 0;
			if (allowDashReset) {
				allowDash = true;
				allowDashReset = false;
			}
		}

		float xInput = allowControls ? Input.GetAxisRaw("Horizontal") : 0;
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

		if (xInput != 0) {
			float newVelocityX = velocity.x + xInput * acceleration * Time.deltaTime;
			velocity.x = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
		}
		else if ((allowControls || (isGrounded && !doDash)) && velocity.x != 0) {
			velocity.x -= velocity.x * deacceleration * Time.deltaTime;
		}

		animator.SetFloat(speedHash, Mathf.Abs(xInput));

		if (!allowControls)
			return;

		if (Input.GetButtonDown("Jump")) {
			if (isGrounded) {
				BeginJump(onJump);
				doJump = true;
			}
			else if (!humanControls && airJumpsUsed < airJumpsAllowed) {
				BeginJump(onAirJump);
				doJump = false;
				airJumpsUsed++;
				doAirJump = true;
			}
		}

		if (!humanControls && CheckDashInput() && allowDash)
			BeginDash(facing);
	}

	private void FixedUpdate() {
		if (doJump) {
			AnimationCurve curve = humanControls ? humanJumpCurve : jumpCurve;
			float endTime = humanControls ? humanJumpEndTime : jumpEndTime;
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

		rb2D.velocity = velocity;
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

		rb2D.velocity = new Vector2(velocity.x, derivative);

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

	private bool CheckDashInput() {
		if (Mathf.Abs(Input.GetAxisRaw("Dash")) > 0) {
			bool oldState = dashHeld;

			dashHeld = true;
			return oldState == false;
		}

		dashHeld = false;
		return false;
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
}
