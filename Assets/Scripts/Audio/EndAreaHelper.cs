using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAreaHelper : MonoBehaviour {
	private FadeVolumeInOut musicFade;
	private AudioSource musicAudioSource;
	private bool isEntered;
	private float baseVolume;

	private void Start() {
		GameObject go = GameObject.FindWithTag("LevelMusicPlayer");
		if (go != null) {
			musicFade = go.GetComponent<FadeVolumeInOut>();
			musicAudioSource = go.GetComponent<AudioSource>();
		}
	}

	private void OnDisable() {
		if (!isEntered || musicAudioSource == null) return;

		musicAudioSource.volume = baseVolume;
		musicAudioSource.Play();
	}

	public void HandlePlayerEntry() {
		if (musicFade == null || musicAudioSource == null || isEntered) return;

		baseVolume = musicAudioSource.volume;
		musicFade.StopWithFade();
		isEntered = true;
	}
}
