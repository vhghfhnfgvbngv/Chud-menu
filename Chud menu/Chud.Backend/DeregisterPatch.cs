using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRigJobManager), "DeregisterVRRig")]
public static class DeregisterPatch
{
	public static bool Prefix(VRRigJobManager __instance, VRRig rig)
	{
		return (Object)(object)rig != (Object)(object)VRRig.LocalRig;
	}
}
