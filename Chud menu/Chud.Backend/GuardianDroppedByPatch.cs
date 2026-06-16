using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "DroppedByPlayer")]
internal class GuardianDroppedByPatch
{
	public static bool Prefix()
	{
		return !GuardianGrabbedByPatch.enabled;
	}
}
