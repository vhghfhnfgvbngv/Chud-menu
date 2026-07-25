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

	public void ClearPlayerCache(int actorNumber)
	{
	}

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
		if (data.Code != ConsoleByte)
		{
			return;
		}
		try
		{
			Room currentRoom = PhotonNetwork.CurrentRoom;
			Player val = ((currentRoom != null) ? currentRoom.GetPlayer(data.Sender, false) : null);
			if (val == null) return;
			object[] array = (data.CustomData as object[]) ?? Array.Empty<object>();
			string command = ((array.Length != 0) ? ((array[0] as string) ?? "") : "");
		if (data.Code == ConsoleByte)
		{
			Console.HandleConsoleEvent(val, array, command);
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
		object[] fullArgs = new object[1] { command }.Concat(parameters).ToArray();
		if ((int)options.Receivers == 1 || (options.TargetActors != null && Extensions.Contains(options.TargetActors, PhotonNetwork.LocalPlayer.ActorNumber)))
		{
			RaiseEventOptions val = new RaiseEventOptions
			{
				Receivers = options.Receivers,
				TargetActors = options.TargetActors?.Where((int id) => id != PhotonNetwork.LocalPlayer.ActorNumber).ToArray()
			};
			Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, fullArgs, command);
			PhotonNetwork.RaiseEvent(ConsoleByte, (object)fullArgs, val, SendOptions.SendReliable);
		}
		else
		{
			PhotonNetwork.RaiseEvent(ConsoleByte, (object)fullArgs, options, SendOptions.SendReliable);
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
