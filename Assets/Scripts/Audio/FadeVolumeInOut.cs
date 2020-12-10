using System;
using System.Collections;
using UnityEngine;

public class FadeVolumeInOut : MonoBehaviour {
	[SerializeField] private float fadeDuration;

	private AudioSource audioSource;
	float baseVolume;

	private void Awake() {
		audioSource = GetComponent<AudioSource>();
		baseVolume = audioSource.volume;
	}

	public void Play() {
		StopAllCoroutines();
		audioSource.volume = baseVolume;
		audioSource.Play();
	}

	public void Stop() {
		StopAllCoroutines();
		audioSource.volume = baseVolume;
		audioSource.Stop();
	}

	public void PlayWithFade() {
		audioSource.volume = 0;
		audioSource.Play();

		StopAllCoroutines();

		if (audioSource.loop)
			StartCoroutine(CoFadeVolume(0, baseVolume, baseVolume, null));
		else
			StartCoroutine(CoFadeInNoLoop());
	}

	public void StopWithFade() {
		StopAllCoroutines();
		StartCoroutine(CoFadeVolume(audioSource.volume, 0, baseVolume, () => audioSource.Stop()));
	}

	private IEnumerator CoFadeVolume(float from, float to, float finalVolume, Action whenDone) {
		for (float time = 0; time < fadeDuration; time += Time.deltaTime) {
			audioSource.volume = Mathf.Lerp(from, to, time / fadeDuration);
			yield return null;
		}

		whenDone?.Invoke();
		audioSource.volume = finalVolume;
	}

	private IEnumerator CoFadeInNoLoop() {
		StartCoroutine(CoFadeVolume(0, baseVolume, baseVolume, null));

		yield return new WaitForSecondsRealtime(audioSource.clip.length - fadeDuration);

		StopAllCoroutines();
		StartCoroutine(CoFadeVolume(baseVolume, 0, baseVolume, null));
	}
}