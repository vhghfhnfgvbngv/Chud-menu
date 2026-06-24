using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
internal class OnPlayerLeft : HarmonyPatch
{
	private static void Prefix(Player otherPlayer)
	{
		if (otherPlayer != PhotonNetwork.LocalPlayer)
		{
			NotifiLib.SendNotification("[<color=blue>ROOM</color>] Player: " + otherPlayer.NickName + " Left Lobby");
		}
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}
