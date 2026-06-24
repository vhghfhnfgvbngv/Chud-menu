using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig))]
internal class UnsanitizedNamesPatch
{
	[HarmonyPatch("SetNameTagText")]
	[HarmonyPostfix]
	public static void SetNameTagText(VRRig __instance)
	{
		NetPlayer netPlayer = __instance.Creator;
		string rawName = (netPlayer != null) ? netPlayer.NickName : NetworkSystem.Instance.GetMyNickName();
		if (!string.IsNullOrEmpty(rawName))
		{
			__instance.playerText1.text = rawName;
		}
	}
}
