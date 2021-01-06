using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CutsceneHelper : MonoBehaviour {
	private static readonly int skipHash = Animator.StringToHash("Skip");

	[SerializeField] private SceneHelper sceneHelper;
	[SerializeField] private InputActionReference skipAction;
	[SerializeField] private GameObject skipInfo;
	[SerializeField] private RectTransform skipProgressBar;
	[SerializeField] private float skipStayDuration = 1.5f;
	[SerializeField] private float skipHoldDuration = 1.5f;
	[Space]
	[SerializeField] private UnityEvent onCutsceneStart;
	[SerializeField] private UnityEvent onCutsceneSkip;
	[SerializeField] private UnityEvent onCutsceneEnd;

	private SimpleTimer skipInfoFadeTimer;
	private Animator animator;
	private bool isSkipped;
	private bool isSkipBeingHeld;
	private float skipHoldTimer;
	private float maxSkipProgressBarWidth;

	private void Start() {
		animator = GetComponent<Animator>();
		maxSkipProgressBarWidth = ((RectTransform)skipProgressBar.parent).sizeDelta.x;
		SetSkipHoldProgressValue(0);
	}

	private void OnEnable() {
		skipAction.action.performed += OnSkipInput;
		skipAction.action.canceled += OnSkipInput;
		skipAction.action.Enable();
	}

	private void OnDisable() {
		skipAction.action.performed -= OnSkipInput;
		skipAction.action.canceled -= OnSkipInput;
	}

	private void Update() {
		if (isSkipped) return;

		if (IsAnyButtonDown()) {
			skipInfoFadeTimer.Reset(skipStayDuration);
			skipInfo.SetActive(true);
			SetSkipHoldProgressValue(0);
		}
		else if (skipInfoFadeTimer.Tick()) {
			skipInfo.SetActive(false);
		}

		if (isSkipBeingHeld && !sceneHelper.IsTransitioning) {
			skipHoldTimer += Time.deltaTime;

			float t = Mathf.Clamp01(skipHoldTimer / skipHoldDuration);
			SetSkipHoldProgressValue(t);

			if (skipHoldTimer >= skipHoldDuration)
				Skip();
		}
	}

	private bool IsAnyButtonDown() {
		return Keyboard.current != null && Keyboard.current.anyKey.isPressed ||
		       Mouse.current != null && Mouse.current.leftButton.isPressed ||
		       Gamepad.current != null && Gamepad.current.allControls.Any(control => control is ButtonControl button &&
			       button.isPressed);
	}

	private void SetSkipHoldProgressValue(float value) {
		skipProgressBar.sizeDelta = new Vector2(value * maxSkipProgressBarWidth, skipProgressBar.sizeDelta.y);
	}

	private void Skip() {
		animator.SetTrigger(skipHash);
		isSkipped = true;
		skipInfo.SetActive(false);
		onCutsceneSkip.Invoke();
	}

	private void OnSkipInput(InputAction.CallbackContext ctx) {
		if (ctx.performed) {
			if (!isSkipBeingHeld)
				skipHoldTimer = 0;

			isSkipBeingHeld = true;
		}
		else if (ctx.canceled) {
			skipHoldTimer = 0;
			isSkipBeingHeld = false;
			SetSkipHoldProgressValue(0);
		}
	}

	#region Animation events

	private void HandleCutsceneStart() {
		onCutsceneStart.Invoke();
	}

	private void HandleCutsceneEnd() {
		onCutsceneEnd.Invoke();
		Destroy(gameObject);
	}

	#endregion
}
