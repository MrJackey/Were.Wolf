﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

	[SerializeField] private BoxCollider2D boxCollider;
	private BoxCollider2D boxTrigger;
	private Rigidbody2D rb2D;
	private Vector2 startPos;
	private Health health;
	private Carryable carryable;
	private Interactable interactable;

	private void Awake() {
		startPos = transform.position;
		rb2D = GetComponent<Rigidbody2D>();
		health = GetComponent<Health>();
		boxTrigger = GetComponent<BoxCollider2D>();
		carryable = GetComponent<Carryable>();
		interactable = GetComponent<Interactable>();
		rb2D.isKinematic = true;
	}

	private void OnEnable() {
		interactable.OnInteract.AddListener(Interact);
	}

	private void OnDisable() {
		interactable.OnInteract.RemoveListener(Interact);
	}

	public void Respawn() {
		health.RestoreHealth();
		transform.position = startPos;
		transform.rotation = Quaternion.identity;
		rb2D.velocity = Vector2.zero;
	}

	public void OnPickUp() {
		boxCollider.enabled = false;
		boxTrigger.enabled = false;

	}

	public void OnDrop() {
		rb2D.isKinematic = false;
		boxCollider.enabled = true;
		boxTrigger.enabled = true;
	}

	private void OnCollisionEnter2D(Collision2D other) {
		rb2D.isKinematic = true;
	}

	private void Interact(GameObject player) {
		player.GetComponent<PlayerCarrying>().PickUpItem(carryable);
	}
}