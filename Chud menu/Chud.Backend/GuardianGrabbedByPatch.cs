using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "GrabbedByPlayer")]
internal class GuardianGrabbedByPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
