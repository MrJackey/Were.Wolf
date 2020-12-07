using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Gate : SignalReceiver {
	private static readonly int isOpenHash = Animator.StringToHash("isOpen");

	[Header("Gate")]
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;
	[SerializeField] private float showDuration = 1f;

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionDuration;
	private float cameraTransitionMultiplier = 0.10f;
	private bool isShowing = false;
	private bool allowShow = false;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraTransitionDuration = camera.TransitionDuration;
	}

	public void Toggle() {
		if (panCamera && camera != null && !isShowing && allowShow)
			StartCoroutine(ShowEvent());
		else if (animator.isInitialized)
			animator.SetBool(isOpenHash, IsActivated);

		allowShow = true;
	}

	private IEnumerator ShowEvent() {
		isShowing = true;
		playerController.AllowControls = false;
		Time.timeScale = 0;
		float newTransitionDuration = cameraTransitionDuration *
		                              cameraTransitionMultiplier *
		                              Vector2.Distance(transform.position, player.position);

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
		isShowing = false;
	}
}
