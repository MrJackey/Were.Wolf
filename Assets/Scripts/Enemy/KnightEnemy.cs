using System;
using System.Collections;
using UnityEngine;

public class KnightEnemy : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isWalkingHash = Animator.StringToHash("isWalking");
	private static readonly int turnHash = Animator.StringToHash("Turn");
	private static readonly int attackHash = Animator.StringToHash("Attack");
	private static readonly int rechargeHash = Animator.StringToHash("Recharge");
	private static readonly int finishRechargeHash = Animator.StringToHash("Finish Recharge");

	[SerializeField] private PlayerDetectionCone detectionCone;
	[SerializeField] private Collider2D wallCollider = null;
	[SerializeField] private Collider2D groundCollider = null;
	[SerializeField] private LayerMask groundLayer = 1 << 8;

	[Header("Movement")]
	[SerializeField] private bool doMovement = false;
	[SerializeField] private Transform leftPoint = null;
	[SerializeField] private Transform rightPoint = null;
	[SerializeField] private float movementSpeed = 2f;
	[SerializeField] private float animationSpeedMultiplier = 1f;
	[SerializeField] private float dashSpeed = 2f;
	[SerializeField] private float attackCooldown = 3f;

	[Header("Sounds")]
	[SerializeField] private SoundRandomizer attackSound;
	[SerializeField] private SoundRandomizer walkSound;

	private Animator animator;

	private State state;
	private float facing;
	private float turnDirection;
	private bool isDashing;
	private bool isBlocked;
	private SimpleTimer attackCooldownTimer;

	private void Start() {
		animator = GetComponent<Animator>();

		detectionCone.OnBecomeVisible.AddListener(OnPlayerBecomeVisible);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		UpdateIsBlocked();
	}

	private void OnTriggerExit2D(Collider2D other) {
		UpdateIsBlocked();
	}

	private void Update() {
		animator.SetFloat(speedHash, movementSpeed * animationSpeedMultiplier);
		animator.SetBool(isWalkingHash, state == State.Walking && doMovement);

		facing = Mathf.Sign(transform.lossyScale.x);
		UpdateMovement();

		if (state == State.Cooldown && attackCooldownTimer.Tick())
			animator.Play(finishRechargeHash);
	}

	private void OnPlayerBecomeVisible() {
		if (state != State.Attacking && state != State.Cooldown)
			Attack();
	}

	private void UpdateIsBlocked() {
		isBlocked = wallCollider.IsTouchingLayers(groundLayer) || !groundCollider.IsTouchingLayers(groundLayer);
	}

	private void UpdateMovement() {
		if (isDashing) {
			UpdateDashing();
			return;
		}

		if (state == State.Walking && doMovement) {
			Transform self = transform;
			Vector3 position = self.position;
			position.x += movementSpeed * facing * Time.deltaTime;
			self.position = position;

			if (position.x <= leftPoint.position.x) {
				if (facing < 0)
					Turn(1);
			}
			else if (position.x >= rightPoint.position.x) {
				if (facing >= 0)
					Turn(-1);
			}
		}
	}

	private void UpdateDashing() {
		if (isBlocked) return;

		Transform self = transform;
		Vector3 position = self.position;
		position.x += dashSpeed * facing * Time.deltaTime;
		self.position = position;
	}

	private void Turn(float direction) {
		if (facing == direction) return;

		animator.Play(turnHash);
		state = State.Turning;
		turnDirection = direction;
		detectionCone.enabled = false;
	}

	private void SetFacing(float direction) {
		Transform self = transform;
		Vector3 scale = self.localScale;
		scale.x = direction;
		self.localScale = scale;
		facing = direction;
	}

	private void Attack() {
		animator.Play(attackHash);
		attackSound.PlayRandom();
		state = State.Attacking;
		isDashing = true;
	}

	private void BeginCooldown() {
		state = State.Cooldown;
		animator.Play(rechargeHash);
		attackCooldownTimer.Reset(attackCooldown);
	}

	private void EndCooldown() {
		state = State.Walking;

		if (detectionCone.IsPlayerVisible)
			Attack();
	}

	private void EndTurning() {
		state = State.Walking;
		SetFacing(turnDirection);
		detectionCone.enabled = true;
	}


	#region Animation events

	private void OnAttackAnimationFinished() {
		Debug.Assert(state == State.Attacking);
		BeginCooldown();
	}

	private void OnTurnAnimationFinished() {
		Debug.Assert(state == State.Turning);
		EndTurning();
	}

	private void OnDashAnimationFinished() {
		Debug.Assert(isDashing);
		isDashing = false;
	}

	private void OnCooldownAnimationFinished() {
		Debug.Assert(state == State.Cooldown);
		EndCooldown();
	}

	public void PlayWalkingSound() {
		walkSound.PlayRandom();
	}

	#endregion


	private enum State {
		Walking,
		Turning,
		Attacking,
		Cooldown,
	}
}