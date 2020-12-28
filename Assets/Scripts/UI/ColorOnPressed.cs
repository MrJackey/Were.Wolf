using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorOnPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	[SerializeField] private Image image;
	[SerializeField] private Color pressedColor;

	private Color normalColor;

	private void Awake() {
		normalColor = image.color;
	}

	public void OnPointerDown(PointerEventData eventData) {
		image.color = pressedColor;
	}

	public void OnPointerUp(PointerEventData eventData) {
		image.color = normalColor;
	}
}