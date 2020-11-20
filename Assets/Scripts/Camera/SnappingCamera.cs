using UnityEngine;

public class SnappingCamera : MonoBehaviour {
	[SerializeField] private Transform target = null;

	[Header("Smoothing")]
	[SerializeField] private bool enableSmoothing = true;
	[SerializeField, EnableIf(nameof(enableSmoothing))]
	private float transitionDuration = 0.15f;

	private new Camera camera;
	private Vector3 currentVelocity;
	private Vector2 gridOrigin;

	private void Start() {
		camera = GetComponent<Camera>();
		gridOrigin = GetCameraWorldRect(camera).min;

		if (target == null) {
			GameObject go = GameObject.FindWithTag("Player");
			if (go != null) target = go.transform;
		}

		if (!camera.orthographic)
			Debug.LogError($"{nameof(SnappingCamera)} only works on orthographic cameras!");

		if (target == null)
			Debug.LogError($"{nameof(SnappingCamera)} has no target.");
	}

	private void LateUpdate() {
		Vector3 targetCameraPosition = CalculateTargetPosition();
		if (enableSmoothing)
			transform.position = Vector3.SmoothDamp(transform.position, targetCameraPosition,
			                                        ref currentVelocity, transitionDuration);
		else
			transform.position = targetCameraPosition;
	}

	private Vector3 CalculateTargetPosition() {
		Rect cameraRect = GetCameraWorldRect(camera);
		Vector2 targetPositionInGrid = ((Vector2)target.position - gridOrigin) / cameraRect.size;
		Vector2 gridCell = new Vector2(Mathf.Floor(targetPositionInGrid.x), Mathf.Floor(targetPositionInGrid.y));

		return gridOrigin + gridCell * cameraRect.size + cameraRect.size / 2;
	}

	private static Rect GetCameraWorldRect(Camera camera) {
		Rect viewRect = camera.rect;
		return new Rect {
			min = camera.ViewportToWorldPoint(new Vector3(viewRect.xMin, viewRect.yMin, 0)),
			max = camera.ViewportToWorldPoint(new Vector3(viewRect.xMax, viewRect.yMax, 0)),
		};
	}
}