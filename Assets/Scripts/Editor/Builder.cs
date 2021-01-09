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

		[SerializeField] private bool developmentBuild;

		private void OnGUI() {
			developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);

			if (GUILayout.Button("Windows Build"))
				DoBuild(BuildTarget.StandaloneWindows64, developmentBuild);

			if (GUILayout.Button("Windows Build (cheats)"))
				DoBuild(BuildTarget.StandaloneWindows64, developmentBuild, new[] {"CHEATS"});

			if (GUILayout.Button("Multiplatform Build"))
				BuildMultiplatform();

			GUILayout.Space(10);

			if (GUILayout.Button("Open Build Directory"))
				EditorUtility.RevealInFinder(Path.GetFullPath(Application.dataPath + "/../Build"));
		}

		private void BuildMultiplatform() {
			if (DoBuild(BuildTarget.StandaloneWindows64, developmentBuild) == BuildResult.Cancelled) return;
			if (DoBuild(BuildTarget.StandaloneOSX, developmentBuild) == BuildResult.Cancelled) return;
			if (DoBuild(BuildTarget.StandaloneLinux64, developmentBuild) == BuildResult.Cancelled) return;
		}

		private static string[] GetActiveBuildScenes() {
			return EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path)
				.ToArray();
		}

		private static BuildResult DoBuild(BuildPlayerOptions buildOptions) {
			BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
			Debug.Log($"Build for {buildOptions.target} finished with result {report.summary.result}");
			return report.summary.result;
		}

		private static BuildResult DoBuild(BuildTarget target, bool developmentBuild, string[] extraDefines = null) {
			string buildDirPath = Path.GetFullPath(Application.dataPath + "/../Build");

			BuildPlayerOptions options = new BuildPlayerOptions {
				scenes = GetActiveBuildScenes(),
				target = target,
				targetGroup = BuildTargetGroup.Standalone,
				options = developmentBuild ? BuildOptions.Development : BuildOptions.None,
				extraScriptingDefines = extraDefines,
			};

			switch (target) {
				case BuildTarget.StandaloneWindows64:
					string windowsDir = $"{buildDirPath}/Win64/{Application.productName}";
					options.locationPathName = windowsDir + $"/{Application.productName}.exe";
					Directory.CreateDirectory(windowsDir);
					return DoBuild(options);

				case BuildTarget.StandaloneOSX:
					string macDir = $"{buildDirPath}/Mac/{Application.productName}.app";
					options.locationPathName = macDir;

					Directory.CreateDirectory(macDir);
					return DoBuild(options);

				case BuildTarget.StandaloneLinux64:
					string linuxDir = $"{buildDirPath}/Linux/{Application.productName}";
					options.locationPathName = linuxDir + $"/{Application.productName}.x86_64";

					Directory.CreateDirectory(linuxDir);
					return DoBuild(options);

				default:
					throw new ArgumentException("Unsupported build target.");
			}
		}
	}
}