using GorillaLocomotion;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GTPlayer), "ApplyClampedKnockback")]
internal class GuardianClampedKnockbackPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
