using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour {
	[SerializeField] private bool isInteractable;
	[SerializeField] private float interactArrowHeight;
	[SerializeField] private UnityEvent<GameObject> onInteract;

	public UnityEvent<GameObject> OnInteract => onInteract;
	public float InteractableArrowHeight => interactArrowHeight;
	public bool IsInteractable {
		get => isInteractable;
		set => isInteractable = value;
	}

	public void Interact(GameObject player) {
		onInteract.Invoke(player);
	}
}
