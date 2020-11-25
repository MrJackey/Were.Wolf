using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExitGate : Gate {
	[Space]
	[SerializeField] private UnityEvent onLevelComplete;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.attachedRigidbody.CompareTag("Player") && !other.isTrigger) {
			onLevelComplete.Invoke();
		}
	}
}
