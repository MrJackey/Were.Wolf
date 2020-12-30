using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor {
	public class Builder : EditorWindow {
		[MenuItem("Tools/Build")]
		private static void ShowWindow() {
			var window = GetWindow<Builder>();
			window.titleContent = new GUIContent("Builder");
			window.Show();
		}

		[SerializeField] private string outputPath;
		[SerializeField] private bool runOnFinished;

		private void OnEnable() {
			outputPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.StandaloneWindows64);
			if (string.IsNullOrEmpty(outputPath))
				outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/Were.Wolf.exe"));
		}

		private void OnGUI() {
			outputPath = EditorGUILayout.TextField("Output Path", outputPath);
			runOnFinished = EditorGUILayout.Toggle("Start When Finished", runOnFinished);

			if (GUILayout.Button("Development Build")) {
				BuildPlayerOptions buildOptions = DefaultOptions();
				buildOptions.options = BuildOptions.Development;
				DoBuild(buildOptions);
			}

			if (GUILayout.Button("Release Build")) {
				BuildPlayerOptions buildOptions = DefaultOptions();
				DoBuild(buildOptions);
			}

			if (GUILayout.Button("Release Build (cheats)")) {
				BuildPlayerOptions buildOptions = DefaultOptions();
				buildOptions.extraScriptingDefines = new[] {"CHEATS"};
				DoBuild(buildOptions);
			}
		}

		private BuildPlayerOptions DefaultOptions() {
			return new BuildPlayerOptions {
				options = BuildOptions.None,
				scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray(),
				target = BuildTarget.StandaloneWindows64,
				targetGroup = BuildTargetGroup.Standalone,
				locationPathName = outputPath,
			};
		}

		private void DoBuild(BuildPlayerOptions buildOptions) {
			BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
			Debug.Log($"Build finished with result: {report.summary.result}");

			if (runOnFinished && report.summary.result == BuildResult.Succeeded)
				Process.Start(outputPath);
		}
	}
}