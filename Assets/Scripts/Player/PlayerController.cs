using System;
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
	[SerializeField] private BoxCollider2D clearAboveCollider;
	[SerializeField] private float crouchTransitionTime = 1f;
	[SerializeField] private float crouchMaxSpeed = 1f;
	[SerializeField] private BoxCollider2D humanCollider;
	[SerializeField] private BoxCollider2D humanGroundCollider;
	[SerializeField] private BoxCollider2D crouchCollider;
	[SerializeField] private BoxCollider2D crouchGroundCollider;

	[Header("Sounds")]
	[SerializeField] private SoundRandomizer wolfJumpSound;
	[SerializeField] private SoundRandomizer humanJumpSound;
	[SerializeField] private AudioSource crawlingSound;

	[Header("Events")]
	[SerializeField] private UnityEvent onJump;
	[SerializeField] private UnityEvent onAirJump;
	[SerializeField] private UnityEvent onDash;
	[SerializeField] private UnityEvent onCrouch;
	[SerializeField] private UnityEvent onUncrouch;

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
	private float crouchTransitionTimer = float.PositiveInfinity;
	private bool isCrouched = false;
	private bool isClearAbove = false;

	private Vector2 moveInput;
	private bool jumpInputDown, jumpInputUp;
	private bool dashInputDown;
	private bool crouchInput;

	public bool AllowControls {
		get => allowControls;
		set => allowControls = value;
	}
	public bool IsGrounded => isGrounded;
	public bool IsCrouched {
		get => isCrouched;
		set => isCrouched = value;
	}
	public bool IsClearAbove => isClearAbove;
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
	public Vector2 Velocity { set => velocity = value; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	public bool NoClip {
		get => noClip;
		set {
			if (noClip == value) return;
			noClip = value;
			GetComponent<Health>().IsInvincible = noClip;
			colliders.SetActive(!noClip);
			rb2D.isKinematic = noClip;
			rb2D.velocity = Vector2.zero;
		}
	}
#endif

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
			float max = isCrouched ? crouchMaxSpeed : maxSpeed;

			velocity.x = Mathf.Clamp(newVelocityX, -max, max);
		}
		else if ((allowControls || isGrounded) && !doDash && velocity.x != 0) {
			velocity.x -= velocity.x * deacceleration * Time.deltaTime;
		}

		animator.SetFloat(speedHash, Mathf.Abs(xInput));

		if (isCrouching && crouchTransitionTimer <= crouchTransitionTime) {
			crouchTransitionTimer += Time.deltaTime;
			if (isCrouched)
				UpdateCrouchColliders(humanCollider, crouchCollider, humanGroundCollider, crouchGroundCollider);
			else
				UpdateCrouchColliders(crouchCollider, humanCollider, crouchGroundCollider, humanGroundCollider);
		}

		if (!allowControls)
			return;

		if (jumpInputUp && (doJump || doAirJump)) {
			InterruptJump();
			rb2D.velocity = new Vector2(velocity.x, velocity.y * jumpCancel);
		}

		if (jumpInputDown && !isCrouched && !isCrouching) {
			if (isGrounded) {
				BeginJump(onJump);
				doJump = true;

				if (transformation.IsHuman)
					humanJumpSound.PlayRandom();
				else
					wolfJumpSound.PlayRandom();
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

		if (transformation.IsHuman) {
			if (crouchInput && !isCrouched && isGrounded)
				StartCrouch();
			else if (isCrouched && !crouchInput && isClearAbove)
				Uncrouch();
		}


		if (isCrouched) {
			if (Mathf.Abs(xInput) > 0.1f) {
				if (!crawlingSound.isPlaying)
					crawlingSound.Play();
			}
			else {
				crawlingSound.Stop();
			}
		}
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
			if (!isGrounded)
				allowDashReset = true;

			EndCoyote(true);
		}

		isClearAbove = !clearAboveCollider.IsTouchingLayers(groundLayer);
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (!groundedCollider.IsTouchingLayers(groundLayer)) {
			if (coyoteRoutine != null)
				StopCoroutine(coyoteRoutine);

			coyoteRoutine = StartCoroutine(CoCoyoteDuration());

			if (isCrouched)
				Uncrouch();
		}

		isClearAbove = !clearAboveCollider.IsTouchingLayers(groundLayer);
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

	public void OnCrouchInput(InputAction.CallbackContext ctx) {
		if (ctx.phase == InputActionPhase.Performed && ctx.ReadValueAsButton())
			crouchInput = true;
		else if (ctx.phase == InputActionPhase.Canceled)
			crouchInput = false;
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
		isGrounded = groundedCollider.IsTouchingLayers(groundLayer);
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

	public void InterruptDash() {
		allowControls = true;
		doGravity = true;
		dashTimer = 0;
		doDash = false;
		animator.SetBool(isDashingHash, false);
	}

	private void StartCrouch() {
		if (isCrouching) return;

		isCrouching = true;
		isCrouched = true;
		crouchTransitionTimer = 0f;
		onCrouch.Invoke();
	}

	private void Uncrouch() {
		if (!isCrouched) return;

		isCrouching = true;
		isCrouched = false;
		crouchTransitionTimer = 0f;
		crawlingSound.Stop();
		onUncrouch.Invoke();
	}

	private void UpdateCrouchColliders(BoxCollider2D hitFrom, BoxCollider2D hitTo, BoxCollider2D groundFrom, BoxCollider2D groundTo) {
		if (!isCrouching) return;
		float factor = crouchTransitionTimer / crouchTransitionTime;

		hitCollider.size = Vector2.Lerp(hitFrom.size, hitTo.size, factor);
		hitCollider.offset = Vector2.Lerp(hitFrom.offset, hitTo.offset, factor);
		groundedCollider.size = Vector2.Lerp(groundFrom.size, groundTo.size, factor);
		groundedCollider.offset = Vector2.Lerp(groundFrom.offset, groundTo.offset, factor);

		if (factor >= 1)
			isCrouching = false;
	}

	public void InterruptCrouch() {
		if (!isCrouched) return;

		isCrouching = false;
		isCrouched = false;
		crouchTransitionTimer = float.PositiveInfinity;
		onUncrouch.Invoke();
	}
}
