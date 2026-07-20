using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch]
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

	[HarmonyTargetMethod]
	static System.Reflection.MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(PhotonNetwork), nameof(PhotonNetwork.RaiseEvent),
			new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) });
	}
}
