using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAreaHelper : MonoBehaviour {
	private LevelMusicPlayer musicPlayer;
	private FadeVolumeInOut musicFade;
	private bool isEntered;
	private float baseVolume;

	private void Start() {
		musicPlayer = LevelMusicPlayer.Instance;
		if (musicPlayer != null)
			musicFade = musicPlayer.gameObject.GetComponent<FadeVolumeInOut>();
	}

	private void OnDisable() {
		if (!isEntered || musicPlayer == null) return;

		musicPlayer.AudioSource.volume = baseVolume;
		musicPlayer.AudioSource.Play();
	}

	public void HandlePlayerEntry() {
		if (musicFade == null || musicPlayer == null || isEntered) return;

		baseVolume = musicPlayer.AudioSource.volume;
		musicFade.StopWithFade();
		isEntered = true;
	}
}
