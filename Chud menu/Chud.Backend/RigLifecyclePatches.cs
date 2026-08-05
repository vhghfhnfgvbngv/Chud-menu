using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "OnDisable")]
internal static class GhostPatch
{
	public static bool Prefix(VRRig __instance)
	{
		return (Object)(object)__instance != (Object)(object)VRRig.LocalRig;
	}
}

[HarmonyPatch(typeof(VRRigJobManager), "DeregisterVRRig")]
internal static class DeregisterPatch
{
	public static bool Prefix(VRRigJobManager __instance, VRRig rig)
	{
		return (Object)(object)rig != (Object)(object)VRRig.LocalRig;
	}
}
