using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(101)]
public class SpeechZone : MonoBehaviour {
	[SerializeField] private Canvas speechCanvasPrefab = null;
	[SerializeField] private GameObject speechBubblePrefab = null;
	[SerializeField] private Vector2 offset = default;

	[SerializeField, TextArea]
	private string message = null;
	[Space]
	[SerializeField, Tag]
	private string triggerTag = "Player";
	[SerializeField] private bool ignoreTriggers = true;
	[SerializeField] private bool forceShow = false;

	private static Canvas canvas;

	private Camera mainCamera;
	private RectTransform messageBubble;
	private bool isShowing;

	private void Start() {
		mainCamera = Camera.main;
		if (forceShow)
			ShowMessage();
	}

	private void LateUpdate() {
		if (!isShowing) return;

		UpdateMessagePosition();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (IsMatchingTarget(other))
			ShowMessage();
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (!forceShow && IsMatchingTarget(other))
			HideMessage();
	}

	private void UpdateMessagePosition() {
		Vector2 position = canvas.transform.InverseTransformPoint(transform.position + (Vector3)offset);
		messageBubble.anchoredPosition = position;
	}

	private bool IsMatchingTarget(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return false;
		return other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(triggerTag);
	}

	private void ShowMessage() {
		SetupCanvas();

		if (messageBubble == null)
			messageBubble = (RectTransform)Instantiate(speechBubblePrefab, canvas.transform).transform;

		messageBubble.position = (Vector2)mainCamera.WorldToScreenPoint(transform.position + (Vector3)offset);
		messageBubble.GetComponentInChildren<Text>().text = message;
		messageBubble.gameObject.SetActive(true);
		isShowing = true;
	}

	private void HideMessage() {
		if (messageBubble != null)
			messageBubble.gameObject.SetActive(false);

		isShowing = false;
	}

	private void SetupCanvas() {
		if (canvas != null) return;

		canvas = Instantiate(speechCanvasPrefab);
		canvas.worldCamera = mainCamera;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		if (!Application.isPlaying)
			mainCamera = Camera.main;

		if (mainCamera == null) return;

		Vector2 position = transform.position + (Vector3)offset;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(position, 0.2f);
	}
#endif
}