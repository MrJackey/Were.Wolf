using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreens : MonoBehaviour {
	[SerializeField] private GameObject[] screens;

	private Stack<int> stack = new Stack<int>();

	public int CurrentIndex => stack.Peek();
	public GameObject CurrentScreen => screens[stack.Peek()];

	private void Start() {
		if (screens.Length == 0)
			Debug.LogError("No screens selected.");

		screens[0].SetActive(true);
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
		screens[index].SetActive(true);
		stack.Push(index);
	}

	public void PopScreen() {
		if (stack.Count == 1) {
			Debug.LogError("Attempt to pop the last menu off the stack.");
			return;
		}

		screens[stack.Pop()].SetActive(false);
		screens[stack.Peek()].SetActive(true);
	}
}