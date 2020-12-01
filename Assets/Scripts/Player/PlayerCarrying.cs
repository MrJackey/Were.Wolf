using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrying : MonoBehaviour {
	[SerializeField] private GameObject playerHand;
	private Carryable carriedItem;
	private bool isCarryingItem = false;

	public bool IsCarryingItem => isCarryingItem;

	public void PickUpItem(Carryable item) {
		isCarryingItem = true;
		carriedItem = item;
		carriedItem.OnPickUp(playerHand);
	}

	public void DropItem() {
		carriedItem.OnDrop();
		carriedItem = null;
		isCarryingItem = false;
	}
}
