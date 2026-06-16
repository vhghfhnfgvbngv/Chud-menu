using BepInEx;
using Chud.Backend;
using Chud.UI;
using GTAG_NotificationLib;
using HarmonyLib;
using UnityEngine;

namespace Chud;

[BepInPlugin("chudmenu", "chudmenu", "1.4.7")]
public class Plugin : BaseUnityPlugin
{
	public const string Name = "chudmenu";

	public const string GUID = "chudmenu";

	public const string Version = "1.4.7";

	private static bool loaded;

	private void Awake()
	{
		if (!loaded)
		{
			Harmony val = new Harmony("chudmenu");
			val.PatchAll();
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
			val.AddComponent<ConsoleIntegration>();
			Object.DontDestroyOnLoad((Object)(object)val);
		}
	}
}
