using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : SignalReceiver {
	private static readonly int openHash = Animator.StringToHash("open");
	private static readonly int closeHash = Animator.StringToHash("close");

	[Header("Gate")]
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;
	[SerializeField, EnableIf(nameof(panCamera))]
	private AnimationClip showClip;
	[SerializeField, EnableIf(nameof(panCamera))]
	private float showCooldown = 7.5f;
	[SerializeField, EnableIf(nameof(panCamera))]
	private bool showOnEmitterUpdate;
	[SerializeField] private UnityEvent onStateUpdate;
	[SerializeField] private float cameraTransitionMultiplier = 3f;
	[SerializeField] private VignetteHighlight highlightPrefab;
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AnimationClip animationClip;

	private static Queue<Gate> panningQueue = new Queue<Gate>();

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraBaseTransitionDuration;
	private bool isShowing = false;
	private bool inShowQueue = false;
	private bool allowShow = true;
	private bool prevIsActivated = false;
	private float animationClipCurrentTime;
	private bool doCountUp = false, doCountDown = false;
	
	public bool PanCamera { set => panCamera = value; }

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraBaseTransitionDuration = camera.TransitionDuration;
		animator.Play(IsActivated ? openHash : closeHash, 0, 1);
		prevIsActivated = IsActivated;
	}

	private void Update() {
		UpdateAnimationTime();
	}

	public void Toggle() {
		if (panCamera && camera != null && allowShow && isInitialized) {
			AddToPanningQueue();
		}
		else if (animator.isInitialized && !inShowQueue && !isShowing) {
			if (prevIsActivated != IsActivated)
				PlaySound();

			UpdateAnimation();
			prevIsActivated = IsActivated;
			onStateUpdate.Invoke();
		}
	}

	private void AddToPanningQueue() {
		if (inShowQueue) return;

		panningQueue.Enqueue(this);
		inShowQueue = true;

		if (panningQueue.Count == 1) {
			VignetteHighlight highlight = Instantiate(highlightPrefab, transform.position, Quaternion.identity);
			StartCoroutine(CoShowEvent(highlight));
		}
	}

	private IEnumerator CoShowEvent(VignetteHighlight highlight) {
		isShowing = true;
		inShowQueue = false;
		Transform selfTransform = transform;
		highlight.WorldTarget = selfTransform;
		playerController.AllowControls = false;
		Time.timeScale = 0;

		Vector2 selfGridPos = MathX.Floor(camera.WorldToGrid(selfTransform.position));
		Vector2 targetGridPos = MathX.Floor(camera.WorldToGrid(camera.Target.position));

		camera.TransitionDuration = Vector2.Distance(selfGridPos, targetGridPos) * cameraBaseTransitionDuration;
		camera.Target = selfTransform;

		yield return new WaitForSecondsRealtime(camera.TransitionDuration * cameraTransitionMultiplier);

		if (prevIsActivated != IsActivated)
			PlaySound();

		UpdateAnimation();
		onStateUpdate.Invoke();

		yield return new WaitForSecondsRealtime(showClip.length);

		panningQueue.Dequeue();
		if (panningQueue.Count > 0) {
			isShowing = false;
			prevIsActivated = IsActivated;
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
		prevIsActivated = IsActivated;
		StartCoroutine(CoShowCooldown());
	}

	private IEnumerator CoShowCooldown() {
		allowShow = false;
		yield return new WaitForSeconds(showCooldown);

		allowShow = true;
	}

	private void UpdateAnimationTime() {
		if (doCountUp) 
			animationClipCurrentTime += Time.deltaTime;
		else if (doCountDown)
			animationClipCurrentTime -= Time.deltaTime;

		animationClipCurrentTime = Mathf.Clamp(animationClipCurrentTime, 0, animationClip.length);

		if (animationClipCurrentTime >= animationClip.length)
			doCountUp = false;
		else if (animationClipCurrentTime <= 0)
			doCountDown = false;
	}

	private void UpdateAnimation() {
		float animationClipTimeUpdate = animationClipCurrentTime / animationClip.length;
		if (IsActivated) {
			animator.Play(openHash, 0, animationClipTimeUpdate);
			doCountDown = false;
			doCountUp = true;
		}
		else {
			animator.Play(closeHash, 0, 1 - animationClipTimeUpdate);
			doCountUp = false;
			doCountDown = true;
		}
	}

	private void PlaySound() {
		audioSource.Play();
	}

	public void HandleEmitterUpdate() {
		if (panCamera && camera != null && showOnEmitterUpdate && allowShow)
			AddToPanningQueue();
		else if (!inShowQueue && !isShowing)
			onStateUpdate.Invoke();
	}
}