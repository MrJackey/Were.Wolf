using UnityEngine;

public class Checkpoint : MonoBehaviour {
	[SerializeField] private bool isActivated;

	public bool IsActivated => isActivated;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player"))
			isActivated = true;
	}
}