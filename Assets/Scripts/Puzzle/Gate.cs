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
	[SerializeField] private InputActionReference enterActionReference;
	[SerializeField, Range(0.125f, 1f)]
	private float enterThreshold = 0.5f;
	[SerializeField] private UnityEvent onEnter;

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionDuration;
	private float cameraTransitionMultiplier = 0.10f;
	private bool isShowing = false;
	private bool canEnter = false;
	private bool isEntering;

	private void Awake() {
		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
		cameraTransitionDuration = camera.TransitionDuration;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger)
			canEnter = true;
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger)
			canEnter = false;
	}

	private void OnEnable() {
		if (enterActionReference == null) return;

		enterActionReference.action.performed += OnEnterInput;
	}

	private void OnDisable() {
		if (enterActionReference == null) return;

		enterActionReference.action.performed -= OnEnterInput;
	}

	public void Toggle() {
		if (panCamera && camera != null && !isShowing)
			StartCoroutine(ShowEvent());
		else if (animator.isInitialized)
			animator.SetBool(isOpenHash, IsActivated);
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

	private void OnEnterInput(InputAction.CallbackContext ctx) {
		if (!canEnter) return;

		Vector2 moveInput = ctx.ReadValue<Vector2>();
		if (moveInput.y > enterThreshold) {
			if (!isEntering) {
				isEntering = true;
				onEnter.Invoke();
			}
		}
		else {
			isEntering = false;
		}
	}
}
