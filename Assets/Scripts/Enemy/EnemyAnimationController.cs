using System;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");

	[SerializeField] private float baseSpeed = 0.6f;

	private Animator animator;
	private Patrolling patrolling;

	private void Start() {
		animator = GetComponent<Animator>();
		patrolling = GetComponent<Patrolling>();
	}

	private void Update() {
		if (patrolling != null && patrolling.enabled)
			animator.SetFloat(speedHash, Mathf.Abs(patrolling.Velocity.x) / baseSpeed);
	}
}