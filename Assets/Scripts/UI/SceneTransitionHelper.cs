using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour {
	private static readonly int entryHash = Animator.StringToHash("Entry");

	[SerializeField] private Animator animator;
	[SerializeField] private AnimationClip transitionClip;
	[SerializeField] private AudioMixer mixer;

	private int sceneToLoad = -1;
	private float transitionDuration = 0f;
	private bool isFadingIn = false;
	private float baseVolume;
	private bool doFadeAudio = false;
	private AsyncOperation loadOperation;

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

		StartCoroutine(CoTransition());
	}

	private IEnumerator CoTransition() {
		if (isFadingIn)
			animator.SetTrigger(entryHash);

		for (float time = 0f; time < transitionDuration; time += Time.unscaledDeltaTime) {
			if (doFadeAudio)
				FadeAudio(time);

			yield return null;
		}

		if (!isFadingIn) {
			// Wait for scene to be ready
			while (loadOperation.progress < 0.9f)
				yield return null;

			isFadingIn = true;
			loadOperation.allowSceneActivation = true;
			StartCoroutine(CoTransition());
		}
		else {
			CompletedCallback?.Invoke();
			Destroy(gameObject);
		}
	}

	private void FadeAudio(float time) {
		float newVolume;
		if (isFadingIn)
			newVolume = Mathf.Lerp(0, baseVolume, time / transitionDuration);
		else
			newVolume = Mathf.Lerp(baseVolume, 0, time / transitionDuration);

		mixer.SetFloat("masterVolume", MathX.LinearToDecibels(newVolume));
	}
}
