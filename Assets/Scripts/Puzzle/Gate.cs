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
	private AnimationClip showClip;
	[SerializeField, EnableIf(nameof(panCamera))]
	private float showCooldown = 7.5f;
	[SerializeField, EnableIf(nameof(panCamera))]
	private float cameraTransitionMultiplier = 3f;
	[SerializeField] private VignetteHighlight highlightPrefab;
	[SerializeField] private AudioSource audioSource;

	private static Queue<Gate> panningQueue = new Queue<Gate>();

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraBaseTransitionDuration;
	private bool isShowing = false;
	private bool allowShow = true;
	private bool isFirstSoundPlayed = false;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraBaseTransitionDuration = camera.TransitionDuration;
	}

	public void Toggle() {
		if (panCamera && camera != null && !isShowing && allowShow && isInitialized) {
			panningQueue.Enqueue(this);

			if (panningQueue.Count == 1) {
				VignetteHighlight highlight = Instantiate(highlightPrefab, transform.position, Quaternion.identity);
				StartCoroutine(CoShowEvent(highlight));
			}
		}
		else if (animator.isInitialized) {
			PlaySound();
			animator.SetBool(isOpenHash, IsActivated);
		}
	}

	private IEnumerator CoShowEvent(VignetteHighlight highlight) {
		Transform selfTransform = transform;
		highlight.WorldTarget = selfTransform;
		isShowing = true;
		playerController.AllowControls = false;
		Time.timeScale = 0;

		Vector2 selfGridPos = MathX.Floor(camera.WorldToGrid(selfTransform.position));
		Vector2 targetGridPos = MathX.Floor(camera.WorldToGrid(camera.Target.position));

		camera.TransitionDuration = Vector2.Distance(selfGridPos, targetGridPos) * cameraBaseTransitionDuration;
		camera.Target = selfTransform;

		yield return new WaitForSecondsRealtime(camera.TransitionDuration * cameraTransitionMultiplier);

		PlaySound();
		animator.SetBool(isOpenHash, IsActivated);

		yield return new WaitForSecondsRealtime(showClip.length);

		panningQueue.Dequeue();
		if (panningQueue.Count > 0) {
			isShowing = false;
			StartCoroutine(CoShowCooldown());
			StartCoroutine(panningQueue.Peek().CoShowEvent(highlight));
			yield break;
		}

		camera.Target = player.transform;
		highlight.FadeOut();

		yield return new WaitForSecondsRealtime(camera.TransitionDuration * cameraTransitionMultiplier);

		camera.TransitionDuration = cameraBaseTransitionDuration;
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

	public void PlaySound() {
		if (isFirstSoundPlayed) {
			audioSource.Play();
		}
		isFirstSoundPlayed = true;
	}
}
