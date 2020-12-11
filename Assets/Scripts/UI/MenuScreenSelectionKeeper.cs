using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuScreenSelectionKeeper : MonoBehaviour {
	[SerializeField] private Selectable firstSelected = null;
	[SerializeField] private Selectable fallbackSelected;
	[SerializeField] private bool rememberSelection = true;
	[SerializeField] private bool tryMakeSelectionVisible = false;

	private Selectable currentSelected;

	private void OnEnable() {
		EventSystem eventSystem = EventSystem.current;
		Debug.Assert(eventSystem != null, "eventSystem != null");

		if (!rememberSelection || currentSelected == null) {
			currentSelected = firstSelected;

			if (currentSelected == null || !currentSelected.interactable)
				currentSelected = fallbackSelected;
		}

		eventSystem.SetSelectedGameObject(currentSelected != null ? currentSelected.gameObject : null);

		if (tryMakeSelectionVisible && eventSystem.currentSelectedGameObject != null)
			StartCoroutine(CoNudgeSelection(eventSystem.currentSelectedGameObject));
	}

	private void OnDisable() {
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem != null) {
			GameObject selectedGameObject = eventSystem.currentSelectedGameObject;
			if (selectedGameObject != null)
				currentSelected = selectedGameObject.GetComponent<Selectable>();
		}
	}

	private static IEnumerator CoNudgeSelection(GameObject selection) {
		// Wait for the next frame to let things initialize.
		yield return null;

		// HACK: If you know of a better way to make the selection visible, please change this.
		MoveSelection(MoveDirection.Up);
		MoveSelection(MoveDirection.Down);
		MoveSelection(MoveDirection.Left);
		MoveSelection(MoveDirection.Right);
		EventSystem.current.SetSelectedGameObject(selection);
	}

	private static void MoveSelection(MoveDirection dir) {
		EventSystem eventSystem = EventSystem.current;
		var data = new AxisEventData(eventSystem) {
			moveDir = dir,
			selectedObject = eventSystem.currentSelectedGameObject,
		};

		ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
	}
}