using HarmonyLib;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "GetSlidePercentage")]
    public static class SlidePatch
    {
        public static bool everythingGrippy;
        public static void Postfix(GorillaLocomotion.GTPlayer __instance, ref float __result)
        {
            if (everythingGrippy) __result = 0f;
        }
    }
}
