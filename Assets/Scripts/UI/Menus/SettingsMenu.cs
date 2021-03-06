﻿using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
	[SerializeField] private SaveHelper saveHelper;
	[SerializeField] private AudioMixer mixer;
	[SerializeField] private Slider masterSlider;
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;
	[SerializeField] private Toggle fullscreenToggle;

	private void OnEnable() {
		SaveManager.SettingsData settings = saveHelper.Settings;
		masterSlider.value = settings.masterVolume;
		musicSlider.value = settings.musicVolume;
		sfxSlider.value = settings.sfxVolume;

		masterSlider.onValueChanged.AddListener(OnMasterValueChanged);
		musicSlider.onValueChanged.AddListener(OnMusicValueChanged);
		sfxSlider.onValueChanged.AddListener(OnSFXValueChanged);

		fullscreenToggle.isOn = Screen.fullScreen;
		fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
	}

	private void OnDisable() {
		saveHelper.Save();

		masterSlider.onValueChanged.RemoveListener(OnMasterValueChanged);
		musicSlider.onValueChanged.RemoveListener(OnMusicValueChanged);
		sfxSlider.onValueChanged.RemoveListener(OnSFXValueChanged);

		fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
	}

	private void OnMasterValueChanged(float value) {
		saveHelper.Settings.masterVolume = value;
		mixer.SetFloat("masterVolume", MathX.LinearToDecibels(value));
	}

	private void OnMusicValueChanged(float value) {
		saveHelper.Settings.musicVolume = value;
		mixer.SetFloat("musicVolume", MathX.LinearToDecibels(value));
	}

	private void OnSFXValueChanged(float value) {
		saveHelper.Settings.sfxVolume = value;
		mixer.SetFloat("sfxVolume", MathX.LinearToDecibels(value));
	}

	private void OnFullscreenChanged(bool value) {
		if (value) {
			Resolution resolution = Screen.currentResolution;
			Screen.SetResolution(resolution.width, resolution.height, true);
		}
		else {
			Screen.SetResolution(1280, 720, false);
		}
	}
}