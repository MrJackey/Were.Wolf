using System;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSettingsLoader : MonoBehaviour {
	[SerializeField] private SaveHelper saveHelper;
	[SerializeField] private AudioMixer mixer;

	private void Start() {
		mixer.SetFloat("masterVolume", MathX.LinearToDecibels(saveHelper.Settings.masterVolume));
		mixer.SetFloat("musicVolume", MathX.LinearToDecibels(saveHelper.Settings.musicVolume));
		mixer.SetFloat("sfxVolume", MathX.LinearToDecibels(saveHelper.Settings.sfxVolume));
	}
}