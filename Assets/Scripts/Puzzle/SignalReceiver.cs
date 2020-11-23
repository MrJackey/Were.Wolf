using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalReceiver : MonoBehaviour {
	[SerializeField] private LogicGate logicGate = LogicGate.AND;
	[SerializeField] private bool invert = false;
	[SerializeField] private SignalEmitter[] emitters;

	private bool isActivated = false;

	public bool IsActivated {
		get => isActivated;
		set => isActivated = value;
	}

	private void Start() {
		foreach (SignalEmitter emitter in emitters) {
			emitter.OnActivationChange.AddListener(EmitterUpdate);
		}
	}

#if UNITY_EDITOR
	private void OnValidate() {
		EmitterUpdate();
	}
#endif

	private void EmitterUpdate() {
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
	}

	// All emitters must be active
	private bool CheckAND() {
		foreach (SignalEmitter emitter in emitters) {
			if (!emitter.IsActivated)
				return false;
		}

		return true;
	}

	// One or more emitters must be active
	private bool CheckOR() {
		foreach (SignalEmitter emitter in emitters) {
			if (emitter.IsActivated)
				return true;
		}

		return false;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		foreach (SignalEmitter emitter in emitters) {
			Gizmos.color = emitter.IsActivated ? Color.green : Color.red;
			Gizmos.DrawLine(emitter.transform.position, transform.position);
		}

		Gizmos.color = isActivated ? Color.green : Color.red;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
	}
#endif
}

public enum LogicGate {
	AND,
	OR,
}
