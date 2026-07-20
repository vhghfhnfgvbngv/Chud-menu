using System.Collections.Generic;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(LoadBalancingClient), nameof(LoadBalancingClient.OpRaiseEvent),
	new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) })]
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
}
