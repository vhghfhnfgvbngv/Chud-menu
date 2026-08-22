using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "OnDisable")]
internal static class GhostRigOnDisablePatch
{
	public static bool Prefix(VRRig __instance)
	{
		if (__instance != null && __instance.gameObject != null && __instance.gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}

[HarmonyPatch(typeof(VRRig), "OnEnable")]
internal static class GhostRigOnEnablePatch
{
	public static bool Prefix(VRRig __instance)
	{
		if (__instance != null && __instance.gameObject != null && __instance.gameObject.name == "Chud_GhostRig")
		{
			try
			{
				return false;
			} catch { return false; }
		}
		return true;
	}
}

[HarmonyPatch(typeof(BodyDockPositions), "RefreshTransferrableItems")]
internal static class GhostBodyDockPatch
{
	public static bool Prefix(BodyDockPositions __instance)
	{
		if (__instance != null && __instance.GetComponentInParent<VRRig>() != null && __instance.GetComponentInParent<VRRig>().gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}

[HarmonyPatch(typeof(VRRigCollection), "OnRigTriggerEnter")]
internal static class GhostVRRigCollectionPatch
{
	public static bool Prefix(Collider other)
	{
		if (other != null)
		{
			VRRig rig = other.GetComponentInParent<VRRig>();
			if (rig != null && rig.gameObject.name == "Chud_GhostRig")
				return false;
		}
		return true;
	}
}
