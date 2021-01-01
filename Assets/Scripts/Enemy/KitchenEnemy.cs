using UnityEngine;

public class KitchenEnemy : MonoBehaviour {
	private static readonly int speedHash = Animator.StringToHash("speed");
	private static readonly int turnHash = Animator.StringToHash("Turn");
	private static readonly int attackUpHash = Animator.StringToHash("Attack Up");
	private static readonly int attackLeftHash = Animator.StringToHash("Attack Left");
	private static readonly int attackSplashHash = Animator.StringToHash("Attack Splash");
	private static readonly int isWalkingHash = Animator.StringToHash("isWalking");

	[Header("References")]
	[SerializeField] private Transform leftAttackPrefab = null;
	[SerializeField] private Transform upAttackPrefab = null;
	[SerializeField] private Transform splashAttackPrefab = null;
	[SerializeField] private Transform leftAttackPoint = null;
	[SerializeField] private Transform upAttackPoint = null;

	[Header("Movement")]
	[SerializeField] private bool doMovement = true;

	[SerializeField, EnableIf(nameof(doMovement))]
	private Transform leftPoint = null;

	[SerializeField, EnableIf(nameof(doMovement))]
	private Transform rightPoint = null;

	[SerializeField, EnableIf(nameof(doMovement))]
	private float movementSpeed = 2f;

	[SerializeField, EnableIf(nameof(doMovement))]
	private float animationSpeedMultiplier = 1f;

	[Header("Attack")]
	[SerializeField] private bool attackPlayerIfClose = true;
	[SerializeField] private AttackAction leftAttack = AttackAction.None;
	[SerializeField] private AttackAction rightAttack = AttackAction.None;

	[SerializeField, EnableIf(nameof(doMovement), false)]
	private AttackAction stillAttack = AttackAction.Right;

	[SerializeField, EnableIf(nameof(doMovement), false)]
	private float stillAttackDelay = 5f;

	[Header("Sounds")]
	[SerializeField] private AudioSource cauldronSound;
	[SerializeField] private AudioSource soupBeamSound;
	[SerializeField] private SoundRandomizer gruntSounds;

	private Animator animator;
	private State state = State.Walking;
	private float facing = -1;
	private float turnDirection;
	private bool turnAfterAttack;
	private bool attackAfterTurn;
	private bool doSplashAttack;
	private Direction attackDirection;
	private SimpleTimer stillAttackTimer;

	private bool isPlayerClose;
	private Transformation playerTransformation;

	private void Start() {
		animator = GetComponent<Animator>();

		GameObject playerObject = GameObject.FindWithTag("Player");
		if (playerObject != null)
			playerTransformation = playerObject.GetComponent<Transformation>();

		SetFacing(Mathf.Sign(transform.localScale.x));

		if (!doMovement) {
			animator.SetBool(isWalkingHash, false);
			stillAttackTimer.Reset(stillAttackDelay);
		}

		animator.SetFloat(speedHash, movementSpeed * animationSpeedMultiplier);
	}

	private void Update() {
		if (attackPlayerIfClose && isPlayerClose && state == State.Walking && !playerTransformation.IsHuman)
			AttackPlayer();

		if (doMovement) {
			UpdateMovement();
		}
		else if (stillAttackTimer.Tick()) {
			stillAttackTimer.Reset(stillAttackDelay);

			if (stillAttack != AttackAction.None)
				Attack((Direction)stillAttack);
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (!other.isTrigger && other.attachedRigidbody.CompareTag("Player"))
			isPlayerClose = true;
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (!other.isTrigger && other.attachedRigidbody.CompareTag("Player"))
			isPlayerClose = false;
	}

	private void UpdateMovement() {
		if (state != State.Walking) {
			animator.SetBool(isWalkingHash, false);
			return;
		}

		animator.SetBool(isWalkingHash, true);

		Transform self = transform;
		Vector3 position = self.position;
		position.x += movementSpeed * facing * Time.deltaTime;
		self.position = position;

		if (position.x <= leftPoint.position.x) {
			if (facing < 0)
				OnReachPoint(Direction.Left);
		}
		else if (position.x >= rightPoint.position.x) {
			if (facing >= 0)
				OnReachPoint(Direction.Right);
		}
	}

	private void OnReachPoint(Direction direction) {
		AttackAction action = direction == Direction.Left ? leftAttack : rightAttack;
		if (action == AttackAction.None) {
			Turn(-facing);
		}
		else {
			turnAfterAttack = true;
			Attack((Direction)action);
		}
	}

	private void Turn(float direction) {
		if (facing == direction) return;

		animator.Play(turnHash);
		state = State.Turning;
		turnDirection = direction;
	}

	private void SetFacing(float direction) {
		Transform self = transform;
		Vector3 scale = self.localScale;
		scale.x = facing;
		self.localScale = scale;
		facing = direction;
	}

	private void Attack(Direction direction) {
		if (doSplashAttack)
			animator.Play(attackSplashHash);
		else
			animator.Play(direction == Direction.Up ? attackUpHash : attackLeftHash);

		attackDirection = direction;
		state = State.Attacking;
	}

	private void AttackPlayer() {
		doSplashAttack = true;
		turnAfterAttack = false;
		Attack(Direction.Up);
	}


	#region Animation events

	private void OnAttackStart() {
		Debug.Assert(state == State.Attacking);

		if (doSplashAttack) {
			doSplashAttack = false;
			Instantiate(splashAttackPrefab, upAttackPoint);
			cauldronSound.Play();
			PlaySoupSound();
		}
		else if (attackDirection == Direction.Up) {
			Instantiate(upAttackPrefab, upAttackPoint);
			cauldronSound.Play();
			PlaySoupSound();
		}
		else {
			float attackFacing = attackDirection == Direction.Left ? -1 : 1;
			if (facing != attackFacing) {
				SetFacing(attackFacing);
				turnAfterAttack = false;
			}

			Instantiate(leftAttackPrefab, leftAttackPoint);
		}
	}

	private void OnAttackFinished() {
		Debug.Assert(state == State.Attacking);

		state = State.Walking;
		if (turnAfterAttack)
			Turn(-facing);
	}

	private void OnTurnFinished() {
		Debug.Assert(state == State.Turning);

		state = State.Walking;
		SetFacing(turnDirection);

		if (attackAfterTurn) {
			attackAfterTurn = false;
			turnAfterAttack = false;
			Attack(attackDirection);
		}
	}

	private void PlayGruntSound() {
		gruntSounds.PlayRandom();
	}

	private void PlaySoupSound() {
		soupBeamSound.Play();
	}

	#endregion

#if UNITY_EDITOR
	private void OnValidate() {
		if (!Application.isPlaying) return;
		Animator anim = GetComponent<Animator>();

		anim.SetFloat(speedHash, movementSpeed * animationSpeedMultiplier);
		if (!doMovement)
			anim.SetBool(isWalkingHash, false);

		if (stillAttackTimer.Elapsed)
			stillAttackTimer.Reset(stillAttackDelay);
	}
#endif


	private enum State {
		Walking,
		Turning,
		Attacking,
	}

	private enum Direction {
		Up = 1,
		Left = 2,
		Right = 3,
	}

	private enum AttackAction {
		None,
		Up = 1,
		Left = 2,
		Right = 3,
	}
}