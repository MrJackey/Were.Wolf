using System;
using UnityEngine;
using UnityEngine.UI;

public class UIValueBar : MonoBehaviour {
	[SerializeField] private RectTransform sliderMask;
	[SerializeField] private RectTransform foreground, background;
	[SerializeField] private float value;
	[SerializeField] private float maxValue;

	private float unitWidth;

	public float Value {
		get => value;
		set {
			this.value = Mathf.Clamp(value, 0, maxValue);
			UpdateMaskSize();
		}
	}

	public float MaxValue {
		get => maxValue;
		set {
			maxValue = Mathf.Max(value, 0);
			this.value = Mathf.Clamp(this.value, 0, maxValue);
			UpdateBarSize();
			UpdateMaskSize();
		}
	}

	private void Start() {
		unitWidth = foreground.GetComponent<Image>().sprite.rect.width;
	}

	private void OnValidate() {
		value = Mathf.Clamp(value, 0, maxValue);

		if (foreground != null && background != null) {
			Image fgImage = foreground.GetComponent<Image>();
			if (fgImage != null && fgImage.sprite != null)
				unitWidth = fgImage.sprite.rect.width;

			UpdateBarSize();
		}

		UpdateMaskSize();
	}

	private void UpdateBarSize() {
		float width = unitWidth * maxValue;
		SetWidth(foreground, width);
		SetWidth(background, width);
	}

	private void UpdateMaskSize() {
		if (maxValue == 0f) return;
		Vector2 maskSize = sliderMask.sizeDelta;
		maskSize.x = unitWidth * maxValue * (value / maxValue);
		sliderMask.sizeDelta = maskSize;
	}

	private static void SetWidth(RectTransform rectTransform, float width) {
		Vector2 size = rectTransform.sizeDelta;
		size.x = width;
		rectTransform.sizeDelta = size;
	}
}