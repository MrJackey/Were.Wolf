using UnityEngine;

public static class GameObjectExtensions {
	public static T GetComponentUnlessNull<T>(this GameObject gameObject) where T : Component {
		return gameObject != null ? gameObject.GetComponent<T>() : null;
	}

	public static T GetComponentUnlessNull<T>(this Component component) where T : Component {
		return component != null ? component.GetComponent<T>() : null;
	}
}