using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class DiscoLight : MonoBehaviour {
	[SerializeField] private float cycleSpeed = 1;

	private new Light2D light;

	private void Start() {
		light = GetComponent<Light2D>();
	}

	private void Update() {
		float hue = (Mathf.Sin(Time.time * cycleSpeed) + 1f) / 2f;
		light.color = Color.HSVToRGB(hue, 1, 1);
	}
}