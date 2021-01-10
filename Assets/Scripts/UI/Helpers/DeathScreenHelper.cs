using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeathScreenHelper : MonoBehaviour {
	private static readonly int showHash = Animator.StringToHash("Show");
	private static readonly int restartHash = Animator.StringToHash("Restart");
	private static readonly int humanDeathHash = Animator.StringToHash("HumanDeath");
	private static readonly int wolfDeathHash = Animator.StringToHash("WolfDeath");
	private static readonly int genericHash = Animator.StringToHash("Generic");
	private static readonly int spikeHash = Animator.StringToHash("Spike");

	[SerializeField] private AnimationClip animationClip;
	[SerializeField] private SceneHelper sceneHelper;
	[SerializeField] private Animator bodyAnimator;

	private Health playerHealth;
	private Transformation playerTransformation;
	private PlayerController playerController;
	private Animator animator;
	private CheckpointManager checkpointManager;

	private float screenDuration = 0f;
	private DamageSource damageSource;

	private void Awake() {
		screenDuration = animationClip.length;

		animator = GetComponent<Animator>();

		GameObject go = GameObject.FindWithTag("Player");
		playerHealth = go.GetComponent<Health>();
		playerTransformation = go.GetComponent<Transformation>();
		playerController = go.GetComponent<PlayerController>();
	}

	private void Start() {
		GameObject go = GameObject.FindWithTag("CheckpointManager");
		if (go != null)
			checkpointManager = go.GetComponent<CheckpointManager>();
	}

	private void OnEnable() {
		if (playerHealth != null)
			playerHealth.OnDie.AddListener(OnDeath);
	}

	private void OnDisable() {
		if (playerHealth != null)
			playerHealth.OnDie.RemoveListener(OnDeath);
	}

	private void OnDeath(DamageSource source) {
		damageSource = source;
		StartCoroutine(CoShowDeath());
	}

	private IEnumerator CoShowDeath() {
		Time.timeScale = 0;
		playerController.AllowControls = false;
		animator.SetTrigger(showHash);

		if (checkpointManager != null) {
			yield return new WaitForSecondsRealtime(screenDuration / 2);
			checkpointManager.Respawn();
			playerTransformation.ResetTransformation();
			Time.timeScale = 1;
			yield return new WaitForSeconds(screenDuration / 2);
			playerController.AllowControls = true;
			yield break;
		}

		yield return new WaitForSecondsRealtime(screenDuration);
		Time.timeScale = 1;
		sceneHelper.ReloadScene();
	}

	// Run by animation event
	public void StartBodyAnimation() {
		bodyAnimator.SetTrigger(restartHash);

		if (playerTransformation.IsHuman)
			bodyAnimator.SetTrigger(humanDeathHash);
		else
			bodyAnimator.SetTrigger(wolfDeathHash);

		switch (damageSource) {
			case DamageSource.Spike:
				bodyAnimator.SetTrigger(spikeHash);
				break;
			default:
				bodyAnimator.SetTrigger(genericHash);
				break;
		}
	}
}
