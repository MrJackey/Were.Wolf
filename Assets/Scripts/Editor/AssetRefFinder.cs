/*
 * Author: Jonatan Johansson
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AssetRefFinder {
	[MenuItem("Assets/Find all references")]
	private static void FindAllAssetRefs() {
		var dict = new Dictionary<string, List<string>>();

		foreach (Object o in Selection.objects) {
			string assetPath = AssetDatabase.GetAssetPath(o);
			if (string.IsNullOrEmpty(assetPath)) continue;
			dict.Add(assetPath, new List<string>());
		}

		if (dict.Count == 0) return;

		foreach (string path in AssetDatabase.GetAllAssetPaths()) {
			string[] deps = AssetDatabase.GetDependencies(path);
			foreach (string key in dict.Keys) {
				if (key == path) continue;
				if (deps.Contains(key)) {
					dict[key].Add(path);
				}
			}
		}

		var sb = new StringBuilder();
		foreach (var pair in dict) {
			sb.AppendLine($"Found {pair.Value.Count} references to {pair.Key}\n");
			foreach (string s in pair.Value.OrderBy(path => path)) {
				sb.Append("- ");
				sb.AppendLine(s);
			}

			sb.AppendLine();
		}

		Debug.Log(sb.ToString());
	}

	[MenuItem("Assets/Find all references", true)]
	private static bool FindAllAssetRefsValidate() => Selection.objects.Length != 0;
}
