using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Scene Helper", menuName = "Game/Scene Helper")]
public class SceneHelper : ScriptableObject {
	public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
	public void LoadScene(int buildIndex) => SceneManager.LoadScene(buildIndex);
	public void LoadScene(SceneReference scene) => SceneManager.LoadScene(scene);

	public void ReloadScene() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}