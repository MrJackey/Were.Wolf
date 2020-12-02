using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(101)]
public class SpeechZone : MonoBehaviour {
	[SerializeField] private Canvas speechCanvasPrefab = null;
	[SerializeField] private GameObject speechBubblePrefab = null;
	[SerializeField] private Vector2 textOffset = default;

	[Space]
	[SerializeField, Tag]
	private string triggerTag = "Player";
	[SerializeField] private bool ignoreTriggers = true;
	[SerializeField] private bool forceShow = false;

	[Space]
	[SerializeField, TextArea]
	private string message = null;

	[SerializeField] private bool slowWrite = false;
	[SerializeField, EnableIf(nameof(slowWrite))]
	private float charactersPerSecond = 15;

	[SerializeField] private bool fadeOut = false;
	[SerializeField, EnableIf(nameof(fadeOut))]
	private float fadeDuration = 0.5f;

	private static Canvas canvas;

	private Camera mainCamera;
	private RectTransform messageBubble;
	private CanvasGroup messageBubbleCanvasGroup;
	private Text messageText;
	private bool isShowing;
	private Coroutine slowWriteRoutine;
	private Coroutine fadeOutRoutine;

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
		messageBubble.anchoredPosition = canvas.transform.InverseTransformPoint(transform.position + (Vector3)textOffset);
	}

	private bool IsMatchingTarget(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return false;
		return other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(triggerTag);
	}

	private void ShowMessage() {
		if (isShowing) return;
		SetupCanvas();

		if (messageBubble == null) {
			messageBubble = (RectTransform)Instantiate(speechBubblePrefab, canvas.transform).transform;
			messageBubbleCanvasGroup = messageBubble.GetComponent<CanvasGroup>();
		}

		if (fadeOutRoutine != null) {
			StopCoroutine(fadeOutRoutine);
			fadeOutRoutine = null;
			messageBubbleCanvasGroup.alpha = 1f;
		}

		UpdateMessagePosition();
		messageText = messageBubble.GetComponentInChildren<Text>();

		if (slowWrite)
			slowWriteRoutine = StartCoroutine(CoSlowWrite());
		else
			messageText.text = message;

		messageBubble.gameObject.SetActive(true);
		isShowing = true;
	}

	private void HideMessage() {
		if (!isShowing) return;
		if (slowWriteRoutine != null) {
			StopCoroutine(slowWriteRoutine);
			slowWriteRoutine = null;
		}

		if (fadeOut)
			fadeOutRoutine = StartCoroutine(CoFadeOut());
		else
			messageBubble.gameObject.SetActive(false);
		isShowing = false;
	}

	private void SetupCanvas() {
		if (canvas != null) return;

		canvas = Instantiate(speechCanvasPrefab);
		canvas.worldCamera = mainCamera;
	}

	private IEnumerator CoSlowWrite() {
		float charDelay = 1f / charactersPerSecond;
		string msg = message;
		int length = 0;

		messageText.text = "";

		while (length < msg.Length) {
			do
				length++;
			while (length < msg.Length && char.IsWhiteSpace(msg[length]));

			messageText.text = msg.Substring(0, length);

			yield return new WaitForSeconds(charDelay);
		}
	}

	private IEnumerator CoFadeOut() {
		for (float time = 0; time < fadeDuration; time += Time.deltaTime) {
			messageBubbleCanvasGroup.alpha = MathX.EaseOutQuad(1, 0, time / fadeDuration);
			yield return null;
		}

		messageBubble.gameObject.SetActive(false);
		messageBubbleCanvasGroup.alpha = 1f;
		fadeOutRoutine = null;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		if (!Application.isPlaying)
			mainCamera = Camera.main;

		if (mainCamera == null) return;

		Vector2 position = transform.position + (Vector3)textOffset;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(position, 0.2f);
	}
#endif
}