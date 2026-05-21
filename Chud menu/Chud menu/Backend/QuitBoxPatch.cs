using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(GorillaQuitBox), "OnBoxTriggered")]
    internal class QuitBoxPatch
    {
        public static bool enabled = true;

        public static bool Prefix() => enabled;
    }
}
