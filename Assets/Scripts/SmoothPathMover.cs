using Cinemachine;
using UnityEngine;

public class SmoothPathMover : MonoBehaviour {
	[SerializeField] private CinemachinePathBase path = null;
	[SerializeField] private float speed = 5f;
	[SerializeField] private bool bounce = true;
	[SerializeField] private bool rotate = false;

	private float time;
	private float direction;

	private void Start() {
		if (path == null)
			path = GetComponent<CinemachinePathBase>();
	}

	private void Update() {
		float pos;
		if (bounce) {
			time += Time.deltaTime * direction;
			pos = time * speed;
			float max = path.MaxUnit(CinemachinePathBase.PositionUnits.Distance);

			if (pos >= max) {
				pos = max;
				direction = -1;
				UpdateScale();
			}
			else if (pos <= 0) {
				pos = 0;
				direction = 1;
				UpdateScale();
			}
		}
		else {
			time += Time.deltaTime;
			pos = time * speed;
		}

		transform.position = path.EvaluatePositionAtUnit(pos, CinemachinePathBase.PositionUnits.Distance);

		if (rotate) {
			Vector3 tangent = path.EvaluateTangentAtUnit(pos, CinemachinePathBase.PositionUnits.Distance);
			float angle = Mathf.Atan2(tangent.y, tangent.x);
			transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
		}
	}

	private void UpdateScale() {
		Vector3 scale = transform.localScale;
		scale.x = direction;
		transform.localScale = scale;
	}
}