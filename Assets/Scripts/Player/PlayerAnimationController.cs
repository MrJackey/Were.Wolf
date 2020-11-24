using System;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
	private static readonly int jumpHash = Animator.StringToHash("jump");
	private static readonly int speedYHash = Animator.StringToHash("speedY");
	private static readonly int isHumanHash = Animator.StringToHash("isHuman");
	private static readonly int transformSpeedHash = Animator.StringToHash("transformSpeed");
	private static readonly int jumpSpeedHash = Animator.StringToHash("jumpSpeed");

	[SerializeField] private AnimationClip transformAnimation;
	[SerializeField] private AnimationClip jumpAnimation;

	private Animator animator;
	private Rigidbody2D rb2D;
	private PlayerController playerController;
	private Transformation transformation;

	private void Awake() {
		transformation = GetComponent<Transformation>();
	}

	private void Start() {
		animator = GetComponent<Animator>();
		rb2D = GetComponent<Rigidbody2D>();
		playerController = GetComponent<PlayerController>();
	}

	private void Update() {
		animator.SetFloat(speedYHash, Mathf.Abs(rb2D.velocity.y));
	}

	private void OnEnable() {
		transformation.OnTransformInterrupt.AddListener(TransformInterrupt);
	}

	private void OnDisable() {
		transformation.OnTransformInterrupt.RemoveListener(TransformInterrupt);
	}

	public void JumpStart() {
		animator.SetFloat(jumpSpeedHash, 1f / (playerController.JumpLength / jumpAnimation.length));
		animator.SetTrigger(jumpHash);
	}

	public void AirJumpStart() {
		animator.SetFloat(jumpSpeedHash, 1f / (playerController.AirJumpLength / jumpAnimation.length));
		animator.SetTrigger(jumpHash);
	}

	public void TransformStart() {
		animator.SetFloat(transformSpeedHash, 1f / (transformation.TransformDuration / transformAnimation.length));
		animator.SetBool(isHumanHash, transformation.OldState != TransformationState.Human);
	}

	public void TransformInterrupt(float transformationDone) {
		float timeRemaining = transformation.TransformDuration - transformationDone;
		animator.Play("Human To Werewolf", 0, timeRemaining / transformation.TransformDuration);

		transformation.State = TransformationState.Human;
		transformation.TransformToWolf(timeRemaining);
	}
}
