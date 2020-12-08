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
	[SerializeField] private SceneHelper sceneHelper;

	private int sceneToLoad = -1;
	private float transitionDuration = 0f;
	private bool isFadingIn = false;
	private float baseVolume;
	private bool doFadeAudio = false;
	private AsyncOperation loadedScene;

	public int SceneToLoad { set => sceneToLoad = value; }
	public bool IsEntry { set => isFadingIn = value; }
	public bool DoFadeAudio { set => doFadeAudio = value; }

	private void Start() {
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
		if (isFadingIn)
			animator.SetTrigger(entryHash);

		for (float time = 0f; time < transitionDuration; time += Time.unscaledDeltaTime) {
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
			sceneHelper.IsTransitioning = false;
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
