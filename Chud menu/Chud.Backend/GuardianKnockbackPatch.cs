using GorillaLocomotion;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GTPlayer), "ApplyKnockback")]
internal class GuardianKnockbackPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
