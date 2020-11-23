using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SignalEmitter : MonoBehaviour {
	[SerializeField, PropertySetter(nameof(IsActivated), PropertySetterMode.Always)] private bool isActivated = false;

	[Header("Events")]
	[SerializeField] private UnityEvent onActivation;
	[SerializeField] private UnityEvent onDeactivation;

	private UnityEvent onActivationChange = new UnityEvent();


	public bool IsActivated {
		get => isActivated;
		set {
			if (value == isActivated)
				return;

			isActivated = value;
			onActivationChange.Invoke();
			if (isActivated)
				onActivation.Invoke();
			else
				onDeactivation.Invoke();
		}
	}

	public UnityEvent OnActivationChange => onActivationChange;
}
