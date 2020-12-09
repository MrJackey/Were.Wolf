using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteHighlight : MonoBehaviour {
	private static readonly int centerID = Shader.PropertyToID("_Center");

	[SerializeField] private RawImage rawImage;
	[SerializeField] private float fadeDuration = 1f;

	private new Camera camera = null;
	private Transform worldTarget = null;
	private Material material = null;
	private float baseAlpha = 0f;
	private float alpha = 0f;
	private float fadeTime = 0f;
	private int fadeDirection = 1;

	public Transform WorldTarget { set => worldTarget = value; }

	private void Start() {
		Canvas canvas = GetComponent<Canvas>();

		if (canvas == null) {
			Debug.LogError($"{nameof(VignetteHighlight)} requires a canvas");
			return;
		}

		camera = canvas.worldCamera;
		material = new Material(rawImage.material);
		rawImage.material = material;
		baseAlpha = material.color.a;
	}

	private void LateUpdate() {
		Vector4 viewportTarget = GetViewportTarget();
		fadeTime = Mathf.Min(fadeTime += Time.unscaledDeltaTime * fadeDirection, fadeDuration);

		FadeHighlight();
		material.SetVector(centerID, viewportTarget);

		if (fadeTime < 0)
			Destroy(gameObject);
	}

	private Vector2 GetViewportTarget() {
		return camera.WorldToViewportPoint(worldTarget.position);
	}

	private void FadeHighlight() {
		alpha = MathX.EaseInQuad(0, baseAlpha, fadeTime / fadeDuration);
		Mathf.Clamp(alpha, 0, baseAlpha);

		material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
	}

	public void FadeOut() {
		fadeDirection *= -1;
	}
}
