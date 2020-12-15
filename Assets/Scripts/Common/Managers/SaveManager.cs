using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
	[SerializeField] private SaveData saveData;
	[SerializeField] private SettingsData settingsData;

	private string saveFile;
	private string settingsFile;

	public SaveData Data => saveData;

	public SettingsData Settings => settingsData;

	public static SaveManager Instance { get; private set; }

	private void Awake() {
		saveFile = Path.Combine(Application.persistentDataPath, "save.json");
		settingsFile = Path.Combine(Application.persistentDataPath, "settings.json");

		Instance = this;
		DontDestroyOnLoad(this);
		Load();
	}

	private void OnDestroy() {
		Save();
	}

	[ContextMenu("Load Data")]
	public void Load() {
		if (!DeserializeFile(saveFile, out saveData))
			saveData = new SaveData();

		if (!DeserializeFile(settingsFile, out settingsData))
			settingsData = new SettingsData();
	}

	[ContextMenu("Save Data")]
	public void Save() {
		SerializeFile(saveFile, saveData);
		SerializeFile(settingsFile, settingsData);
	}

	[ContextMenu("Clear Data")]
	public void ClearData() {
		saveData = new SaveData();
		File.Delete(saveFile);
	}


	private static bool DeserializeFile<T>(string path, out T data) where T : class, new() {
		data = null;
		if (!File.Exists(path)) return false;

		string json;
		try {
			json = File.ReadAllText(path);
		}
		catch (IOException e) {
			Debug.LogError($"Failed to read file: {e}");
			return false;
		}

		try {
			data = JsonUtility.FromJson<T>(json);
			return data != null;
		}
		catch (Exception e) {
			Debug.LogError($"Failed to deserialize file: {e}");
			return false;
		}
	}

	private static bool SerializeFile(string path, object data) {
		try {
			string json = JsonUtility.ToJson(data);
			File.WriteAllText(path, json);
			return true;
		}
		catch (IOException e) {
			Debug.LogError($"Failed to write file: {e}");
			return false;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnLoad() {
		_ = new GameObject("Save Manager", typeof(SaveManager));
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Tools/Open Save Directory", priority = 0)]
	private static void OpenSaveDirectory() {
		UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
	}

	[UnityEditor.MenuItem("Tools/Clear Save Data", priority = 1)]
	private static void ClearSaveData() {
		if (Application.isPlaying) {
			Instance.ClearData();
		}
		else {
			string saveFile = Path.Combine(Application.persistentDataPath, "save.json");
			File.Delete(saveFile);
		}
	}
#endif


	[Serializable]
	public class SaveData {
		public int completedLevel = -1;
	}

	[Serializable]
	public class SettingsData {
		public float masterVolume = 0.5f;
		public float musicVolume = 0.5f;
		public float sfxVolume = 0.5f;
	}
}