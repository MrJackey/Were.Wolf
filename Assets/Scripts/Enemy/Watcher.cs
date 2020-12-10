﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Watcher : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isAttackingHash = Animator.StringToHash("isAttacking");
	private static readonly int fixLampHash = Animator.StringToHash("fixLamp");
	private static readonly int endFixLampHash = Animator.StringToHash("endFixLamp");

	[SerializeField] private Vector2 eyeOffset = Vector2.zero;
	[SerializeField] private float visionConeAngle = 30f;
	[SerializeField] private float visionDistance = 4f;
	[SerializeField, Range(1, 15)]
	private int visionRayCount = 3;
	[SerializeField] private LayerMask raycastLayers = -1;
	[SerializeField] private bool ignoreTriggers = true;

	[Space]
	[SerializeField] private Transform lantern = null;
	[SerializeField] private Collider2D wallCollider = null;
	[SerializeField] private Collider2D groundCollider = null;
	[SerializeField] private LayerMask wallLayer = 1 << 8;

	[Space]
	[SerializeField] private AnimationClip standClip;

	[Header("AI")]
	[SerializeField] private bool doMovement = true;
	[SerializeField] private Transform point1 = null, point2 = null;
	[SerializeField] private float movementSpeed = 5f;
	[Space]
	[SerializeField] private float minFollowingDistance = 3f;
	[SerializeField] private float playerForgetTime = 0.5f;
	[SerializeField] private float lanternRechargeDuration = 5f;
	[SerializeField] private float visibilityPadTime = 0.05f;
	[SerializeField] private float damage = 1f;
	[SerializeField] private float damageTime = 5f;

	[Header("Effect")]
	[SerializeField] private GameObject detectionEffectPrefab = null;
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

	private GameObject activeEffect;
	private Transform playerTransform;
	private Health playerHealth;
	private PlayerController playerController;
	private Transformation playerTransformation;
	private SnappingCamera snappingCamera;
	private Animator animator;

	private State state = State.Patrolling;
	private bool isPlayerVisible;
	private bool isPlayerConsideredVisible;
	private bool isBlocked;
	private float facing = 1;
	private float lanternAngle;
	private Vector3 playerPosition;
	private Vector2 playerDirection;
	private SimpleTimer playerLooseTimer;
	private SimpleTimer damageTimer;
	private SimpleTimer rechargeTimer;
	private SimpleTimer looseVisibilityTimer;

	private float currentSpeed;


	private void Start() {
		animator = GetComponent<Animator>();

		playerTransform = GameObject.FindWithTag("Player").transform;
		playerHealth = playerTransform.GetComponent<Health>();
		playerController = playerTransform.GetComponent<PlayerController>();
		playerTransformation = playerTransform.GetComponent<Transformation>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();
	}

	private void OnDisable() {
		Gamepad.current?.SetMotorSpeeds(0, 0);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		isBlocked = wallCollider.IsTouchingLayers(wallLayer) || !groundCollider.IsTouchingLayers(wallLayer);
	}

	private void OnTriggerExit2D(Collider2D other) {
		isBlocked = wallCollider.IsTouchingLayers(wallLayer) || !groundCollider.IsTouchingLayers(wallLayer);
	}

	private void Update() {
		lantern.localPosition = eyeOffset;
		facing = Mathf.Sign(transform.localScale.x);

		playerPosition = playerTransform.position;
		playerDirection = (playerPosition - lantern.position).normalized;

		currentSpeed = 0f;
		if (doMovement && state != State.Recharging)
			UpdateMovement();

		animator.SetFloat(speedHash, currentSpeed);

		if (state == State.Tracking && !isPlayerConsideredVisible) {
			if (playerLooseTimer.Tick()) {
				state = State.Patrolling;
				lanternAngle = 0;
			}
		}

		if (state != State.Patrolling && state != State.Recharging) {
			// Facing towards the player.
			SetFacing(Mathf.Sign(playerPosition.x - transform.position.x));
			// Angle lantern towards player.
			lanternAngle = MathX.Angle(playerDirection) * Mathf.Rad2Deg;
			if (facing < 0) lanternAngle = 180f - lanternAngle;
		}

		lantern.localRotation = Quaternion.AngleAxis(lanternAngle, Vector3.forward);
		if (state != State.Recharging)
			UpdatePlayerVisibility();

		if (looseVisibilityTimer.Tick()) {
			state = State.Tracking;
			isPlayerConsideredVisible = false;
			playerLooseTimer.Reset(playerForgetTime);
			OnLost();
		}

		animator.SetBool(isAttackingHash, isPlayerConsideredVisible);

		if (state == State.Following) {
			Debug.Assert(isPlayerConsideredVisible, "isPlayerVisible && state == State.Following");

			if (damageTimer.Tick()) {
				playerHealth.TakeDamage(damage);
				StartCoroutine(CoRechargeLantern());
			}
		}
	}

	private void UpdateMovement() {
		if (state != State.Patrolling &&
			(isBlocked || Mathf.Abs(playerPosition.x - transform.position.x) <= minFollowingDistance)) return;

		Vector3 position = transform.position;
		position.x += movementSpeed * facing * Time.deltaTime;
		transform.position = position;

		if (state == State.Patrolling) {
			if (position.x <= point1.position.x) {
				SetFacing(1);
				lanternAngle = 0;
			}
			else if (position.x >= point2.position.x) {
				SetFacing(-1);
				lanternAngle = 0;
			}
		}

		currentSpeed = movementSpeed;
	}

	private void SetFacing(float direction) {
		facing = direction;
		Vector3 localScale = transform.localScale;
		localScale.x = facing;
		transform.localScale = localScale;
	}

	private void UpdatePlayerVisibility() {
		if (isPlayerVisible == (isPlayerVisible = CheckPlayerVisible())) return;
		if (isPlayerVisible) {
			if (!isPlayerConsideredVisible) {
				state = State.Following;
				damageTimer.Reset(damageTime);
				OnDetected();
			}

			isPlayerConsideredVisible = true;
			looseVisibilityTimer.Stop();
		}
		else {
			if (looseVisibilityTimer.Elapsed)
				looseVisibilityTimer.Reset(visibilityPadTime);
		}
	}

	private bool CheckPlayerVisible() {
		Vector2 eyePosition = transform.TransformPoint(eyeOffset);
		Vector2 forward = lantern.right * facing;
		float angleStep = visionConeAngle / (visionRayCount - 1);

		for (int i = 0; i < visionRayCount; i++) {
			float angle = -visionConeAngle / 2f + angleStep * i;
			Vector2 direction = MathX.Rotate(forward, angle * Mathf.Deg2Rad);

			if (DoSingleRaycast(eyePosition, direction)) {
			#if UNITY_EDITOR
				if (!Application.isPlaying) continue;
			#endif

				return true;
			}
		}

		return false;
	}

	private bool DoSingleRaycast(Vector2 origin, Vector2 direction) {
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, visionDistance, raycastLayers);

	#if UNITY_EDITOR
		float distance = hit.collider != null ? hit.distance : visionDistance;
		Debug.DrawRay(origin, direction * distance, Color.red);

		if (hit.collider != null)
			Debug.DrawLine(hit.point, hit.point + hit.normal * 0.1f, Color.yellow);
	#endif

		if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player")) {
			if (ignoreTriggers && hit.collider.isTrigger)
				return false;

			Transformation transformation = hit.rigidbody.GetComponent<Transformation>();
			if (transformation == null || transformation.State != TransformationState.Human)
				return true;
		}

		return false;
	}

	private void OnDetected() {
		playerController.SpeedMultiplier = playerSpeedMultiplier;
		playerTransformation.AllowTransformation = false;
		if (detectionEffectPrefab != null)
			activeEffect = Instantiate(detectionEffectPrefab, playerTransform, false);

		if (cameraShake && snappingCamera != null) {
			snappingCamera.BeginShake(shakeFrequency, shakeAmplitude);
			if (playerTransform.GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
				Gamepad.current?.SetMotorSpeeds(rumbleFrequencies.x, rumbleFrequencies.y);
		}

		onDetected.Invoke();
	}

	private void OnLost() {
		playerController.SpeedMultiplier = 1f;
		playerTransformation.AllowTransformation = true;
		if (activeEffect != null)
			Destroy(activeEffect);

		if (cameraShake && snappingCamera != null) {
			snappingCamera.EndShake();
			Gamepad.current?.SetMotorSpeeds(0, 0);
		}

		onLost.Invoke();
	}

	private IEnumerator CoRechargeLantern() {
		state = State.Recharging;
		lantern.gameObject.SetActive(false);
		rechargeTimer.Reset(lanternRechargeDuration);
		looseVisibilityTimer.Stop();
		isPlayerConsideredVisible = false;
		isPlayerVisible = false;
		lanternAngle = 0;
		OnLost();

		animator.SetTrigger(fixLampHash);
		yield return new WaitForSeconds(lanternRechargeDuration - standClip.length);

		animator.SetTrigger(endFixLampHash);
		yield return new WaitForSeconds(standClip.length);

		state = State.Patrolling;
		lantern.gameObject.SetActive(true);
	}

	private void OnDrawGizmos() {
		if (!enabled || Application.isPlaying) return;
		Vector3 eyePosition = transform.TransformPoint(eyeOffset);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(eyePosition, 0.2f);

		CheckPlayerVisible();
	}

	private void OnValidate() {
		lantern.localPosition = eyeOffset;
	}


	public enum State {
		Patrolling,
		Following,
		Tracking,
		Recharging,
	}
}
