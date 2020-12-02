using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelMusicPlayer : SingletonBehaviour<LevelMusicPlayer> {
	[SerializeField] private SceneHelper sceneHelper;

	private AudioSource audioSource;
	private bool isPlaying;

	protected override void Awake() {
		base.Awake();
		audioSource = GetComponent<AudioSource>();
	}

	private void Start() {
		if (sceneHelper.CurrentLevel != -1)
			Play();
	}

	private void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (sceneHelper.CurrentLevel != -1)
			Play();
		else
			Stop();
	}

	public void Play() {
		if (isPlaying) return;
		audioSource.Play();
		isPlaying = true;
	}

	public void Stop() {
		audioSource.Stop();
		isPlaying = false;
	}
}