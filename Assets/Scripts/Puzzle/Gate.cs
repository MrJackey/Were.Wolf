using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : SignalReceiver {
	private static readonly int isOpenHash = Animator.StringToHash("isOpen");

	[Header("Gate")]
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;
	[SerializeField] private float showDuration = 1f;
	[SerializeField] private UnityEvent onEnter;

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionMultiplier = 0.10f;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
	}

	public void Toggle() {
		if (panCamera && camera != null)
			StartCoroutine(ShowEvent());
		else
			animator.SetBool(isOpenHash, IsActivated);
	}

	private IEnumerator ShowEvent() {
		playerController.AllowControls = false;
		float oldTransDur = camera.TransitionDuration;
		float newTransDur = oldTransDur * cameraTransitionMultiplier * Vector2.Distance(transform.position, player.transform.position);

		camera.TransitionDuration = newTransDur;
		camera.Target = transform;

		yield return new WaitForSeconds(newTransDur * 2f);
		animator.SetBool(isOpenHash, IsActivated);

		yield return new WaitForSeconds(showDuration);
		camera.Target = player.transform;
		camera.TransitionDuration = oldTransDur;
		playerController.AllowControls = true;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger) {
			onEnter.Invoke();
		}
	}
}
