using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int transformStartHash = Animator.StringToHash("transformStart");
	private static readonly int transformInterruptHash = Animator.StringToHash("transformInterrupt");
	private static readonly int transformEndHash = Animator.StringToHash("transformEnd");
	private static readonly int jumpHash = Animator.StringToHash("jump");

	private Animator animator;
	private Rigidbody2D rb2D;

	private void Start() {
		animator = GetComponent<Animator>();
		rb2D = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		animator.SetFloat(speedHash, Mathf.Abs(rb2D.velocity.x));
	}

	public void JumpStart() {
		animator.SetTrigger(jumpHash);
	}

	public void TransformStart() {
		animator.SetTrigger(transformStartHash);
	}

	public void TransformInterrupt() {
		animator.SetTrigger(transformInterruptHash);
	}

	public void TransformEnd() {
		animator.SetTrigger(transformEndHash);
	}
}
