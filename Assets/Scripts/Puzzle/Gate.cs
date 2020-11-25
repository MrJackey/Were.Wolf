using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : SignalReceiver {
	private static readonly int openHash = Animator.StringToHash("open");
	private static readonly int closeHash = Animator.StringToHash("close");

	[SerializeField] private bool isOpen;
	[SerializeField] private Animator animator;
	[SerializeField] private bool panCamera;

	private void Awake() {
		if (isOpen)
			Open();
	}

	public void Open() {
		isOpen = true;
		animator.SetTrigger(openHash);
	}

	public void Close() {
		isOpen = false;
		animator.SetTrigger(closeHash);
	}
}
