using GorillaNetworking;
using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger), "OnBoxTriggered")]
    internal class NetworkTriggerPatch
    {
        public static bool enabled = false;

        public static bool Prefix() => !enabled;
    }
}
