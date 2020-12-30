using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedCall : MonoBehaviour {
	[SerializeField] private int numberOfFrames = 1;
	[SerializeField] private UnityEvent onDelayDone;

	public void StartDelay() => StartCoroutine(CoDelay());

	private IEnumerator CoDelay() {
		for (int i = 0; i < numberOfFrames; i++) {
			yield return null;
		}

		onDelayDone.Invoke();
	}
}
