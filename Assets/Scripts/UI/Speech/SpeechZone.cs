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
	[SerializeField] private bool ignoreTriggers = true;

	[Space]
	[SerializeField] private float charactersPerSecond = 15;
	[SerializeField] private bool stopOnLeave = true;
	[SerializeField] private bool fadeOut = false;
	[SerializeField, EnableIf(nameof(fadeOut))]
	private float fadeDuration = 0.5f;

	[Space]
	[SerializeField] private MessageItem[] messages = new MessageItem[1];
	[SerializeField] private PlayerController playerController;
	[SerializeField] private Animator playerAnimator;

	private static Canvas canvas;

	private Camera mainCamera;
	private RectTransform messageBubble;
	private CanvasGroup messageBubbleCanvasGroup;
	private Text messageText;
	private bool isShowing;
	private Coroutine showRoutine;
	private Coroutine fadeOutRoutine;

	private Transformation playerTransformation;

	private void Start() {
		mainCamera = Camera.main;
		playerTransformation = GameObject.FindWithTag("Player").GetComponentUnlessNull<Transformation>();
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
		if (stopOnLeave && IsMatchingTarget(other))
			HideMessage(fadeOut);
	}

	private void UpdateMessagePosition() {
		messageBubble.anchoredPosition = canvas.transform.InverseTransformPoint(transform.position + (Vector3)textOffset);
	}

	private bool IsMatchingTarget(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return false;
		return other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player");
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

		showRoutine = StartCoroutine(CoShowMessages());

		messageBubble.gameObject.SetActive(true);
		isShowing = true;
	}

	private void HideMessage(bool fade) {
		if (!isShowing) return;
		if (showRoutine != null) {
			StopCoroutine(showRoutine);
			showRoutine = null;
		}

		if (fade) {
			fadeOutRoutine = StartCoroutine(CoFadeOut());
		}
		else {
			messageText.text = "";
			messageText.gameObject.SetActive(false);
		}
		isShowing = false;

		if (playerController != null) {
			playerController.enabled = true;
			playerAnimator.enabled = true;
		}
	}

	private void SetupCanvas() {
		if (canvas != null) return;

		canvas = Instantiate(speechCanvasPrefab);
		canvas.worldCamera = mainCamera;
	}

	private IEnumerator CoShowMessages() {
		foreach (MessageItem item in messages) {
			if (item.useInForm != Form.Both && (
				playerTransformation.State == TransformationState.Human && item.useInForm != Form.Human ||
				playerTransformation.State == TransformationState.Wolf && item.useInForm != Form.Werewolf)) continue;

			if (item.slowWrite) {
				// Pass through to allow stopping.
				IEnumerator slowWriteEnumerator = CoSlowWrite(item.message, charactersPerSecond);
				while (slowWriteEnumerator.MoveNext())
					yield return slowWriteEnumerator.Current;
			}
			else {
				messageText.text = item.message;
			}

			if (item.delay > 0) {
				yield return new WaitForSeconds(item.delay);
			}
		}

		if (!stopOnLeave)
			HideMessage(fadeOut);
	}

	private IEnumerator CoSlowWrite(string message, float cps) {
		float charDelay = 1f / cps;
		int length = 0;

		messageText.text = "";

		while (length < message.Length) {
			do
				length++;
			while (length < message.Length && char.IsWhiteSpace(message[length]));

			messageText.text = message.Substring(0, length);

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


	public enum Form {
		Both,
		Human,
		Werewolf,
	}

	[Serializable]
	public class MessageItem {
		[TextArea]
		public string message = "";
		public bool slowWrite = false;
		public float delay = 1f;
		public Form useInForm = Form.Both;
	}
}