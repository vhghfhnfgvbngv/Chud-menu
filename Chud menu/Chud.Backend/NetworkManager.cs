using System;
using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaTag;
using GTAG_NotificationLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.Backend;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager instance;

	public const byte ConsoleByte = 68;

	public const byte ChudByte = 69;

	private static readonly Queue<LineRenderer> laserLinePool = new Queue<LineRenderer>();

	private const int MAX_LASER_POOL_SIZE = 20;

	private void Awake()
	{
		instance = this;
		PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
	}

	private void OnDestroy()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
		ClearLaserPool();
	}

	public static bool IsValidPlayer(Player player)
	{
		return player != null && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players.ContainsKey(player.ActorNumber);
	}

	private void OnEventReceived(EventData data)
	{
		if (data.Code == 8 && Mods.seeAntiCheatReports)
		{
			HandleAntiCheatReport(data);
			return;
		}
		if (data.Code != 68 && data.Code != 69)
		{
			return;
		}
		try
		{
			Room currentRoom = PhotonNetwork.CurrentRoom;
			Player val = ((currentRoom != null) ? currentRoom.GetPlayer(data.Sender, false) : null);
			if (val != null)
			{
				object[] array = (data.CustomData as object[]) ?? Array.Empty<object>();
				string command = ((array.Length != 0) ? ((array[0] as string) ?? "") : "");
				if (data.Code == 68)
				{
					Console.HandleConsoleEvent(val, array, command);
				}
				else if (data.Code == 69)
				{
					HandleChudEvent(val, array, command);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[NetworkManager] Event error: " + ex.Message));
		}
	}

	private static void HandleAntiCheatReport(EventData data)
	{
		try
		{
			object[] array = (data.CustomData as object[]) ?? Array.Empty<object>();
			if (array.Length < 6)
			{
				return;
			}
			string reportedName = (array[4] as string) ?? "?";
			string reason = (array[5] as string) ?? "?";
			string key = reason + "_" + reportedName;
			if (Mods.antiCheatReportCounts.TryGetValue(key, out var count))
			{
				Mods.antiCheatReportCounts[key] = count + 1;
				NotifiLib.SendNotification("[<color=red>ANTI-CHEAT</color>] " + reason + " — " + reportedName + " <color=yellow>" + (count + 1) + "x</color>");
			}
			else
			{
				Mods.antiCheatReportCounts[key] = 1;
				NotifiLib.SendNotification("[<color=red>ANTI-CHEAT</color>] " + reason + " — " + reportedName);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[NetworkManager] AntiCheat report error: " + ex.Message));
		}
	}

	private static void HandleChudEvent(Player sender, object[] args, string command)
	{
		switch (command)
		{
		case "chudmenu_state":
		{
			if (args.Length < 9)
			{
				break;
			}
			string category = (args[1] as string) ?? "Main";
			int page = (int)args[2];
			int colorIdx = (int)args[3];
			Vector3 pos = (Vector3)args[4];
			Quaternion rot = (Quaternion)((args.Length > 5) ? ((Quaternion)args[5]) : Quaternion.identity);
			bool remoteAnimationsEnabled = args.Length <= 6 || (bool)args[6];
			long mask0 = Convert.ToInt64(args[7]);
			long mask1 = Convert.ToInt64(args[8]);
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			int num = 0;
			foreach (MenuCategory cat in MenuManager.Categories)
			{
				if (cat.Buttons == null)
				{
					continue;
				}
				if (cat.Name == "Sound" || cat.Name == "Video")
				{
					continue;
				}
				foreach (ButtonInfo button in cat.Buttons)
				{
					if (button.nontoggleable != true)
					{
						long mask = ((num < 64) ? mask0 : mask1);
						int bit = (num < 64) ? num : (num - 64);
						dictionary[button.buttonText] = (mask & (1L << bit)) != 0L;
						num++;
					}
				}
			}
			Mods.ReceiveRemoteMenuState(sender, category, page, colorIdx, pos, rot, dictionary, remoteAnimationsEnabled);
			break;
		}
		case "chudmenu_pos":
			if (args.Length >= 3)
			{
				Mods.ReceiveRemoteMenuPosition(sender, (Vector3)args[1], (Quaternion)args[2]);
			}
			break;
		case "chudobject":
			if (args.Length >= 8)
			{
				Mods.ReceiveRemoteObject((args[1] as string) ?? "cube", (Vector3)args[2], (Vector3)args[3], (Vector3)args[4], new Color((float)args[5], (float)args[6], (float)args[7]));
			}
			break;
		case "chudplat_create":
			if (args.Length >= 10)
			{
				Mods.ReceiveRemotePlatformSpawn(sender.ActorNumber, (args[1] as string) ?? "", (Vector3)args[2], (Quaternion)args[3], (Vector3)args[4], new Color((float)args[5], (float)args[6], (float)args[7]), (bool)args[8], (bool)args[9]);
			}
			break;
		case "chudplat_destroy":
			if (args.Length >= 2)
			{
				Mods.ReceiveRemotePlatformDestroy(sender.ActorNumber, (args[1] as string) ?? "");
			}
			break;
		case "chudmenu_close":
			Mods.ReceiveRemoteMenuClose(sender);
			break;
		case "chudmenu_click":
			Mods.ReceiveRemoteButtonClick(sender, (int)args[1], (bool)args[2], (int)args[3]);
			break;
		}
	}

	public static void StopEventHandling()
	{
		if ((Object)(object)instance != (Object)null)
		{
			PhotonNetwork.NetworkingClient.EventReceived -= instance.OnEventReceived;
		}
	}

	#region Console command sender (used)

	public static void SendConsoleCommand(string command, RaiseEventOptions options, params object[] parameters)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		if ((int)options.Receivers == 1 || (options.TargetActors != null && Extensions.Contains(options.TargetActors, PhotonNetwork.LocalPlayer.ActorNumber)))
		{
			RaiseEventOptions val = new RaiseEventOptions
			{
				Receivers = options.Receivers,
				TargetActors = options.TargetActors?.Where((int id) => id != PhotonNetwork.LocalPlayer.ActorNumber).ToArray()
			};
			object[] args = new object[1] { command }.Concat(parameters).ToArray();
			Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, args, command);
			PhotonNetwork.RaiseEvent((byte)68, (object)new object[1] { command }.Concat(parameters).ToArray(), val, SendOptions.SendReliable);
		}
		else
		{
			PhotonNetwork.RaiseEvent((byte)68, (object)new object[1] { command }.Concat(parameters).ToArray(), options, SendOptions.SendReliable);
		}
	}

	#endregion

	public static LineRenderer GetLaserLine()
	{
		while (laserLinePool.Count > 0)
		{
			LineRenderer val = laserLinePool.Dequeue();
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
		}
		return null;
	}

	public static void ReturnLaserLine(LineRenderer l)
	{
		if ((Object)(object)l != (Object)null && laserLinePool.Count < 20)
		{
			laserLinePool.Enqueue(l);
		}
		else if ((Object)(object)l != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)l).gameObject);
		}
	}

	private static void ClearLaserPool()
	{
		foreach (LineRenderer item in laserLinePool)
		{
			if ((Object)(object)item != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		laserLinePool.Clear();
	}
}
