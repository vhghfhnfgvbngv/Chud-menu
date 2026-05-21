using GorillaNetworking;
using HarmonyLib;
using PlayFab;
using PlayFab.CloudScriptModels;
using System;

namespace MalachiTemp.Backend
{
    public static class BanPatchState
    {
        public static bool enabled = true;
    }

    [HarmonyPatch(typeof(GorillaServer), nameof(GorillaServer.CheckForBadName))]
    internal class AntiBanPlayfabPatch
    {
        public static bool Prefix(CheckForBadNameRequest request, Action<ExecuteFunctionResult> successCallback, Action<PlayFabError> errorCallback)
        {
            if (!BanPatchState.enabled) return true;
            successCallback?.Invoke(new ExecuteFunctionResult { FunctionResult = new PlayFab.Json.JsonObject { { "result", 0 } } });
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaComputer), nameof(GorillaComputer.CheckAutoBanListForName))]
    internal class AntiBanListPatch
    {
        public static bool Prefix(string nameToCheck, ref bool __result)
        {
            if (!BanPatchState.enabled) return true;
            __result = true;
            return false;
        }
    }

}
