using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(GameObject))]
[HarmonyPatch("CreatePrimitive")]
internal class MenuFixer
{
	private static void Postfix(GameObject __result)
	{
		__result.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
	}
}
