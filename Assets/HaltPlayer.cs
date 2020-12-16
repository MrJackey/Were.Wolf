﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltPlayer : MonoBehaviour {
	[SerializeField] private BoxCollider2D hasMetTrigger;

	private PlayerController playerController;
	private bool hasMetOnce = false;

	private void Start() {
		playerController = GameObject.FindWithTag("Player").GetComponentUnlessNull<PlayerController>();
		hasMetOnce = false;
		hasMetTrigger.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger && !hasMetOnce)
			DisableControls();
	}

	private void DisableControls() {
		if (playerController == null)
			return;

		playerController.AllowControls = false;
			
		hasMetTrigger.enabled = true;
		hasMetOnce = true;
	}

	public void EnableControls() {
		if (playerController == null) 
			return;
			
		playerController.AllowControls = true;
	}
}
