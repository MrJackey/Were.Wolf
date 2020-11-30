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
	private float cameraTransitionDuration;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionMultiplier = 0.10f;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraTransitionDuration = camera.TransitionDuration;
	}

	public void Toggle() {
		if (panCamera && camera != null)
			StartCoroutine(ShowEvent());
		else
			animator.SetBool(isOpenHash, IsActivated);
	}

	private IEnumerator ShowEvent() {
		playerController.AllowControls = false;
		Time.timeScale = 0;
		float newTransitionDuration = cameraTransitionDuration *
		                              cameraTransitionMultiplier *
		                              Vector2.Distance(transform.position, player.transform.position);

		camera.TransitionDuration = newTransitionDuration;
		camera.Target = transform;

		yield return new WaitForSecondsRealtime(newTransitionDuration * 2f);
		animator.SetBool(isOpenHash, IsActivated);

		yield return new WaitForSecondsRealtime(showDuration);
		camera.Target = player.transform;

		yield return new WaitForSecondsRealtime(newTransitionDuration * 2f);
		camera.TransitionDuration = cameraTransitionDuration;
		playerController.AllowControls = true;
		Time.timeScale = 1;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger) {
			onEnter.Invoke();
		}
	}
}
