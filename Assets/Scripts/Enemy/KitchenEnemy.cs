﻿using UnityEngine;

public class KitchenEnemy : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int turnHash = Animator.StringToHash("Turn");
	private static readonly int attackUpHash = Animator.StringToHash("Attack Up");
	private static readonly int attackLeftHash = Animator.StringToHash("Attack Left");
	private static readonly int isWalkingHash = Animator.StringToHash("isWalking");

	[Header("References")]
	[SerializeField] private Transform leftAttackPrefab = null;
	[SerializeField] private Transform upAttackPrefab = null;
	[SerializeField] private Transform leftAttackPoint = null;
	[SerializeField] private Transform upAttackPoint = null;

	[Header("Movement")]
	[SerializeField] private Transform leftPoint = null;
	[SerializeField] private Transform rightPoint = null;
	[SerializeField] private float movementSpeed = 2f;
	[SerializeField] private float animationSpeedMultiplier = 1f;

	[Header("Attack")]
	[SerializeField] private AttackAction leftAttack = AttackAction.None;
	[SerializeField] private AttackAction rightAttack = AttackAction.None;

	private Animator animator;
	private State state = State.Walking;
	private float facing = -1;
	private float turnDirection;
	private bool turnAfterAttack;
	private Direction attackDirection;

	private void Start() {
		animator = GetComponent<Animator>();
		SetFacing(Mathf.Sign(transform.localScale.x));
	}

	private void Update() {
		UpdateMovement();
	}

	private void UpdateMovement() {
		animator.SetFloat(speedHash, movementSpeed * animationSpeedMultiplier);

		if (state != State.Walking) {
			animator.SetBool(isWalkingHash, false);
			return;
		}

		animator.SetBool(isWalkingHash, true);

		Transform self = transform;
		Vector3 position = self.position;
		position.x += movementSpeed * facing * Time.deltaTime;
		self.position = position;

		if (position.x <= leftPoint.position.x) {
			if (facing < 0)
				OnReachPoint(Direction.Left);
		}
		else if (position.x >= rightPoint.position.x) {
			if (facing >= 0)
				OnReachPoint(Direction.Right);
		}
	}

	private void OnReachPoint(Direction direction) {
		AttackAction action = direction == Direction.Left ? leftAttack : rightAttack;
		if (action == AttackAction.None) {
			Turn(-facing);
		}
		else {
			turnAfterAttack = true;
			Attack((Direction)action);
		}
	}

	private void Turn(float direction) {
		if (facing == direction) return;

		animator.Play(turnHash);
		state = State.Turning;
		turnDirection = direction;
	}

	private void SetFacing(float direction) {
		Transform self = transform;
		Vector3 scale = self.localScale;
		scale.x = facing;
		self.localScale = scale;
		facing = direction;
	}

	private void Attack(Direction direction) {
		animator.Play(direction == Direction.Up ? attackUpHash : attackLeftHash);

		attackDirection = direction;
		state = State.Attacking;
	}

	#region Animation events

	private void OnAttackStart() {
		Debug.Assert(state == State.Attacking);

		if (attackDirection == Direction.Up) {
			Instantiate(upAttackPrefab, upAttackPoint);
		}
		else {
			float attackFacing = attackDirection == Direction.Left ? -1 : 1;
			if (facing != attackFacing) {
				SetFacing(attackFacing);
				turnAfterAttack = false;
			}

			Instantiate(leftAttackPrefab, leftAttackPoint);
		}
	}

	private void OnAttackFinished() {
		Debug.Assert(state == State.Attacking);

		state = State.Walking;
		if (turnAfterAttack)
			Turn(-facing);
	}

	private void OnTurnFinished() {
		Debug.Assert(state == State.Turning);

		state = State.Walking;
		SetFacing(turnDirection);
	}

	#endregion


	private enum State {
		Walking,
		Turning,
		Attacking,
	}

	public enum Direction {
		Up = 1,
		Left = 2,
		Right = 3,
	}

	public enum AttackAction {
		None,
		Up = 1,
		Left = 2,
		Right = 3,
	}
}