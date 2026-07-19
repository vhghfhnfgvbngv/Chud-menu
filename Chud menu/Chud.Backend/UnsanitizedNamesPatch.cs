using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

internal static class ColorUtil
{
	internal static Color PlayerColor(VRRig rig)
	{
		if ((object)rig == null) return Color.white;
		Color c = rig.playerColor;
		if (c.r < 4f / 255f && c.g < 4f / 255f && c.b < 4f / 255f)
			return Color.white;
		return c;
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SetNameTagText")]
internal static class UnsanitizedChestPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			NetPlayer netPlayer = __instance.Creator;
			string rawName = (netPlayer != null) ? netPlayer.NickName : NetworkSystem.Instance.GetMyNickName();
			if (!string.IsNullOrEmpty(rawName))
				__instance.playerText1.text = rawName;
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SerializeReadShared")]
internal static class ChestColorPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("OnSubscriptionData")]
internal static class ChestColorSubPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
[HarmonyPatch("UpdatePlayerText")]
internal static class UnsanitizedBoardPatch
{
	[HarmonyPostfix]
	public static void Postfix(GorillaPlayerScoreboardLine __instance)
	{
		try
		{
			string rawName = __instance.linePlayer?.NickName;
			if (!string.IsNullOrEmpty(rawName))
				__instance.playerName.text = rawName;
			if ((Object)(object)__instance.playerVRRig != (Object)null)
				__instance.playerName.color = ColorUtil.PlayerColor(__instance.playerVRRig);
		}
		catch
		{
		}
	}
}