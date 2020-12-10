using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class VignetteHighlight : MonoBehaviour {
	private static readonly int centerID = Shader.PropertyToID("_Center");

	[SerializeField] private RawImage rawImage;
	[SerializeField] private float fadeDuration = 1f;

	private new Camera camera = null;
	private Transform worldTarget = null;
	private Material material = null;
	private Color baseColor;
	private float baseAlpha = 0f;
	private float fadeTime = 0f;
	private int fadeDirection = 1;

	public Transform WorldTarget { set => worldTarget = value; }

	private void Start() {
		Canvas canvas = GetComponent<Canvas>();
		camera = canvas.worldCamera;

		material = new Material(rawImage.material);
		rawImage.material = material;
		baseColor = material.color;
		baseAlpha = baseColor.a;
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
		baseColor.a = MathX.EaseOutQuad(0, baseAlpha, fadeTime / fadeDuration);

		material.color = baseColor;
	}

	public void FadeOut() {
		fadeDirection *= -1;
	}
}
