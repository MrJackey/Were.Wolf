using UnityEngine;

public class TimeShift : MonoBehaviour {
	[SerializeField, Range(0.01f, 2f)]
	private float speed = 1f;

	private void Update() {
		if (Time.timeScale != 0)
			Time.timeScale = speed;
	}
}