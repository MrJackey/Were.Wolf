using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour {
	[SerializeField] private bool isActivated;
	[SerializeField] private UnityEvent onActivated;

	public bool IsActivated => isActivated;

	private void Start() {
		if (isActivated)
			onActivated.Invoke();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player")) {
			if (!isActivated) {
				isActivated = true;
				onActivated.Invoke();
			}
		}
	}
}