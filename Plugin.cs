using System;
using System.Net.Http;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalShockCompany.Patches;
using Newtonsoft.Json;

namespace LethalShockCompany
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	[BepInProcess("Lethal Company.exe")]
	public class Plugin : BaseUnityPlugin
	{
		internal static ManualLogSource Log;
		internal static HttpClient HttpClient = new HttpClient();

		internal static ConfigEntry<string> ConfigUsername;
		internal static ConfigEntry<string> ConfigCode;
		internal static ConfigEntry<string> ConfigApiKey;
		internal static ConfigEntry<int> ConfigMaxIntensity;
		internal static ConfigEntry<bool> ConfigVibrateOnDamage;

		private void Awake()
		{
			Log = Logger;
			var harmony = new Harmony("com.volcano.patches.LethalShockCompany");

			var originalKillPlayer =
				AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer));

			var preKillPlayer =
				AccessTools.Method(typeof(ManualPatches), nameof(ManualPatches.PreKillPlayer));
			var postKillPlayer =
				AccessTools.Method(typeof(ManualPatches), nameof(ManualPatches.PostKillPlayer));

			harmony.Patch(originalKillPlayer, new HarmonyMethod(preKillPlayer), new HarmonyMethod(postKillPlayer));

			var originalDamagePlayer =
				AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer));

			var preDamagePlayer =
				AccessTools.Method(typeof(ManualPatches), nameof(ManualPatches.PreDamagePlayer));
			var postDamagePlayer =
				AccessTools.Method(typeof(ManualPatches), nameof(ManualPatches.PostDamagePlayer));

			harmony.Patch(originalDamagePlayer, new HarmonyMethod(preDamagePlayer),
				new HarmonyMethod(postDamagePlayer));

			ConfigUsername = Config.Bind("General", "Username", "Username", "Username to access pi shock api with");
			ConfigCode = Config.Bind("General", "Code", "Code", "Share code to access pi shock api with");
			ConfigApiKey = Config.Bind("General", "ApiKey", "ApiKey", "Api key to access pi shock api with");
			ConfigMaxIntensity = Config.Bind("General", "MaxIntensity", 100, "Maximum intensity of the shock");
			ConfigVibrateOnDamage = Config.Bind("General", "VibrateOnDamage", false,
				"Vibrate when taking damage, in some cases this may prevent a shock when dying");
			// Plugin startup logic

			HttpClient.BaseAddress = new Uri("https://do.pishock.com");
			//HttpClient.BaseAddress = new Uri("http://localhost:8080");

			Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
			Log.LogInfo("Sending test beep...");

			SendPost(100, 1, 2);
		}

		public static void SendPost(int intensity, int duration, int op)
		{
			object body = new
			{
				Username = Plugin.ConfigUsername.Value,
				Code = Plugin.ConfigCode.Value,
				ApiKey = Plugin.ConfigApiKey.Value,
				Op = op,
				Intensity = intensity,
				Duration = duration,
			};

			var content = new StringContent(
				JsonConvert.SerializeObject(body),
				Encoding.UTF8,
				"application/json"
			);
			Plugin.HttpClient.PostAsync("/api/apioperate", content);
		}
	}
}