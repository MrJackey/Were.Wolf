using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelMusicPlayer : SingletonBehaviour<LevelMusicPlayer> {
	[SerializeField] private SceneHelper sceneHelper;
	[SerializeField] private AudioClip[] tracks;

	private AudioSource audioSource;
	private AudioClip currentTrack;
	private bool isPlaying;
	private bool isPaused;

	public bool IsPaused {
		get => isPaused;
		set {
			if (isPaused == value) return;
			isPaused = value;

			if (isPaused) {
				if (isPlaying)
					audioSource.Pause();
			}
			else {
				if (isPlaying)
					audioSource.UnPause();
			}
		}
	}

	protected override void Awake() {
		base.Awake();
		audioSource = GetComponent<AudioSource>();
	}

	private void Start() {
		OnSceneLoaded();
	}

	private void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		OnSceneLoaded();
	}

	private void OnSceneLoaded() {
		int currentLevelIndex = sceneHelper.CurrentLevel;

		if (currentLevelIndex == -1)
			Stop();
		else if (currentLevelIndex < tracks.Length)
			Play(tracks[currentLevelIndex]);
		else
			Stop();
	}

	private void Play(AudioClip track) {
		if (track == null) {
			Stop();
		}
		else if (track == currentTrack) {
			if (!isPlaying)
				Play();
		}
		else {
			SetTrack(track);
			Play();
		}
	}

	private void Play() {
		if (!isPaused)
			audioSource.Play();

		isPlaying = true;
	}

	private void Stop() {
		audioSource.Stop();
		isPlaying = false;
	}

	private void SetTrack(AudioClip track) {
		currentTrack = audioSource.clip = track;
	}
}