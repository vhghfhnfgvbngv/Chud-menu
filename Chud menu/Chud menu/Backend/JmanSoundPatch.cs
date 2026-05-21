using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(VRRig), "PlayHandTapLocal")]
    internal class JmanSoundPatch
    {
        public static bool enabled = false;

        public static bool Prefix(VRRig __instance, int audioClipIndex)
        {
            if (!enabled) return true;
            if (__instance == VRRig.LocalRig) return true;
            return audioClipIndex < 336 || audioClipIndex > 338;
        }
    }
}