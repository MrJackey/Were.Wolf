using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeathScreenHelper : MonoBehaviour {
	private static readonly int showHash = Animator.StringToHash("Show");

	[SerializeField] private AnimationClip animationClip;
	[SerializeField] private SceneHelper sceneHelper;

	private Health playerHealth;
	private Animator animator;
	private CheckpointManager checkpointManager;

	private float screenDuration = 0f;

	private void Awake() {
		screenDuration = animationClip.length;

		animator = GetComponent<Animator>();

		GameObject go = GameObject.FindWithTag("Player");
		playerHealth = go.GetComponent<Health>();
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

	private void OnDeath() {
		StartCoroutine(CoShowDeath());
	}

	private IEnumerator CoShowDeath() {
		Time.timeScale = 0;
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
}
