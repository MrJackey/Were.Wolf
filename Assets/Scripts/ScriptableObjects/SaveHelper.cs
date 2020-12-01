using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Save Helper", menuName = "Game/Save Helper", order = 0)]
public class SaveHelper : ScriptableObject {
	[SerializeField] private SceneHelper sceneHelper;

	public void MarkCurrentLevelCompleted() {
		if (sceneHelper.CurrentLevel != -1)
			MarkLevelCompleted(sceneHelper.CurrentLevel);
	}

	public void MarkLevelCompleted(int levelIndex) {
		SaveManager.Instance.Data.currentLevel = Mathf.Max(SaveManager.Instance.Data.currentLevel, levelIndex);
		SaveManager.Instance.Save();
	}
}