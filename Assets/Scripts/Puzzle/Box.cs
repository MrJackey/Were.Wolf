using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

	public BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;
	private Vector2 startPos;
	private Health health;

	private void Start() {
		startPos = transform.position;
		rb2D = GetComponent<Rigidbody2D>();
		health = GetComponent<Health>();
	}

	public void Respawn() {
		health.RestoreHealth();
		transform.position = startPos;
		transform.rotation = Quaternion.identity;
		rb2D.velocity = Vector2.zero;
	}
}