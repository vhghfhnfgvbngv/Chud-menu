using HarmonyLib;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(LoadBalancingClient), nameof(LoadBalancingClient.OpRaiseEvent))]
public static class RPCProtection
{
	const int MaxRPCs = 500;
	static float startTime = Time.unscaledTime;
	static int rpcCount;

	static bool Prefix()
	{
		float currentTime = Time.unscaledTime;
		if (currentTime - startTime > 1f)
		{
			startTime = currentTime;
			rpcCount = 0;
		}
		rpcCount++;
		return rpcCount <= MaxRPCs;
	}
}
