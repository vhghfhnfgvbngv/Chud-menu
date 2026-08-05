using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "Awake")]
internal class RigAwakePatch
{
	public static bool Prefix(VRRig __instance)
	{
		return ((Object)((Component)__instance).gameObject).name != "Local Gorilla Player(Clone)";
	}
}
