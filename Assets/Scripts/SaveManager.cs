using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
	[SerializeField] private SaveData data;

	private string saveFile;

	public SaveData Data => data;

	public static SaveManager Instance { get; private set; }

	private void Awake() {
		saveFile = Path.Combine(Application.persistentDataPath, "save.json");

		Instance = this;
		DontDestroyOnLoad(this);
		Load();
	}

	private void OnDestroy() {
		Save();
	}

	[ContextMenu("Load Data")]
	public void Load() {
		if (!File.Exists(saveFile)) return;

		string json;

		try { json = File.ReadAllText(saveFile); }
		catch (IOException e) {
			Debug.LogError($"Failed to read save file: {e}");
			return;
		}

		try { data = JsonUtility.FromJson<SaveData>(json); }
		catch (Exception e) {
			Debug.LogError($"Failed to deserialize save file: {e}");
		}
	}

	[ContextMenu("Save Data")]
	public void Save() {
		try {
			string json = JsonUtility.ToJson(data);
			File.WriteAllText(saveFile, json);
		}
		catch (IOException e) {
			Debug.LogError($"Failed to write save file: {e}");
		}
	}

	[ContextMenu("Clear Data")]
	public void ClearData() {
		data = new SaveData();
		File.Delete(saveFile);
	}


	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad() {
		new GameObject("Save Manager", typeof(SaveManager));
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Tools/Clear Save Data")]
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
		public int completedLevel;
	}
}