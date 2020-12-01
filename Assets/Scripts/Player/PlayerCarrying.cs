using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCarrying : MonoBehaviour {
	[SerializeField] private GameObject playerHand;
	[SerializeField] private UnityEvent onPickUp;
	[SerializeField] private UnityEvent onDrop;

	private Carryable carriedItem;
	private bool isCarryingItem = false;

	public bool IsCarryingItem => isCarryingItem;

	public void PickUpItem(Carryable item) {
		isCarryingItem = true;
		carriedItem = item;
		carriedItem.OnPickUp(playerHand);
		onPickUp.Invoke();
	}

	public void DropItem() {
		if (!isCarryingItem) return;

		carriedItem.OnDrop();
		carriedItem = null;
		isCarryingItem = false;
		onDrop.Invoke();
	}
}
