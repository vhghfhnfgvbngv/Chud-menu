using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "OnDisable")]
internal class GhostPatch
{
	public static bool Prefix(VRRig __instance)
	{
		return (Object)(object)__instance != (Object)(object)VRRig.LocalRig;
	}
}
