using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(GameObject))]
[HarmonyPatch("CreatePrimitive")]
internal class MenuFixer
{
	private static Shader _cachedShader;

	private static void Postfix(GameObject __result)
	{
		if (_cachedShader == null)
			_cachedShader = Shader.Find("GorillaTag/UberShader");
		if (_cachedShader != null)
			__result.GetComponent<Renderer>().material.shader = _cachedShader;
	}
}
