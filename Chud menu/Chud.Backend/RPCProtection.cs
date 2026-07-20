using System.Collections.Generic;
using UnityEngine;

namespace Chud.Backend;

public static class RPCProtection
{
	const int MaxRPCs = 500;
	static Queue<float> eventTimes = new Queue<float>();

	public static bool Prefix()
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
