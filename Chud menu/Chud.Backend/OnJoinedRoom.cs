using HarmonyLib;
using Photon.Pun;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnJoinedRoom")]
internal class OnJoinedRoom : HarmonyPatch
{
	private static void Postfix()
	{
		Mods.ReapplyActiveMods();
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}
