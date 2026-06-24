using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SetNameTagText")]
internal static class UnsanitizedChestPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		NetPlayer netPlayer = __instance.Creator;
		string rawName = (netPlayer != null) ? netPlayer.NickName : NetworkSystem.Instance.GetMyNickName();
		if (!string.IsNullOrEmpty(rawName))
			__instance.playerText1.text = rawName;
	}
}

[HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
[HarmonyPatch("UpdatePlayerText")]
internal static class UnsanitizedBoardPatch
{
	[HarmonyPostfix]
	public static void Postfix(GorillaPlayerScoreboardLine __instance)
	{
		string rawName = __instance.linePlayer?.NickName;
		if (!string.IsNullOrEmpty(rawName))
			__instance.playerName.text = rawName;
	}
}