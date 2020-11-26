using UnityEngine;
using UnityEngine.Events;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private SceneHelper sceneHelper = null;
	[SerializeField] private UnityEvent onPause = null;
	[SerializeField] private UnityEvent onResume = null;

	private bool isPaused;

	private void Update() {
		if (Input.GetButtonDown("Pause")) {
			if (isPaused)
				Resume();
			else
				Pause();
		}
	}

	public void Pause() {
		if (isPaused) return;
		isPaused = true;
		Time.timeScale = 0;
		onPause.Invoke();
	}

	public void Resume() {
		if (!isPaused) return;
		isPaused = false;
		Time.timeScale = 1;
		onResume.Invoke();
	}

	public void ExitToMenu() {
		Time.timeScale = 1;
		sceneHelper.LoadScene(sceneHelper.MenuScene);
	}
}