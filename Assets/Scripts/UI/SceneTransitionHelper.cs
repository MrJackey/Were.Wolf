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
	private static bool isTransitioning = false;
	private float transitionDuration = 0f;
	private bool isFadingIn = false;
	private float baseVolume;
	private bool doFadeAudio = false;
	private AsyncOperation loadedScene;

	public int SceneToLoad { set => sceneToLoad = value; }
	public bool IsEntry { set => isFadingIn = value; }
	public bool DoFadeAudio { set => doFadeAudio = value; }

	private void Start() {
		if (isTransitioning) return;
		transitionDuration = transitionClip.length;

		if (sceneToLoad != -1) {
			loadedScene = SceneManager.LoadSceneAsync(sceneToLoad);
			loadedScene.allowSceneActivation = false;
		}
		mixer.GetFloat("masterVolume", out baseVolume);
		baseVolume = MathX.DecibelsToLinear(baseVolume);

		StartCoroutine(CoTransition());
	}

	private IEnumerator CoTransition() {
		isTransitioning = true;

		if (isFadingIn)
			animator.SetTrigger(entryHash);

		for (float time = 0f; time < transitionDuration; time += Time.deltaTime) {
			if (doFadeAudio)
				FadeAudio(time);

			yield return null;
		}

		if (!isFadingIn) {
			// Wait for scene to be ready
			while (loadedScene.progress < 0.9f) {
				yield return null;
			}

			isFadingIn = true;
			loadedScene.allowSceneActivation = true;
			StartCoroutine(CoTransition());
		}
		else {
			isTransitioning = false;
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
