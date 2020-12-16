using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DefaultExecutionOrder(101)]
public class SpeechZone : MonoBehaviour {
	[SerializeField] private Canvas speechCanvasPrefab = null;
	[SerializeField] private GameObject speechBubblePrefab = null;
	[SerializeField] private Vector2 textOffset = default;
	[SerializeField] private bool ignoreTriggers = true;

	[Space]
	[SerializeField, Tooltip("Always show the message.")]
	private bool forceShow;

	[SerializeField, Tooltip("Controls how to behave if the player transforms in the middle of a message.")]
	private ReshowMode reshowOnTransform = ReshowMode.NoReshow;

	[SerializeField, Tooltip("Controls the speed of slow write.")]
	private float charactersPerSecond = 15;

	[SerializeField, Tooltip("Stop showing the message when the player leaves the trigger.")]
	private bool stopOnLeave = true;

	[SerializeField, Tooltip("Fade out the text after the last message.")]
	private bool fadeOut = false;

	[SerializeField, EnableIf(nameof(fadeOut)), Tooltip("The duration of the fade out.")]
	private float fadeDuration = 0.5f;

	[Space]
	[SerializeField] private MessageItem[] messages = new MessageItem[1];

	[Header("Events")]
	[SerializeField] private UnityEvent onSpeechStart;
	[SerializeField] private UnityEvent onSpeechEnd;

	private static Canvas canvas;

	private Camera mainCamera;
	private RectTransform messageBubble;
	private CanvasGroup messageBubbleCanvasGroup;
	private Text messageText;
	private bool isShowing;
	private bool reshow;
	private bool isPlayerInTrigger;
	private TransformationState showingTransformationState;
	private Coroutine showRoutine;
	private Coroutine fadeOutRoutine;

	private Transformation playerTransformation;

	private void OnEnable() {
		mainCamera = Camera.main;
		playerTransformation = GameObject.FindWithTag("Player").GetComponentUnlessNull<Transformation>();
		playerTransformation.OnTransformEnd.AddListener(OnTransformEnd);
	}

	private void OnDisable() {
		playerTransformation.OnTransformEnd.RemoveListener(OnTransformEnd);
	}

	private void Start() {
		if (forceShow)
			ShowMessage();
	}

	private void OnTransformEnd() {
		if (!forceShow && !isPlayerInTrigger) return;

		switch (reshowOnTransform) {
			case ReshowMode.Interrupt:
				ReshowMessage();
				break;

			case ReshowMode.AfterMessage:
			case ReshowMode.WhenFinished:
				if (isShowing)
					reshow = true;
				else {
					ShowMessage();
				}

				break;
		}
	}

	private void LateUpdate() {
		if (!isShowing) return;

		UpdateMessagePosition();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (!forceShow && IsMatchingTarget(other)) {
			reshow = false;
			isPlayerInTrigger = true;
			ShowMessage();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (!forceShow && IsMatchingTarget(other)) {
			isPlayerInTrigger = false;
			if (stopOnLeave)
				HideMessage(fadeOut);
		}
	}

	private void UpdateMessagePosition() {
		messageBubble.anchoredPosition = canvas.transform.InverseTransformPoint(transform.position + (Vector3)textOffset);
	}

	private bool IsMatchingTarget(Collider2D other) {
		if (ignoreTriggers && other.isTrigger) return false;
		return other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player");
	}

	private void ReshowMessage() {
		reshow = false;
		if (isShowing)
			HideMessage(false);
		ShowMessage();
	}

	private void ShowMessage() {
		if (isShowing) return;
		SetupCanvas();
		reshow = false;

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

		showingTransformationState = playerTransformation.State;
		showRoutine = StartCoroutine(CoShowMessages());

		messageBubble.gameObject.SetActive(true);
		isShowing = true;
		onSpeechStart.Invoke();
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
		onSpeechEnd.Invoke();
	}

	private void SetupCanvas() {
		if (canvas != null) return;

		canvas = Instantiate(speechCanvasPrefab);
		canvas.worldCamera = mainCamera;
	}

	private IEnumerator CoShowMessages() {
		foreach (MessageItem item in messages) {
			if (item.useInForm != Form.Both && (
				showingTransformationState == TransformationState.Human && item.useInForm != Form.Human ||
				showingTransformationState == TransformationState.Wolf && item.useInForm != Form.Werewolf)) continue;

			if (item.slowWrite) {
				// Pass through to allow stopping.
				IEnumerator slowWriteEnumerator = CoSlowWrite(item.message, charactersPerSecond);
				while (slowWriteEnumerator.MoveNext())
					yield return slowWriteEnumerator.Current;
			}
			else {
				messageText.text = item.message;
			}

			if (item.delay > 0)
				yield return new WaitForSeconds(item.delay);

			if (reshow && reshowOnTransform == ReshowMode.AfterMessage) {
				ReshowMessage();
				yield break;
			}
		}

		if (reshow && reshowOnTransform == ReshowMode.WhenFinished) {
			ReshowMessage();
			yield break;
		}

		if (!stopOnLeave && !forceShow)
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

	public enum ReshowMode {
		[InspectorName("Don't Reshow"), Tooltip("Don't reshow message on transformation.")]
		NoReshow,
		[Tooltip("Interrupt the current message immediately.")]
		Interrupt,
		[Tooltip("Reshow after the current message.")]
		AfterMessage,
		[Tooltip("Reshow after all messages.")]
		WhenFinished,
	}

	[Serializable]
	public class MessageItem {
		[TextArea]
		public string message = "";
		public bool slowWrite = false;
		[Tooltip("The duration the message should stay on screen after writing.")]
		public float delay = 1f;
		[Tooltip("The transformation form the player must be in for this message to be used.")]
		public Form useInForm = Form.Both;
	}
}