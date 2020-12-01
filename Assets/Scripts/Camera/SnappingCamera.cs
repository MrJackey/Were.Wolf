using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SnappingCamera : MonoBehaviour {
	[SerializeField] private Transform target = null;

	[Header("Smoothing")]
	[SerializeField] private bool enableSmoothing = true;
	[SerializeField, EnableIf(nameof(enableSmoothing))]
	private float transitionDuration = 0.15f;

#if UNITY_EDITOR
	[Header("Grid")]
	[SerializeField] private bool showGrid = true;
	[SerializeField] private Color gridColor = Color.gray;
	[SerializeField] private Color activeCellColor = Color.white;
#endif

	private new Camera camera;
	private Vector3 currentVelocity;
	private Vector2 startPosition;
	private Vector2 gridOrigin;
	private Vector2 cellSize;

	public Transform Target { set => target = value; }
	public float TransitionDuration {
		get => transitionDuration;
		set => transitionDuration = value;
	}

	private void Start() {
		camera = GetComponent<Camera>();
		startPosition = transform.position;

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
		float cameraSize = camera.orthographicSize;
		cellSize = GetCameraWorldRect(camera).size;
		gridOrigin = startPosition - new Vector2(cameraSize * camera.aspect, cameraSize);
		Vector3 targetCameraPosition = CalculateTargetPosition();

		if (enableSmoothing) {
			transform.position = Vector3.SmoothDamp(transform.position, targetCameraPosition,
			                                        ref currentVelocity, transitionDuration,
			                                        float.PositiveInfinity, Time.unscaledDeltaTime);
		}
		else {
			transform.position = targetCameraPosition;
		}
	}

	private Vector3 CalculateTargetPosition() {
		Vector3 targetPosition = CalculateTargetRect().center;
		targetPosition.z = transform.position.z;
		return targetPosition;
	}

	private Rect CalculateTargetRect() {
		if (target == null)
			return GetCameraWorldRect(camera);

		Vector2 gridPos = WorldToGrid(target.position);
		return new Rect(GridToWorld(MathX.Floor(gridPos)), cellSize);
	}

	private Vector2 WorldToGrid(Vector2 point) {
		return (point - gridOrigin) / cellSize;
	}

	private Vector2 GridToWorld(Vector2 point) {
		return gridOrigin + point * cellSize;
	}


	private static Rect GetCameraWorldRect(Camera camera) {
		Rect viewRect = camera.rect;
		return new Rect {
			min = camera.ViewportToWorldPoint(new Vector3(viewRect.xMin, viewRect.yMin, 0)),
			max = camera.ViewportToWorldPoint(new Vector3(viewRect.xMax, viewRect.yMax, 0)),
		};
	}


#if UNITY_EDITOR
	private void OnDrawGizmos() {
		if (!showGrid) return;
		if (!Application.isPlaying) {
			camera = GetComponent<Camera>();
			if (camera != null) {
				Rect cameraRect = GetCameraWorldRect(camera);
				gridOrigin = cameraRect.min;
				cellSize = cameraRect.size;
			}
		}

		if (camera == null) return;

		if (UnityEditor.SceneView.currentDrawingSceneView == null ||
			!UnityEditor.SceneView.currentDrawingSceneView.camera.orthographic) return;
		Camera sceneViewCamera = UnityEditor.SceneView.currentDrawingSceneView.camera;
		Rect window = GetCameraWorldRect(sceneViewCamera);

		Vector2 minGridPoint = GridToWorld(MathX.Ceil(WorldToGrid(window.min)));
		Vector2 maxGridPoint = GridToWorld(MathX.Floor(WorldToGrid(window.max)));

		DrawGrid(window, minGridPoint, maxGridPoint);
	}

	private void DrawGrid(Rect window, Vector2 min, Vector2 max) {
		const float small = 0.0001f;

		// Draw the grid lines.
		Gizmos.color = gridColor;
		for (float x = min.x; x <= max.x + small; x += cellSize.x)
			Gizmos.DrawLine(new Vector3(x, window.yMin),
			                new Vector3(x, window.yMax));

		for (float y = min.y; y <= max.y + small; y += cellSize.y)
			Gizmos.DrawLine(new Vector3(window.xMin, y),
			                new Vector3(window.xMax, y));

		// Draw the active grid cell.
		Gizmos.color = activeCellColor;
		GizmoUtils.DrawRect(CalculateTargetRect());
	}
#endif
}
