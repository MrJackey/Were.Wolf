using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Scene Helper", menuName = "Game/Scene Helper")]
public class SceneHelper : ScriptableObject {
	[SerializeField] private SceneReference menuScene;
	[SerializeField] private SceneReference endScene;
	[SerializeField] private SceneReference[] levels;

	/// <summary>
	/// Returns the index of the currently loaded level or -1 if the loaded scene is not in the list of levels.
	/// </summary>
	public int CurrentLevel {
		get {
			int current = SceneManager.GetActiveScene().buildIndex;
			return FindLevelByBuildIndex(current);
		}
	}

	public SceneReference MenuScene => menuScene;
	public SceneReference[] Levels => levels;
	public SceneReference EndScene => endScene;

	public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

	public void LoadScene(int buildIndex) {
		SceneManager.LoadScene(buildIndex);
	}

	public void LoadScene(SceneReference scene) {
		SceneManager.LoadScene(scene);
	}

	public void ReloadScene() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	/// <summary>
	/// Load the previous level if there is one.
	/// </summary>
	public void LoadPreviousLevel() {
		if (levels.Length < 2) return;

		int levelIndex = CurrentLevel;
		if (levelIndex <= 0) return;

		LoadScene(levels[levelIndex - 1]);
	}

	/// <summary>
	/// Load the next level. Loads the end scene if the current level is the last one, and loads the first level if
	/// the current scene is not in the list of levels.
	/// </summary>
	public void LoadNextLevel() {
		if (levels.Length == 0) return;

		int levelIndex = CurrentLevel;
		if (levelIndex == -1)
			LoadScene(levels[0]);
		else if (levelIndex == levels.Length - 1)
			LoadScene(endScene);
		else
			LoadScene(levels[levelIndex + 1]);
	}

	/// <summary>
	/// Load a level by index if it exists.
	/// </summary>
	public void LoadLevel(int levelIndex) {
		if (levelIndex < 0) throw new ArgumentOutOfRangeException(nameof(levelIndex));
		if (levelIndex >= levels.Length) return;

		LoadScene(levels[levelIndex]);
	}

	private int FindLevelByBuildIndex(int buildIndex) {
		foreach (SceneReference scene in levels)
			if (scene.BuildIndex == buildIndex)
				return scene.BuildIndex;

		return -1;
	}
}