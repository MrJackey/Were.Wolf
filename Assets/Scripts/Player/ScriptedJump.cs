using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ScriptedJump : MonoBehaviour {
	[SerializeField] private float jumpStartTime = 0.1f;
	[SerializeField] private float jumpLength = 0.05f;
	[SerializeField] private float moveLength = 0.45f;

	private PlayerController playerController;
	private SimpleTimer jumpStartTimer;
	private SimpleTimer jumpLengthTimer;
	private SimpleTimer moveTimer;

	private bool firstFrame = true;

	private void Awake() {
		playerController = GetComponent<PlayerController>();
	}

	private void OnEnable() {
		moveTimer.Reset(moveLength);
		jumpStartTimer.Reset(jumpStartTime);
	}

	private void Update() {
		if (firstFrame) {
			firstFrame = false;
			return;
		}

		if (jumpStartTimer.Tick()) {
			jumpLengthTimer.Reset(jumpLength);
			playerController.JumpInputDown = true;
		}

		if (jumpLengthTimer.Tick())
			playerController.JumpInputUp = true;

		if (moveTimer.Tick()) {
			playerController.MoveInput = Vector2.zero;
			enabled = false;
		}

		if (!moveTimer.Elapsed)
			playerController.MoveInput = new Vector2(1, 0);
	}
}