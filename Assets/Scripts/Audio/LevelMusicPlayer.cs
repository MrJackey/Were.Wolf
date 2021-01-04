using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelMusicPlayer : SingletonBehaviour<LevelMusicPlayer> {
	[SerializeField] private SceneHelper sceneHelper;
	[SerializeField] private float fadeDuration = 3f;
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
		AudioClip track = GetTrackForLevel(sceneHelper.CurrentLevel);
		Play(track, true);
	}

	private AudioClip GetTrackForLevel(int levelIndex) {
		if (levelIndex == -1 || levelIndex >= tracks.Length)
			return null;

		return tracks[levelIndex];
	}

	private void Play(AudioClip track, bool allowFade) {
		if (track == null) {
			Stop();
		}
		else if (isPlaying && track == currentTrack) {
			audioSource.volume = 1;
		}
		else {
			if (allowFade) {
				StopAllCoroutines();
				StartCoroutine(CoFadeVolume(1));
			}
			else {
				audioSource.volume = 1;
			}

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

	private IEnumerator CoFadeVolume(int direction) {
		if (fadeDuration > 0) {
			for (float time = 0; time <= fadeDuration; time += Time.deltaTime) {
				float t = Mathf.Clamp01(time / fadeDuration);
				audioSource.volume = direction >= 0 ? t : 1f - t;

				yield return null;
			}
		}

		audioSource.volume = direction >= 0 ? 1 : 0;
	}


	public void OnBeginLevelTransition(int newLevel) {
		if (isPlaying && currentTrack != GetTrackForLevel(newLevel)) {
			StopAllCoroutines();
			StartCoroutine(CoFadeVolume(-1));
			isPlaying = false;
		}
	}
}