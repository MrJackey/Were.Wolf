using System;
using UnityEngine;
using Discord;
using UnityEngine.SceneManagement;

public class DiscordManager : SingletonBehaviour<DiscordManager> {
	private const long ClientID = 796132860087894076L;

	[SerializeField] private SceneHelper sceneHelper;

	private Discord.Discord discord;
	private Activity discordActivity;

	private void OnEnable() {
		DontDestroyOnLoad(this);

		try {
			discord = new Discord.Discord(ClientID, (ulong)CreateFlags.NoRequireDiscord);

			LogLevel logLevel = LogLevel.Error;
		#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logLevel = LogLevel.Info;
		#endif

			Debug.Log("Discord initialized");
			discord.SetLogHook(logLevel, LogHandler);

			discordActivity = new Activity {
				Type = ActivityType.Playing,
				State = "Playing",
				Assets = new ActivityAssets {
					LargeImage = "team_wolf_icon_x2",
				},
				Timestamps = new ActivityTimestamps {
					Start = DateTimeOffset.Now.ToUnixTimeSeconds()
				},
			};

			UpdateActivity();
		}
		catch {
			Debug.LogError("Discord failed to initialize.");
			Destroy(gameObject);
			return;
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;

		if (discord != null) {
			Debug.Log("Discord shutting down");
			discord.Dispose();
			discord = null;
		}
	}

	private void Update() {
		discord.RunCallbacks();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		int level = sceneHelper.FindLevelByBuildIndex(scene.buildIndex);
		if (level != -1) {
			discordActivity.State = $"Playing Level {level}";
			UpdateActivity();
		}
		else if (discordActivity.State != "Playing") {
			discordActivity.State = "Playing";
			UpdateActivity();
		}
	}

	private void LogHandler(LogLevel level, string message) {
		LogType logType;
		switch (level) {
			case LogLevel.Error:
				logType = LogType.Error;
				break;
			case LogLevel.Warn:
				logType = LogType.Warning;
				break;
			default:
				logType = LogType.Log;
				break;
		}

		Debug.unityLogger.Log(logType, "[Discord] " + message);
	}

	private void UpdateActivity() {
		discord.GetActivityManager().UpdateActivity(discordActivity, result => {
			if (result != Result.Ok)
				Debug.LogError($"[Discord] UpdateActivity failed with result {result}");
		});
	}
}