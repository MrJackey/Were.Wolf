using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour {
	[SerializeField] private SceneHelper sceneHelper = null;
	[SerializeField] private Checkpoint[] checkpoints = null;

	public void Respawn() {
		Checkpoint checkpoint = checkpoints.LastOrDefault(cp => cp.IsActivated);
		if (checkpoint == null) {
			sceneHelper.ReloadScene();
			return;
		}

		GameObject playerObject = GameObject.FindWithTag("Player");
		playerObject.GetComponent<Health>().RestoreHealth();
		playerObject.GetComponent<Knockbackable>().InterruptKnockback();
		playerObject.GetComponent<Rigidbody2D>().position = checkpoint.transform.position;
		PlayerController playerController = playerObject.GetComponent<PlayerController>();
		playerController.Velocity = Vector2.zero;
		playerController.DoKnockBack = false;
	}
}
