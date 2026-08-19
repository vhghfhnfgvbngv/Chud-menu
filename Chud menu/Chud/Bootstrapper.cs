using Chud.Backend;
using Chud.UI;
using ExitGames.Client.Photon;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using UnityEngine;
using static Chud.PluginInfo;

namespace Chud;

public static class Bootstrapper
{
	private static bool patched;

	public static void Patch()
	{
		if (patched)
			return;
		Harmony val = new Harmony(GUID);
		val.PatchAll();
		MethodInfo opRaiseEvent = typeof(LoadBalancingClient).GetMethod("OpRaiseEvent", BindingFlags.Public | BindingFlags.Instance, null,
			new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) }, null);
		MethodInfo prefix = typeof(RPCProtection).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
		if (opRaiseEvent != null && prefix != null)
			val.Patch(opRaiseEvent, new HarmonyMethod(prefix));
		patched = true;
	}

	public static void Initialize()
	{
		if ((Object)(object)GameObject.Find("Chud_Init") != (Object)null)
			return;
		GameObject go = new GameObject("Chud_Init");
		go.AddComponent<WristMenu>();
		go.AddComponent<Mods>();
		go.AddComponent<NetworkManager>();
		go.AddComponent<NotifiLib>();
		go.AddComponent<CustomPropSetter>();
		go.AddComponent<Console>();
		Object.DontDestroyOnLoad((Object)(object)go);
	}
}
