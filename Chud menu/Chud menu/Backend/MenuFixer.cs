using HarmonyLib;
using UnityEngine;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(GameObject))]
    [HarmonyPatch("CreatePrimitive", MethodType.Normal)]
    internal class MenuFixer
    {
        private static void Postfix(GameObject __result)
        {
            __result.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
        }
    }
}
