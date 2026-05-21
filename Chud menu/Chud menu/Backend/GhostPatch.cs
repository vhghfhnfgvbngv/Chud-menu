using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(VRRig), "OnDisable")]
    internal class GhostPatch
    {
        public static bool Prefix(VRRig __instance)
        {
            return __instance != VRRig.LocalRig;
        }
    }

    [HarmonyPatch(typeof(VRRig), "Awake")]
    internal class RigAwakePatch
    {
        public static bool Prefix(VRRig __instance)
        {
            return __instance.gameObject.name != "Local Gorilla Player(Clone)";
        }
    }

    [HarmonyPatch(typeof(VRRigJobManager), "DeregisterVRRig")]
    public static class DeregisterPatch
    {
        public static bool Prefix(VRRigJobManager __instance, VRRig rig)
        {
            return rig != VRRig.LocalRig;
        }
    }

    [HarmonyPatch(typeof(VRRig), "PostTick")]
    public static class PostTickPatch
    {
        public static bool Prefix(VRRig __instance)
        {
            if (!__instance.isLocal) return true;
            return !(Mods.ghostMonkeOn || Mods.invisMonkeOn);
        }
    }
}
