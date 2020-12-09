using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Gate : SignalReceiver {
	private static readonly int isOpenHash = Animator.StringToHash("isOpen");

	[Header("Gate")]
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;
	[SerializeField, EnableIf(nameof(panCamera))]
	private float showDuration = 1f;
	[SerializeField, EnableIf(nameof(panCamera))]
	private float showCooldown = 7.5f;

	private static Queue<Gate> panningQueue = new Queue<Gate>();

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionDuration;
	private float cameraTransitionMultiplier = 0.10f;
	private bool isShowing = false;
	private bool allowShow = true;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraTransitionDuration = camera.TransitionDuration;
	}

	private void OnEnable() => AddInternalListeners();

	private void OnDisable() => RemoveInternalListeners();

	protected void AddInternalListeners() {
		onActivation.AddListener(Toggle);
		onDeactivation.AddListener(Toggle);
	}

	protected void RemoveInternalListeners() {
		onActivation.RemoveListener(Toggle);
		onDeactivation.RemoveListener(Toggle);
	}

	public void Toggle() {
		if (panCamera && camera != null && !isShowing && allowShow && isInitialized) {
			panningQueue.Enqueue(this);

			if (panningQueue.Count == 1)
				StartCoroutine(CoShowEvent());
		}
		else if (animator.isInitialized)
			animator.SetBool(isOpenHash, IsActivated);
	}

	private IEnumerator CoShowEvent() {
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
		panningQueue.Dequeue();
		if (panningQueue.Count > 0) {
			isShowing = false;
			StartCoroutine(CoShowCooldown());
			StartCoroutine(panningQueue.Peek().CoShowEvent());
			yield break;
		}

		camera.Target = player.transform;

		yield return new WaitForSecondsRealtime(newTransitionDuration * 2f);
		camera.TransitionDuration = cameraTransitionDuration;
		playerController.AllowControls = true;
		Time.timeScale = 1;
		isShowing = false;
		StartCoroutine(CoShowCooldown());
	}

	private IEnumerator CoShowCooldown() {
		allowShow = false;
		yield return new WaitForSeconds(showCooldown);

		allowShow = true;
	}
}
