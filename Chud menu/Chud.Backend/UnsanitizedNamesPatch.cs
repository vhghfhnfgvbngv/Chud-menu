using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

internal static class ColorUtil
{
	internal static Color PlayerColor(VRRig rig)
	{
		Color c = rig.playerColor;
		float epsilon = 0.0005f;
		if (c.r == 0f && c.g == 0f && c.b == 0f)
			c = Color.white;
		else if (Mathf.Abs(c.r - 1f / 255f) < epsilon && Mathf.Abs(c.g - 1f / 255f) < epsilon && Mathf.Abs(c.b - 1f / 255f) < epsilon)
			c = Color.white;
		else if (Mathf.Abs(c.r - 2f / 255f) < epsilon && Mathf.Abs(c.g - 2f / 255f) < epsilon && Mathf.Abs(c.b - 2f / 255f) < epsilon)
			c = Color.white;
		else if (Mathf.Abs(c.r - 3f / 255f) < epsilon && Mathf.Abs(c.g - 3f / 255f) < epsilon && Mathf.Abs(c.b - 3f / 255f) < epsilon)
			c = Color.white;
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