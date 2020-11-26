using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreens : MonoBehaviour {
	[SerializeField, PropertySetter(nameof(IsVisible))]
	private bool isVisible = true;

	[SerializeField] private GameObject[] screens;

	private readonly Stack<int> stack = new Stack<int>();

	public int CurrentIndex => stack.Peek();
	public GameObject CurrentScreen => screens[stack.Peek()];

	public bool IsVisible {
		get => isVisible;
		set {
			isVisible = value;
			CurrentScreen.SetActive(isVisible);
		}
	}

	private void Awake() {
		if (screens.Length == 0)
			Debug.LogError("No screens selected.");

		screens[0].SetActive(isVisible);
		for (int i = 1; i < screens.Length; i++)
			screens[i].SetActive(false);

		stack.Push(0);
	}

	public void PushScreen(GameObject screen) {
		int index = Array.IndexOf(screens, screen);
		if (index == -1) {
			Debug.LogError("Screen not found.");
			return;
		}

		PushScreen(index);
	}

	public void PushScreen(int index) {
		screens[stack.Peek()].SetActive(false);
		screens[index].SetActive(isVisible);
		stack.Push(index);
	}

	public void PopScreen() {
		if (stack.Count == 1) {
			Debug.LogError("Attempt to pop the last menu off the stack.");
			return;
		}

		screens[stack.Pop()].SetActive(false);
		screens[stack.Peek()].SetActive(isVisible);
	}
}