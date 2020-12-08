using System;
using UnityEngine;
using UnityEngine.UI;

public class UIIndexedImage : MonoBehaviour {
	[SerializeField] private Sprite[] sprites;

	private Image image;

	public float Value {
		set {
			int index = Mathf.Clamp(Mathf.FloorToInt(value * sprites.Length), 0, sprites.Length - 1);
			image.sprite = sprites[index];
		}
	}

	private void Awake() {
		image = GetComponent<Image>();
	}
}