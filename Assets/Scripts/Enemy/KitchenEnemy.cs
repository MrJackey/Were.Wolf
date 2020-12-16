using System;
using UnityEngine;

public class KitchenEnemy : MonoBehaviour {
	// TODO: Optimize animator.Play hashes.

	[SerializeField] private Transform leftAttackPrefab;
	[SerializeField] private Transform upAttackPrefab;

	[SerializeField] private Transform leftAttackPoint;
	[SerializeField] private Transform upAttackPoint;


	[SerializeField] private Transform point1 = null;
	[SerializeField] private Transform point2 = null;

	[SerializeField] private float movementSpeed;


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
		if (state == State.Walking)
			UpdateMovement();
	}

	private void UpdateMovement() {
		Vector3 position = transform.position;
		position.x += movementSpeed * facing * Time.deltaTime;
		transform.position = position;

		if (position.x <= point1.position.x) {
			if (facing < 0) {
				turnAfterAttack = true;
				Attack(Direction.Left);
			}
		}
		else if (position.x >= point2.position.x) {
			if (facing >= 0) {
				turnAfterAttack = true;
				Attack(Direction.Right);
			}
		}
	}

	private void Turn(float direction) {
		if (facing == direction) return;

		animator.Play("Turn");
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
		if (direction == Direction.Up) {
			animator.Play("Attack Up");
		}
		else {
			animator.Play("Attack Left");
		}

		attackDirection = direction;
		state = State.Attacking;
	}


	// Animation events
	private void OnTurnFinished() {
		print("turn finished");
		Debug.Assert(state == State.Turning);

		state = State.Walking;
		SetFacing(turnDirection);
	}

	private void OnAttackStart() {
		print("attack start");
		Debug.Assert(state == State.Attacking);

		if (attackDirection == Direction.Up) {
			Instantiate(upAttackPrefab, upAttackPoint);
		}
		else {
			float newFacing = attackDirection == Direction.Left ? -1 : 1;
			if (facing != newFacing)
				SetFacing(newFacing);

			Instantiate(leftAttackPrefab, leftAttackPoint);
		}
	}

	private void OnAttackFinished() {
		print("attack finished");
		Debug.Assert(state == State.Attacking);

		state = State.Walking;
		if (turnAfterAttack)
			Turn(-facing);
	}


	public enum Direction {
		Up,
		Left,
		Right,
	}

	private enum State {
		Walking,
		Turning,
		Attacking,
	}
}