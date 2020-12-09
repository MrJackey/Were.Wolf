using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour {
	private static readonly int entryHash = Animator.StringToHash("Entry");

	[SerializeField] private Animator animator = null;
	[SerializeField] private AnimationClip transitionClip = null;
	[SerializeField] private AudioMixer mixer = null;

	private int sceneToLoad = -1;
	private float transitionDuration = 0f;
	private float baseVolume = 0f;
	private bool doFadeAudio = false;
	private AsyncOperation loadOperation = null;

	public int SceneToLoad { set => sceneToLoad = value; }
	public bool DoFadeAudio { set => doFadeAudio = value; }
	public Action CompletedCallback { private get; set; }

	private void Start() {
		if (sceneToLoad == -1) return;
		transitionDuration = transitionClip.length;

		loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
		loadOperation.allowSceneActivation = false;

		mixer.GetFloat("masterVolume", out baseVolume);
		baseVolume = MathX.DecibelsToLinear(baseVolume);

		StartCoroutine(CoExit());
	}

	private IEnumerator CoExit() {
		for (float time = 0f; time < transitionDuration; time += Time.unscaledDeltaTime) {
			if (doFadeAudio)
				FadeAudio(baseVolume, 0, time);

			yield return null;
		}

		// Wait for scene to be ready
		while (loadOperation.progress < 0.9f)
			yield return null;

		loadOperation.allowSceneActivation = true;
		StartCoroutine(CoEnter());
	}

	private IEnumerator CoEnter() {
		animator.SetTrigger(entryHash);

		for (float time = 0f; time < transitionDuration; time += Time.unscaledDeltaTime) {
			if (doFadeAudio)
				FadeAudio(0, baseVolume, time);

			yield return null;
		}

		CompletedCallback?.Invoke();
		Destroy(gameObject);
	}

	private void FadeAudio(float from, float to, float time) {
		float newVolume = Mathf.Lerp(from, to, time / transitionDuration);

		mixer.SetFloat("masterVolume", MathX.LinearToDecibels(newVolume));
	}
}
