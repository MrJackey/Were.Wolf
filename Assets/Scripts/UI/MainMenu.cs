using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
	[SerializeField] private SaveHelper saveHelper;
	[SerializeField] private Button continueButton;

	private void OnEnable() {
		Cursor.visible = true;
		continueButton.interactable = saveHelper.Data.completedLevel >= 0;
	}

	private void OnDisable() {
		Cursor.visible = false;
	}

	public void Exit() {
	#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode();
	#else
		Application.Quit();
	#endif
	}
}