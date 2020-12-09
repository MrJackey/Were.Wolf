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
		playerObject.GetComponent<Rigidbody2D>().position = checkpoint.transform.position;
		playerObject.GetComponent<PlayerController>().Velocity = Vector2.zero;
	}
}
