using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class SignalReceiver : MonoBehaviour {
	[SerializeField] private bool isActivated;
	[SerializeField] private LogicGate logicGate = LogicGate.AND;
	[SerializeField] private bool invert = false;
	[SerializeField] private SignalEmitter[] emitters;

	[Header("Events")]
	[SerializeField] protected UnityEvent onActivation;
	[SerializeField] protected UnityEvent onDeactivation;

	private bool isInitialized = false;

	public bool IsActivated {
		get => isActivated;
		set => isActivated = value;
	}

	private void Start() {
		foreach (SignalEmitter emitter in emitters) {
			if (emitter == null)
				continue;
			emitter.OnActivationChange.AddListener(EmitterUpdate);
		}

		if (isInitialized) return;

		if (isActivated)
			onActivation.Invoke();
		else
			onDeactivation.Invoke();

		isInitialized = true;
	}

#if UNITY_EDITOR
	private void OnValidate() {
		if (isActivated)
			onActivation.Invoke();
		else
			onDeactivation.Invoke();

		isInitialized = true;
	}
#endif

	private void EmitterUpdate() {
		bool oldState = isActivated;
		switch (logicGate) {
			case (LogicGate.AND) :
				isActivated = CheckAND();
				break;
			case (LogicGate.OR) :
				isActivated = CheckOR();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (invert)
			isActivated = !isActivated;

		if (oldState == isActivated)
			return;

		if (isActivated)
			onActivation.Invoke();
		else
			onDeactivation.Invoke();
	}

	// All emitters must be active
	private bool CheckAND() {
		foreach (SignalEmitter emitter in emitters) {
			if (emitter == null)
				continue;
			if (!emitter.IsActivated)
				return false;
		}

		return true;
	}

	// One or more emitters must be active
	private bool CheckOR() {
		foreach (SignalEmitter emitter in emitters) {
			if (emitter == null)
				continue;
			if (emitter.IsActivated)
				return true;
		}

		return false;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		foreach (SignalEmitter emitter in emitters) {
			if (emitter == null)
				continue;
			Gizmos.color = emitter.IsActivated ? Color.green : Color.red;
			Gizmos.DrawLine(emitter.transform.position, transform.position);
		}
	}
#endif
}

public enum LogicGate {
	AND,
	OR,
}
