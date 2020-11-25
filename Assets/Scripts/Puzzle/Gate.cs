using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : SignalReceiver {
	private static readonly int isOpenHash = Animator.StringToHash("isOpen");

	[SerializeField] private bool isOpen;
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;
	[SerializeField] private float showDuration = 1f;

	private new SnappingCamera camera;
	private Transform player;
	private PlayerController playerController;
	private float cameraTransitionMultiplier = 0.10f;

	private void Awake() {
		if (isOpen)
			Open();

		GameObject playerObj = GameObject.FindWithTag("Player");
		player = playerObj.transform;
		playerController = playerObj.GetComponent<PlayerController>();
		camera = Camera.main.GetComponent<SnappingCamera>();
	}

	public void Open() {
		isOpen = true;

		if (panCamera && camera != null)
			StartCoroutine(ShowEvent());
		else
			animator.SetBool(isOpenHash, isOpen);
	}

	public void Close() {
		isOpen = false;

		if (panCamera && camera != null)
			StartCoroutine(ShowEvent());
		else
			animator.SetBool(isOpenHash, isOpen);
	}

	private IEnumerator ShowEvent() {
		playerController.AllowControls = false;
		float oldTransDur = camera.TransitionDuration;
		float newTransDur = oldTransDur * cameraTransitionMultiplier * Vector2.Distance(transform.position, player.transform.position);

		camera.TransitionDuration = newTransDur;
		camera.Target = transform;

		yield return new WaitForSeconds(newTransDur * 2f);
		animator.SetBool(isOpenHash, isOpen);

		yield return new WaitForSeconds(showDuration);
		camera.Target = player.transform;
		camera.TransitionDuration = oldTransDur;
		playerController.AllowControls = true;
	}
}
