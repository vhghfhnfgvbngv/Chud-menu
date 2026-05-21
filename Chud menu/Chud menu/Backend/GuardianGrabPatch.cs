using GorillaLocomotion;
using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.DoLaunch))]
    internal class GuardianLaunchPatch
    {
        public static bool enabled = false;
        public static bool Prefix() => !enabled;
    }

    [HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.ApplyKnockback))]
    internal class GuardianKnockbackPatch
    {
        public static bool enabled = false;
        public static bool Prefix() => !enabled;
    }

    [HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.ApplyClampedKnockback))]
    internal class GuardianClampedKnockbackPatch
    {
        public static bool enabled = false;
        public static bool Prefix() => !enabled;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.ApplyLocalTrajectoryOverride))]
    internal class GuardianTrajectoryPatch
    {
        public static bool enabled = false;
        public static bool Prefix() => !enabled;
    }

    [HarmonyPatch(typeof(VRRig), "GrabbedByPlayer")]
    internal class GuardianGrabbedByPatch
    {
        public static bool enabled = false;
        public static bool Prefix() => !enabled;
    }

    [HarmonyPatch(typeof(VRRig), "DroppedByPlayer")]
    internal class GuardianDroppedByPatch
    {
        public static bool Prefix() => !GuardianGrabbedByPatch.enabled;
    }
}
