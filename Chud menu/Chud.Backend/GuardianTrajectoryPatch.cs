using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "ApplyLocalTrajectoryOverride")]
internal class GuardianTrajectoryPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
