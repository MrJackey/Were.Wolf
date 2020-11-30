using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour {
	[SerializeField] private UnityEvent<GameObject> onInteract;
	public UnityEvent<GameObject> OnInteract => onInteract;

	public void Interact(GameObject player) {
		onInteract.Invoke(player);
	}
}
