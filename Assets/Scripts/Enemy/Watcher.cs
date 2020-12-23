using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Watcher : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int isWalkingHash = Animator.StringToHash("isWalking");
	private static readonly int isAttackingHash = Animator.StringToHash("isAttacking");
	private static readonly int fixLampHash = Animator.StringToHash("fixLamp");
	private static readonly int endFixLampHash = Animator.StringToHash("endFixLamp");
	private static readonly int turnHash = Animator.StringToHash("Turn");

	[SerializeField] private PlayerDetectionCone lantern;
	[SerializeField] private Collider2D wallCollider = null;
	[SerializeField] private Collider2D groundCollider = null;
	[SerializeField] private LayerMask wallLayer = 1 << 8;

	[Space]
	[SerializeField] private AnimationClip standClip;

	[Header("AI")]
	[SerializeField] private bool doMovement = true;
	[SerializeField] private Transform point1 = null, point2 = null;
	[SerializeField] private float movementSpeed = 5f;
	[SerializeField] private float animationSpeedScale = 1f;
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

	[Header("Sounds")]
	[SerializeField] private AudioSource tinkerSound;
	[SerializeField] private AudioSource floorSound;

	[Header("Events")]
	[SerializeField] private UnityEvent onDetected = null;
	[SerializeField] private UnityEvent onLost = null;
	[SerializeField] private UnityEvent onDamage = null;

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

	private float turnDirection;

	public float DamageTime => damageTime;

	private void Start() {
		animator = GetComponent<Animator>();

		playerTransform = GameObject.FindWithTag("Player").transform;
		playerHealth = playerTransform.GetComponent<Health>();
		playerController = playerTransform.GetComponent<PlayerController>();
		playerTransformation = playerTransform.GetComponent<Transformation>();

		Camera mainCamera = Camera.main;
		if (mainCamera != null)
			snappingCamera = mainCamera.GetComponent<SnappingCamera>();

		animator.SetFloat(speedHash, movementSpeed * animationSpeedScale);
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
		facing = Mathf.Sign(transform.localScale.x);

		playerPosition = playerTransform.position;
		playerDirection = (playerPosition - lantern.transform.position).normalized;

		bool isWalking = false;
		if (doMovement && state != State.Recharging && state != State.Turning) {
			if (UpdateMovement())
				isWalking = true;
		}

		animator.SetBool(isWalkingHash, isWalking);

		if (state == State.Tracking && !isPlayerConsideredVisible) {
			if (playerLooseTimer.Tick()) {
				state = State.Patrolling;
				lanternAngle = 0;
			}
		}

		if (state == State.Following || state == State.Tracking) {
			// Facing towards the player.
			SetFacing(Mathf.Sign(playerPosition.x - transform.position.x));
			// Angle lantern towards player.
			lanternAngle = MathX.Angle(playerDirection) * Mathf.Rad2Deg;
			if (facing < 0) lanternAngle = 180f - lanternAngle;
		}

		lantern.Rotation = lanternAngle;
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

				onDamage.Invoke();
			}
		}
	}

	private bool UpdateMovement() {
		if (state != State.Patrolling &&
			(isBlocked || Mathf.Abs(playerPosition.x - transform.position.x) <= minFollowingDistance)) return false;

		Vector3 position = transform.position;
		position.x += movementSpeed * facing * Time.deltaTime;
		transform.position = position;

		if (state == State.Patrolling) {
			if (position.x <= point1.position.x) {
				Turn(1);
				lanternAngle = 0;
			}
			else if (position.x >= point2.position.x) {
				Turn(-1);
				lanternAngle = 0;
			}
		}

		return true;
	}

	private void Turn(float direction) {
		if (facing == direction) return;

		animator.Play(turnHash);
		state = State.Turning;
		turnDirection = direction;
	}

	private void SetFacing(float direction) {
		facing = direction;
		Vector3 localScale = transform.localScale;
		localScale.x = facing;
		transform.localScale = localScale;
	}

	private void UpdatePlayerVisibility() {
		if (isPlayerVisible == (isPlayerVisible = state != State.Turning && lantern.IsPlayerVisible)) return;
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


	// Animation events
	private void OnTurnFinished() {
		state = State.Patrolling;
		SetFacing(turnDirection);
	}

	private void PlayTinkerSound() {
		tinkerSound.Play();
	}

	private void PlayFloorSound() {
		floorSound.Play();
	}


#if UNITY_EDITOR
	private void OnValidate() {
		if (Application.isPlaying) {
			Animator anim = GetComponent<Animator>();
			if (anim.isInitialized)
				anim.SetFloat(speedHash, movementSpeed * animationSpeedScale);
		}
	}
#endif


	public enum State {
		Patrolling,
		Following,
		Tracking,
		Recharging,
		Turning,
	}
}
