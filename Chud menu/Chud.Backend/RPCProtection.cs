using System.Collections.Generic;
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
	static Queue<float> eventTimes = new Queue<float>();

	static bool Prefix()
	{
		float now = Time.unscaledTime;
		while (eventTimes.Count > 0 && now - eventTimes.Peek() > 1f)
			eventTimes.Dequeue();
		if (eventTimes.Count >= MaxRPCs)
			return false;
		eventTimes.Enqueue(now);
		return true;
	}

	[HarmonyTargetMethod]
	static System.Reflection.MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(PhotonNetwork), nameof(PhotonNetwork.RaiseEvent),
			new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) });
	}
}
