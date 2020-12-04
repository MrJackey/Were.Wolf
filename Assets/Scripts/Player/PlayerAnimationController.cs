using System;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
	private static readonly int jumpHash = Animator.StringToHash("jump");
	private static readonly int speedYHash = Animator.StringToHash("speedY");
	private static readonly int isHumanHash = Animator.StringToHash("isHuman");
	private static readonly int transformSpeedHash = Animator.StringToHash("transformSpeed");
	private static readonly int jumpSpeedHash = Animator.StringToHash("jumpSpeed");
	private static readonly int doPickUpHash = Animator.StringToHash("doPickUp");
	private static readonly int doDropHash = Animator.StringToHash("doDrop");
	private static readonly int isTransformingHash = Animator.StringToHash("isTransforming");

	[SerializeField] private AnimationClip transformAnimation;
	[SerializeField] private AnimationClip jumpAnimation;
	[SerializeField] private AnimationClip humanJumpAnimation;
	[SerializeField] private Rigidbody2D rb2D;
	[SerializeField] private PlayerController playerController;
	[SerializeField] private Transformation transformation;

	private Animator animator;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void Update() {
		animator.SetFloat(speedYHash, rb2D.velocity.y);
	}

	private void OnEnable() {
		transformation.OnTransformInterrupt.AddListener(TransformInterrupt);
	}

	private void OnDisable() {
		transformation.OnTransformInterrupt.RemoveListener(TransformInterrupt);
	}

	public void JumpStart() {
		if (playerController.HumanControls)
			animator.SetFloat(jumpSpeedHash, 1f / (playerController.HumanJumpLength / humanJumpAnimation.length));
		else
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
		animator.SetBool(isTransformingHash, true);
	}

	public void TransformInterrupt(float transformationDone) {
		float timeRemaining = transformation.TransformDuration - transformationDone;
		animator.Play("Human To Werewolf", 0, timeRemaining / transformation.TransformDuration);

		transformation.State = TransformationState.Human;
		transformation.TransformToWolf(timeRemaining);
	}

	public void TransformEnd() {
		animator.SetBool(isTransformingHash, false);
	}

	public void PickUpItem() {
		animator.SetTrigger(doPickUpHash);
	}

	public void DropItem() {
		animator.SetTrigger(doDropHash);
	}
}
