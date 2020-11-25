using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuScreenSelectionKeeper : MonoBehaviour {
	[SerializeField] private GameObject firstSelected;

	private GameObject currentSelected;

	private void OnEnable() {
		EventSystem.current.SetSelectedGameObject(currentSelected == null ? firstSelected : currentSelected);
	}

	private void OnDisable() {
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem != null)
			currentSelected = eventSystem.currentSelectedGameObject;
	}
}