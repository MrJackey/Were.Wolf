using UnityEngine;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(Canvas))]
public class FindMainCameraForCanvas : MonoBehaviour {
	private void OnEnable() {
		GetComponent<Canvas>().worldCamera = Camera.main;
	}
}