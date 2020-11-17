//
// Author: Jonatan Johansson
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
/// Represents a reference to a scene which you can select from the editor.
/// </summary>
[Serializable]
public class SceneReference : ISerializationCallbackReceiver {
	[SerializeField, HideInInspector]
	private string scenePath = "";

#if UNITY_EDITOR
	[SerializeField]
	private Object sceneAsset;

	private string GetScenePathFromAsset() =>
		sceneAsset as SceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : "";
#endif

	public string ScenePath {
		get {
		#if UNITY_EDITOR
			return GetScenePathFromAsset();
		#else
			return scenePath;
		#endif
		}
	}

	public void OnBeforeSerialize() {
	#if UNITY_EDITOR
		// Update the scenePath field before serialization.
		scenePath = GetScenePathFromAsset();
	#endif
	}

	public void OnAfterDeserialize() { }

	// Implicit conversion to string so that it can be passed directly to SceneManager.LoadScene(string) etc.
	public static implicit operator string(SceneReference sceneReference) =>
		sceneReference.ScenePath;
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferencePropertyDrawer : PropertyDrawer {
		private bool hasScene;
		private SerializedProperty sceneAssetProperty;
		private SceneInfo sceneInfo;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			sceneAssetProperty = property.FindPropertyRelative("sceneAsset");

			// Check selected scene.
			SceneAsset sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;
			hasScene = sceneAsset != null;
			if (hasScene) sceneInfo = GetSceneInfo(sceneAsset);

			// Context menu.
			Event e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
				DoContextMenu();

			// Indicator color.
			Color oldColor = GUI.backgroundColor;
			if (hasScene) {
				if (!sceneInfo.InBuildSettings) {
					GUI.backgroundColor = Color.red;
					label.tooltip = "*Not in build* " + label.tooltip;
				}
				else if (!sceneInfo.enabled) {
					GUI.backgroundColor = Color.yellow;
					label.tooltip = $"*Disabled, build index: {sceneInfo.buildIndex}* " + label.tooltip;
				}
				else {
					GUI.backgroundColor = Color.green;
					label.tooltip = $"*Enabled, build index: {sceneInfo.buildIndex}* " + label.tooltip;
				}
			}

			// Draw property.
			label = EditorGUI.BeginProperty(position, label, property);
			sceneAssetProperty.objectReferenceValue =
				EditorGUI.ObjectField(position, label, sceneAssetProperty.objectReferenceValue,
				                      typeof(SceneAsset), false) as SceneAsset;
			EditorGUI.EndProperty();

			// Restore color.
			GUI.backgroundColor = oldColor;
		}

		private void DoContextMenu() {
			if (!hasScene) return;
			GenericMenu menu = new GenericMenu();

			if (hasScene) {
				if (sceneInfo.InBuildSettings)
					menu.AddItem(new GUIContent("Remove scene from build settings"), false, RemoveFromBuild,
					             sceneAssetProperty);
				else
					menu.AddItem(new GUIContent("Add scene to build settings"), false, AddToBuild, sceneAssetProperty);

				if (sceneInfo.InBuildSettings)
					menu.AddItem(new GUIContent("Enabled in build"), sceneInfo.enabled, ToggleEnableInBuild,
					             sceneAssetProperty);
			}

			menu.ShowAsContext();
		}


		// Helpers

		private struct SceneInfo {
			public SceneAsset asset;
			public string path;
			public bool enabled;
			public int buildIndex;

			public bool InBuildSettings => buildIndex != -1;
			public bool IsValid => path != null;
		}

		private static SceneInfo GetSceneInfo(SceneAsset sceneAsset) {
			string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
			if (assetPath == null) return new SceneInfo {asset = sceneAsset};

			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			int index = Array.FindIndex(buildScenes, buildScene => buildScene.path == assetPath);

			return new SceneInfo {
				asset = sceneAsset,
				path = assetPath,
				buildIndex = index,
				enabled = index != -1 && buildScenes[index].enabled
			};
		}

		private static bool TryGetSceneFromProperty(SerializedProperty property, out SceneInfo sceneInfo) {
			sceneInfo = default;
			var sceneAsset = property.objectReferenceValue as SceneAsset;
			if (sceneAsset == null) return false;
			sceneInfo = GetSceneInfo(sceneAsset);
			return sceneInfo.IsValid;
		}

		// Context actions

		private static void AddToBuild(object arg) {
			((SerializedProperty)arg).serializedObject.Update();
			if (!TryGetSceneFromProperty((SerializedProperty)arg, out SceneInfo sceneInfo)) return;
			if (!sceneInfo.InBuildSettings) {
				EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes
					.Append(new EditorBuildSettingsScene(sceneInfo.path, true)).ToArray();
				EditorBuildSettings.scenes = buildScenes;

				Debug.Log(
					$"Added scene: '{sceneInfo.asset.name}' ({sceneInfo.path}) to build settings at build index: {buildScenes.Length - 1}.");
			}
		}

		private static void RemoveFromBuild(object arg) {
			SerializedProperty property = (SerializedProperty)arg;
			property.serializedObject.Update();
			if (!TryGetSceneFromProperty(property, out SceneInfo scene)) return;
			if (!scene.InBuildSettings) return;

			List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
			int index = scene.buildIndex;
			if (index < buildScenes.Count && buildScenes[index].path == scene.path) {
				buildScenes.RemoveAt(index);
				EditorBuildSettings.scenes = buildScenes.ToArray();

				Debug.Log(
					$"Removed scene: '{scene.asset.name}' ({scene.path}) with build index: {index} from build settings.");
			}
		}

		private static void ToggleEnableInBuild(object arg) {
			SerializedProperty property = (SerializedProperty)arg;
			property.serializedObject.Update();
			if (!TryGetSceneFromProperty(property, out SceneInfo scene)) return;
			if (!scene.InBuildSettings) return;


			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			int index = scene.buildIndex;
			if (index < buildScenes.Length && buildScenes[index].path == scene.path) {
				buildScenes[index].enabled = !scene.enabled;
				EditorBuildSettings.scenes = buildScenes;

				Debug.Log(
					$"{(buildScenes[index].enabled ? "Enabled" : "Disabled")} scene: '{scene.asset.name}' ({scene.path}) with build index: {index} in build settings.");
			}
		}
	}
}
#endif