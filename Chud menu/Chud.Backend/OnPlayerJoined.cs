using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
internal class OnPlayerJoined : HarmonyPatch
{
	private static void Prefix(Player newPlayer)
	{
		NotifiLib.SendNotification("[<color=blue>ROOM</color>] Player: " + newPlayer.NickName + " Joined Lobby");
		Mods.ARSCheckPlayer(newPlayer);
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}
