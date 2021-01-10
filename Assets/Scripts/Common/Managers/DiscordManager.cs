using System;
using UnityEngine;
using Discord;

public class DiscordManager : SingletonBehaviour<DiscordManager> {
	private const long ClientID = 796132860087894076L;

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
				Assets = new ActivityAssets {
					LargeImage = "team_wolf_icon_x2",
				},
				Timestamps = new ActivityTimestamps {
					Start = DateTimeOffset.Now.ToUnixTimeSeconds()
				},
			};

			discord.GetActivityManager().UpdateActivity(discordActivity, result => {
				if (result != Result.Ok)
					Debug.LogError($"[Discord] UpdateActivity failed with result {result}");
			});
		}
		catch {
			Debug.LogError("Discord failed to initialize.");
			Destroy(gameObject);
		}
	}

	private void OnDisable() {
		if (discord != null) {
			Debug.Log("Discord shutting down");
			discord.Dispose();
			discord = null;
		}
	}

	private void Update() {
		discord.RunCallbacks();
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
}