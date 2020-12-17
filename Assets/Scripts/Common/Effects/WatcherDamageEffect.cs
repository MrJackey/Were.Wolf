using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WatcherDamageEffect : MonoBehaviour {
	[SerializeField] private Canvas vignetteCanvasPrefab;
	[SerializeField] private float fadeOutDuration = 1f;

	private Watcher watcher;
	private Color vignetteColor;

	private static GameObject canvas;
	private static RawImage image;
	private static Material vignetteMaterial;

	private void Start() {
		watcher = GetComponent<Watcher>();

		if (canvas == null) {
			canvas = Instantiate(vignetteCanvasPrefab).gameObject;
			image = canvas.GetComponentInChildren<RawImage>();
			vignetteMaterial = image.material = new Material(image.material);
		}
		vignetteColor = vignetteMaterial.color;

		canvas.SetActive(false);
	}

	private IEnumerator CoFadeInVignette(float duration) {
		canvas.SetActive(true);

		for (float time = 0; time < duration; time += Time.deltaTime) {
			SetVignetteAlpha(MathX.EaseOutQuad(0, vignetteColor.a, time / duration));
			yield return null;
		}
	}

	private IEnumerator CoFadeOutVignette(float duration) {
		for (float time = 0; time < duration; time += Time.deltaTime) {
			SetVignetteAlpha(MathX.EaseInQuad(vignetteColor.a, 0, time / duration));
			yield return null;
		}

		canvas.SetActive(false);
	}

	private void SetVignetteAlpha(float alpha) {
		vignetteMaterial.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, alpha);
	}


	public void ShowVignette() {
		StopAllCoroutines();
		StartCoroutine(CoFadeInVignette(watcher.DamageTime));
	}

	public void HideVignette() {
		StopAllCoroutines();
		StartCoroutine(CoFadeOutVignette(fadeOutDuration));
	}
}