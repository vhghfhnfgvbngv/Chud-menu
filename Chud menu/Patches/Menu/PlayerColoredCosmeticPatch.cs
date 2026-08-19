using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(PlayerColoredCosmetic), "Awake")]
internal static class PlayerColoredCosmeticPatch
{
	public static bool Prefix()
	{
		return !Mods.cloningGhostRig;
	}
}