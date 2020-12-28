using UnityEngine;

public class SetCursor : MonoBehaviour {
	[SerializeField] private Texture2D texture = null;
	[SerializeField] private Vector2 hotspot = Vector2.zero;
	[SerializeField] private CursorMode mode = CursorMode.ForceSoftware;

	private void Start() {
		Cursor.SetCursor(texture, hotspot, mode);
	}
}