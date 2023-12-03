using GameNetcodeStuff;
using HarmonyLib;

namespace LethalShockCompany.Patches
{
	public class ManualPatches
	{
		[HarmonyPrefix]
		// ReSharper disable once InconsistentNaming
		public static void PreKillPlayer(PlayerControllerB __instance, ref bool __state)
		{
			Plugin.Log.LogDebug("PlayerControllerB.KillPlayer() was called!");
			__state = __instance.isPlayerDead;
		}

		[HarmonyPostfix]
		// ReSharper disable once InconsistentNaming
		public static void PostKillPlayer(PlayerControllerB __instance, bool __state)
		{
			Plugin.Log.LogDebug("PlayerControllerB.KillPlayer() was called!");

			if (__instance.isPlayerDead && !__state && __instance.IsOwner)
			{
				Plugin.SendPost(Plugin.ConfigMaxIntensity.Value, 1, 0);
				Plugin.Log.LogInfo($"Player {__instance.playerUsername} has died, sending shock!");
			}
		}

		[HarmonyPrefix]
		// ReSharper disable once InconsistentNaming
		public static void PreDamagePlayer(PlayerControllerB __instance, ref int __state)
		{
			Plugin.Log.LogDebug("PlayerControllerB.DamagePlayer() was called!");
			__state = __instance.health;
		}

		[HarmonyPostfix]
		// ReSharper disable once InconsistentNaming
		public static void PostDamagePlayer(PlayerControllerB __instance, int __state, CauseOfDeath causeOfDeath)
		{
			if (__instance.isPlayerDead || !__instance.IsOwner) return;
			if (!Plugin.ConfigVibrateOnDamage.Value) return;

			var damage = __state - __instance.health;
			if (damage <= 0) return;

			var intensity = Plugin.ConfigMaxIntensity.Value * damage / 100;
			var op = causeOfDeath == CauseOfDeath.Electrocution ? 0 : 1;

			Plugin.SendPost(intensity, 1, op);

			var type = causeOfDeath == CauseOfDeath.Electrocution ? "shock" : "vibration";
			Plugin.Log.LogInfo(
				$"Player {__instance.playerUsername} has taken {damage} damage, sending {type} with intensity {intensity}!");
		}
	}
}