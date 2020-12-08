using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Detection : MonoBehaviour {
	[SerializeField] private Vector2 eyeOffset = Vector2.zero;
	[SerializeField] private float visionConeAngle = 30f;
	[SerializeField] private float visionDistance = 4f;
	[SerializeField, Range(1, 15)]
	private int visionRayCount = 3;
	[SerializeField] private LayerMask raycastLayers = -1;
	[SerializeField] private bool ignoreTriggers = true;
	[Space]
	[SerializeField] private Transform lantern;

	[Header("Effect")]
	[SerializeField] private GameObject detectionEffectPrefab = null;
	[SerializeField] private float damagePerSecond = 10;
	[SerializeField] private float playerSpeedMultiplier = 0.5f;
	[SerializeField] private Vector2 rumbleFrequencies = new Vector2(0.25f, 0.75f);
	[Space]
	[SerializeField] private bool cameraShake = true;
	[SerializeField, EnableIf(nameof(cameraShake))]
	private float shakeFrequency = 20f;
	[SerializeField, EnableIf(nameof(cameraShake))]
	private float shakeAmplitude = 0.3f;

	[Header("Events")]
	[SerializeField] private UnityEvent onDetected = null;
	[SerializeField] private UnityEvent onLost = null;

	private bool isPlayerVisible;
	private float facing = 1;
	private GameObject activeEffect;
	private GameObject playerObject;
	private Health playerHealth;
	private SnappingCamera snappingCamera;

	private State state = State.Patrolling;
	private bool isAttacking;
	private float lanternAngle;
	private Vector2 playerDirection;
	private float playerLooseTimer;

	[SerializeField] private float playerLooseTime = 0.5f;

	private void Start() {
		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void OnDisable() {
		if (Gamepad.current != null)
			Gamepad.current.SetMotorSpeeds(0, 0);
	}

	private void Update() {
		facing = Mathf.Sign(transform.localScale.x);

		Vector3 playerPosition = GameObject.FindWithTag("Player").transform.position;
		playerDirection = (playerPosition - lantern.position).normalized;

		lantern.localRotation = Quaternion.AngleAxis(lanternAngle, Vector3.forward);

		UpdatePlayerVisibility();

		// Facing towards the player.
		SetFacing(Mathf.Sign(playerPosition.x - transform.position.x));

		// Light follows player.
		if (isPlayerVisible) {
			SetLanternAngle(MathX.Angle(playerDirection) * Mathf.Rad2Deg);
		}
		else {
			SetLanternAngle(0);
		}

		// if (isPlayerVisible)
		// 	playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
	}

	private void SetFacing(float direction) {
		facing = direction;
		Vector3 localScale = transform.localScale;
		localScale.x = facing;
		transform.localScale = localScale;
	}

	private void SetLanternAngle(float angle) {
		if (facing < 0)
			angle = 180f - angle;
		lanternAngle = angle;
	}

	private void UpdatePlayerVisibility() {
		if (isPlayerVisible == (isPlayerVisible = CheckPlayerVisible(out GameObject go))) return;
		if (isPlayerVisible) {
			playerObject = go;
			playerHealth = playerObject.GetComponent<Health>();
			OnDetected();
		}
		else {
			OnLost();
		}
	}

	private bool CheckPlayerVisible(out GameObject go) {
		Vector2 eyePosition = transform.TransformPoint(eyeOffset);
		Vector2 forward = lantern.right * facing;
		float angleStep = visionConeAngle / (visionRayCount - 1);

		for (int i = 0; i < visionRayCount; i++) {
			float angle = -visionConeAngle / 2f + angleStep * i;
			Vector2 direction = MathX.Rotate(forward, angle * Mathf.Deg2Rad);

			if (DoSingleRaycast(eyePosition, direction, out go)) {
			#if UNITY_EDITOR
				if (!Application.isPlaying) continue;
			#endif

				return true;
			}
		}

		go = null;
		return false;
	}

	private bool DoSingleRaycast(Vector2 origin, Vector2 direction, out GameObject go) {
		go = null;
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, visionDistance, raycastLayers);

	#if UNITY_EDITOR
		float distance = hit.collider != null ? hit.distance : visionDistance;
		Debug.DrawRay(origin, direction * distance, Color.red);
	#endif

		if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player")) {
			if (ignoreTriggers && hit.collider.isTrigger)
				return false;

			Transformation transformation = hit.rigidbody.GetComponent<Transformation>();
			if (transformation == null || transformation.State != TransformationState.Human) {
				go = hit.rigidbody.gameObject;
				return true;
			}
		}

		return false;
	}

	private void OnDetected() {
		playerObject.GetComponent<PlayerController>().SpeedMultiplier = playerSpeedMultiplier;
		if (detectionEffectPrefab != null)
			activeEffect = Instantiate(detectionEffectPrefab, playerObject.transform, false);

		if (cameraShake && snappingCamera != null) {
			snappingCamera.BeginShake(shakeFrequency, shakeAmplitude);
			if (playerObject.GetComponent<PlayerInput>().currentControlScheme == "Gamepad" && Gamepad.current != null)
				Gamepad.current.SetMotorSpeeds(rumbleFrequencies.x, rumbleFrequencies.y);
		}

		onDetected.Invoke();
	}

	private void OnLost() {
		playerObject.GetComponent<PlayerController>().SpeedMultiplier = 1f;
		if (activeEffect != null)
			Destroy(activeEffect);

		if (cameraShake && snappingCamera != null) {
			snappingCamera.EndShake();
			if (Gamepad.current != null)
				Gamepad.current.SetMotorSpeeds(0, 0);
		}

		onLost.Invoke();
	}

	private void OnDrawGizmos() {
		if (!enabled) return;
		Vector3 eyePosition = transform.TransformPoint(eyeOffset);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(eyePosition, 0.2f);

		CheckPlayerVisible(out _);
	}


	public enum State {
		Patrolling,
		Following,
	}
}
