using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Save Helper", menuName = "Game/Save Helper", order = 0)]
public class SaveHelper : ScriptableObject {
	[SerializeField] private SceneHelper sceneHelper;

	public SaveManager.SaveData Data => SaveManager.Instance.Data;

	public int CompletedLevel {
		get => Data.completedLevel;
		set => Data.completedLevel = value;
	}

	public void MarkCurrentLevelCompleted() {
		if (sceneHelper.CurrentLevel != -1)
			MarkLevelCompleted(sceneHelper.CurrentLevel);
	}

	public void MarkLevelCompleted(int levelIndex) {
		Data.completedLevel = Mathf.Max(Data.completedLevel, levelIndex);
		Save();
	}

	public void LoadContinueLevel() {
		int level = CompletedLevel + 1;
		if (level >= sceneHelper.Levels.Length)
			level = sceneHelper.Levels.Length - 1;

		sceneHelper.LoadLevelWithTransition(level);
	}

	public void Load() {
		SaveManager.Instance.Load();
	}

	public void Save() {
		SaveManager.Instance.Save();
	}

	public void ClearData() {
		SaveManager.Instance.ClearData();
	}
}
