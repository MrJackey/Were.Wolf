using UnityEngine;
using UnityEngine.UI;

public class LockLevelButtons : MonoBehaviour {
	[SerializeField] private SaveHelper saveHelper;
	[SerializeField] private Button[] buttons;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[SerializeField] private bool onlyLockInRelease = true;
#endif

	private void OnEnable() {
	#if UNITY_EDITOR || DEVELOPMENT_BUILD
		if (onlyLockInRelease) return;
	#endif

		int maxLevel = saveHelper.CompletedLevel + 1;
		for (int i = 0; i < buttons.Length; i++)
			buttons[i].interactable = i <= maxLevel;
	}
}