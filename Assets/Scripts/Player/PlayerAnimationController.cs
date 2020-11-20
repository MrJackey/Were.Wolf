using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
	private static readonly int jumpHash = Animator.StringToHash("jump");
	private static readonly int isGroundedHash = Animator.StringToHash("isGrounded");
	private static readonly int isHumanHash = Animator.StringToHash("isHuman");
	private static readonly int transformSpeedHash = Animator.StringToHash("transformSpeed");
	private static readonly int jumpSpeedHash = Animator.StringToHash("jumpSpeed");

	[SerializeField] private AnimationClip transformAnimation;
	[SerializeField] private AnimationClip jumpAnimation;

	private Animator animator;
	private PlayerController playerController;
	private Transformation transformation;

	private void Start() {
		animator = GetComponent<Animator>();
		playerController = GetComponent<PlayerController>();
		transformation = GetComponent<Transformation>();
	}

	private void Update() {
		animator.SetBool(isGroundedHash, playerController.IsGrounded);
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
		animator.SetBool(isHumanHash, transformation.TransformationState != TransformationStates.Human);
	}

	public void TransformInterrupt() {
		animator.SetBool(isHumanHash, transformation.TransformationState == TransformationStates.Human);
	}

	public void TransformEnd() { }
}