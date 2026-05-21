using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace MalachiTemp.Backend
{
    internal class CustomPropSetter : UnityEngine.MonoBehaviour
    {
        private bool hasSet = false;

        void Update()
        {
            if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.IsConnectedAndReady)
            {
                if (!hasSet)
                {
                    SetProp();
                    hasSet = true;
                }
            }
            else
            {
                hasSet = false;
            }
        }

        public static void SetProp()
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["Chud menu"] = true;
            if (PhotonNetwork.LocalPlayer != null)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnJoinedRoom")]
    internal class OnJoinedRoom : HarmonyPatch
    {
        private static void Postfix()
        {
            CustomPropSetter.SetProp();
            Mods.ReapplyActiveMods();
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
    internal class OnPlayerJoined : HarmonyPatch
    {
        private static void Prefix(Player newPlayer)
        {
            NotifiLib.SendNotification("[<color=blue>ROOM</color>] Player: " + newPlayer.NickName + " Joined Lobby");
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
    internal class OnPlayerLeft : HarmonyPatch
    {
        private static void Prefix(Player otherPlayer)
        {
            if (otherPlayer != PhotonNetwork.LocalPlayer)
            {
                NotifiLib.SendNotification("[<color=blue>ROOM</color>] Player: " + otherPlayer.NickName + " Left Lobby");
            }
        }
    }
}
