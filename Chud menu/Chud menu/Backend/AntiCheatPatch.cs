using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(MonkeAgent), "SendReport")]
    internal class AnticheatPatch
    {
        public static bool showReports = true;

        private static bool Prefix(string susReason, string susId, string susNick)
        {
            if (showReports && susReason != "empty rig" && susId == PhotonNetwork.LocalPlayer.UserId)
            {
                NotifiLib.SendNotification("[<color=red>ANTICHEAT</color>] REPORTED FOR: " + susReason);
            }
            return false;
        }
    }
}
