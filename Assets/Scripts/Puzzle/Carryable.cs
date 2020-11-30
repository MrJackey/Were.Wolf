using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Carryable : MonoBehaviour {
	[SerializeField] private UnityEvent<GameObject> onPickup;
	[SerializeField] private UnityEvent onDrop;


	public virtual void OnPickUp(GameObject playerHand) {
		transform.parent = playerHand.transform;
		transform.localPosition = Vector3.zero;
		onPickup.Invoke(playerHand);
	}
	
	public void OnDrop() {
		transform.parent = null;
		onDrop.Invoke();
	} 
}
