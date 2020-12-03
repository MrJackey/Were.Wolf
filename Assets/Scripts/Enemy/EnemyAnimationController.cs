using System;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isAttackingHash = Animator.StringToHash("isAttacking");

	[SerializeField] private float baseSpeed = 0.6f;

	private Animator animator;
	private Mover mover;

	public bool IsAttacking { get; set; }

	private void Start() {
		animator = GetComponent<Animator>();
		mover = GetComponent<Mover>();
	}

	private void Update() {
		if (mover != null) {
			if (mover.enabled)
				animator.SetFloat(speedHash, mover.Speed / baseSpeed);
			else
				animator.SetFloat(speedHash, 0f);
		}

		animator.SetBool(isAttackingHash, IsAttacking);
	}
}