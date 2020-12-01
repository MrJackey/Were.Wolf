using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuScreenSelectionKeeper : MonoBehaviour {
	[SerializeField] private GameObject firstSelected = null;
	[SerializeField] private bool rememberSelection = true;
	[SerializeField] private bool tryMakeSelectionVisible = false;

	private GameObject currentSelected;

	private void OnEnable() {
		EventSystem eventSystem = EventSystem.current;
		eventSystem.SetSelectedGameObject(!rememberSelection || currentSelected == null ? firstSelected : currentSelected);

		if (tryMakeSelectionVisible && eventSystem.currentSelectedGameObject != null)
			StartCoroutine(CoNudgeSelection(eventSystem.currentSelectedGameObject));
	}

	private void OnDisable() {
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem != null)
			currentSelected = eventSystem.currentSelectedGameObject;
	}

	private IEnumerator CoNudgeSelection(GameObject selection) {
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