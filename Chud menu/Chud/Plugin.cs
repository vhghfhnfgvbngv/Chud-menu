using BepInEx;
using Chud.Backend;
using Chud.UI;
using ExitGames.Client.Photon;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Realtime;
using System.Reflection;
using UnityEngine;

namespace Chud;

	[BepInPlugin("chudmenu", "chudmenu", "1.8.6")]
	public class Plugin : BaseUnityPlugin
	{
		public const string Name = "chudmenu";

		public const string GUID = "chudmenu";

		public const string Version = "1.8.6";

		private static bool loaded;

		private void Awake()
		{
			if (!loaded)
			{
				Harmony val = new Harmony("chudmenu");
				val.PatchAll();
				MethodInfo opRaiseEvent = typeof(LoadBalancingClient).GetMethod("OpRaiseEvent", BindingFlags.Public | BindingFlags.Instance, null,
					new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) }, null);
				MethodInfo prefix = typeof(RPCProtection).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
				if (opRaiseEvent != null && prefix != null)
					val.Patch(opRaiseEvent, new HarmonyMethod(prefix));
				loaded = true;
			}
		}

		private void Start()
		{
			if (!((Object)(object)GameObject.Find("Chud_Init") != (Object)null))
			{
				GameObject val = new GameObject("Chud_Init");
				val.AddComponent<WristMenu>();
				val.AddComponent<Mods>();
				val.AddComponent<NetworkManager>();
				val.AddComponent<NotifiLib>();
				val.AddComponent<CustomPropSetter>();
				val.AddComponent<Console>();
				Object.DontDestroyOnLoad((Object)(object)val);
			}
		}
	}
