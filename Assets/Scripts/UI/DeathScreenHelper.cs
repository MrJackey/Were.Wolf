using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeathScreenHelper : MonoBehaviour {
	private static readonly int showHash = Animator.StringToHash("Show");
	private static readonly int isHumanHash = Animator.StringToHash("HumanDeath");
	private static readonly int regularHash = Animator.StringToHash("Regular");
	private static readonly int piercingHash = Animator.StringToHash("Piercing");

	[SerializeField] private AnimationClip animationClip;
	[SerializeField] private SceneHelper sceneHelper;
	[SerializeField] private Animator bodyAnimator;

	private Health playerHealth;
	private Transformation playerTransformation;
	private Animator animator;
	private CheckpointManager checkpointManager;

	private float screenDuration = 0f;

	private void Awake() {
		screenDuration = animationClip.length;

		animator = GetComponent<Animator>();

		GameObject go = GameObject.FindWithTag("Player");
		playerHealth = go.GetComponent<Health>();
		playerTransformation = go.GetComponent<Transformation>();
	}

	private void Start() {
		checkpointManager = FindObjectOfType<CheckpointManager>();
	}

	private void OnEnable() {
		if (playerHealth != null)
			playerHealth.OnDie.AddListener(OnDeath);
	}

	private void OnDisable() {
		if (playerHealth != null)
			playerHealth.OnDie.RemoveListener(OnDeath);
	}

	private void OnDeath(Health.DamageType damageType) {
		StartCoroutine(CoShowDeath(damageType));
	}

	private IEnumerator CoShowDeath(Health.DamageType damageType) {
		Time.timeScale = 0;
		StartCoroutine(CoSelectBodyAnimation(damageType));
		animator.SetTrigger(showHash);

		yield return new WaitForSecondsRealtime(screenDuration / 2);

		if (checkpointManager != null) {
			checkpointManager.Respawn();
			Time.timeScale = 1;
			yield break;
		}

		yield return new WaitForSecondsRealtime(screenDuration / 2);
		Time.timeScale = 1;
		sceneHelper.ReloadScene();
	}

	private IEnumerator CoSelectBodyAnimation(Health.DamageType damageType) {
		print("new body animation");
		while (!bodyAnimator.isInitialized)
			yield return null;

		bodyAnimator.SetTrigger("Restart");

		if (playerTransformation.IsHuman)
			bodyAnimator.SetTrigger("HumanDeath");
		else
			bodyAnimator.SetTrigger("WolfDeath");
		// bodyAnimator.SetBool(isHumanHash, playerTransformation.IsHuman);

		switch (damageType) {
			case Health.DamageType.Piercing:
				bodyAnimator.SetTrigger(piercingHash);
				break;
			default:
				bodyAnimator.SetTrigger(regularHash);
				break;
		}
	}
}
