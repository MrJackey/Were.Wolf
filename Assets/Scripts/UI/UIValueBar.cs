using UnityEngine;

public class UIValueBar : MonoBehaviour {
	[SerializeField] private RectTransform sliderMask;
	[SerializeField] private float value;
	[SerializeField] private float maxValue;

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
			UpdateMaskSize();
		}
	}

	private void OnValidate() {
		value = Mathf.Clamp(value, 0, maxValue);
		UpdateMaskSize();
	}

	private void UpdateMaskSize() {
		if (maxValue == 0f) return;
		RectTransform parent = (RectTransform)sliderMask.parent;
		Vector2 maskSize = sliderMask.sizeDelta;
		maskSize.x = parent.sizeDelta.x * (value / maxValue);
		sliderMask.sizeDelta = maskSize;
	}
}