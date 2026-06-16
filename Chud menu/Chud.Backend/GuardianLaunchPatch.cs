using GorillaLocomotion;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GTPlayer), "DoLaunch")]
internal class GuardianLaunchPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
