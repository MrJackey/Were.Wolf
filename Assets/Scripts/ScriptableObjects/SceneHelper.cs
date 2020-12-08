using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Scene Helper", menuName = "Game/Scene Helper")]
public class SceneHelper : ScriptableObject {
	[SerializeField] private SceneReference menuScene;
	[SerializeField] private SceneReference endScene;
	[SerializeField] private GameObject transition;
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

	public void LoadSceneWithTransition(string sceneName) {
		DoExitTransition(SceneManager.GetSceneByName(sceneName).buildIndex, true);
	}

	public void LoadSceneWithTransition(int buildIndex, bool fadeAudio = true) {
		DoExitTransition(buildIndex, fadeAudio);
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
	public void LoadNextLevel(bool doTransition = false) {
		if (levels.Length == 0) return;

		int levelIndex = CurrentLevel;
		SceneReference sceneToLoad;

		if (levelIndex == -1)
			sceneToLoad = levels[0];
		else if (levelIndex == levels.Length - 1)
			sceneToLoad = endScene;
		else
			sceneToLoad = levels[levelIndex + 1];

		if (doTransition)
			LoadSceneWithTransition(sceneToLoad, false);
		else
			LoadScene(sceneToLoad);
	}

	/// <summary>
	/// Load a level by index if it exists.
	/// </summary>
	public void LoadLevel(int levelIndex) {
		if (levelIndex < 0) throw new ArgumentOutOfRangeException(nameof(levelIndex));
		if (levelIndex >= levels.Length) return;

		LoadScene(levels[levelIndex]);
	}

	public void LoadLevelWithTransition(int levelIndex) {
		if (levelIndex < 0) throw new ArgumentOutOfRangeException(nameof(levelIndex));
		if (levelIndex >= levels.Length) return;

		LoadSceneWithTransition(levels[levelIndex]);
	}

	private int FindLevelByBuildIndex(int buildIndex) {
		for (int i = 0; i < levels.Length; i++) {
			if (levels[i].BuildIndex == buildIndex)
				return i;
		}

		return -1;
	}

	private void DoExitTransition(int buildIndex, bool fadeAudio = false) {
		GameObject go = Instantiate(transition);
		SceneTransitionHelper helper = go.GetComponent<SceneTransitionHelper>();
		helper.SceneToLoad = buildIndex;
		helper.DoFadeAudio = fadeAudio;
	}
}
