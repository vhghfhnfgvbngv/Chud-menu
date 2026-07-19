using HarmonyLib;
using Chud.UI;
using Photon.Pun;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnJoinedRoom")]
internal class OnJoinedRoom : HarmonyPatch
{
	private static void Postfix()
	{
		Mods.ReapplyActiveMods();
		Mods.TrackedCosmeticsScan();
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}
