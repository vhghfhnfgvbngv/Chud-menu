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

	private const int MAX_CHUD_EVENTS_PER_SEC = 50;

	private const float CHUD_CACHE_DURATION = 30f;

	private readonly Dictionary<int, Queue<float>> _eventTimestamps = new Dictionary<int, Queue<float>>();

	private readonly Dictionary<int, float> _chudCheckTime = new Dictionary<int, float>();

	private readonly Dictionary<int, bool> _chudCache = new Dictionary<int, bool>();

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

	private bool IsChudSender(Player player)
	{
		if (player == null) return false;
		int actor = player.ActorNumber;
		float now = Time.time;
		if (_chudCheckTime.TryGetValue(actor, out var last) && now - last < CHUD_CACHE_DURATION)
			return _chudCache.TryGetValue(actor, out var cached) && cached;
		bool result = player.CustomProperties.TryGetValue("Chud menu", out var val) && val is bool b && b;
		_chudCheckTime[actor] = now;
		_chudCache[actor] = result;
		return result;
	}

	private bool IsRateLimited(int actor)
	{
		float now = Time.time;
		if (!_eventTimestamps.TryGetValue(actor, out var queue))
		{
			queue = new Queue<float>();
			_eventTimestamps[actor] = queue;
		}
		while (queue.Count > 0 && now - queue.Peek() > 1f)
			queue.Dequeue();
		if (queue.Count >= MAX_CHUD_EVENTS_PER_SEC)
			return true;
		queue.Enqueue(now);
		return false;
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
			if (val == null) return;
			object[] array = (data.CustomData as object[]) ?? Array.Empty<object>();
			string command = ((array.Length != 0) ? ((array[0] as string) ?? "") : "");
			if (data.Code == 68)
			{
				Console.HandleConsoleEvent(val, array, command);
			}
			else if (data.Code == 69)
			{
				if (!IsChudSender(val)) return;
				if (IsRateLimited(val.ActorNumber)) return;
				HandleChudEvent(val, array, command);
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
			if (args.Length < 9) break;
			string category = (args[1] as string) ?? "Main";
			if (args[2] is not int page || page < 0 || page > 100) break;
			if (args[3] is not int colorIdx || colorIdx < 0 || colorIdx > 50) break;
			if (args[4] is not Vector3 pos) break;
			if (float.IsNaN(pos.x) || float.IsInfinity(pos.x)) break;
			if (float.IsNaN(pos.y) || float.IsInfinity(pos.y)) break;
			if (float.IsNaN(pos.z) || float.IsInfinity(pos.z)) break;
			Quaternion rot = Quaternion.identity;
			if (args.Length > 5 && args[5] is Quaternion qrot)
			{
				if (float.IsNaN(qrot.x) || float.IsInfinity(qrot.x)) break;
				if (float.IsNaN(qrot.y) || float.IsInfinity(qrot.y)) break;
				if (float.IsNaN(qrot.z) || float.IsInfinity(qrot.z)) break;
				if (float.IsNaN(qrot.w) || float.IsInfinity(qrot.w)) break;
				rot = qrot;
			}
			if (args[6] is not bool remoteAnimationsEnabled) remoteAnimationsEnabled = true;
			if (args[7] is not long mask0)
			{
				if (args[7] is int) mask0 = Convert.ToInt64(args[7]);
				else break;
			}
			if (args[8] is not long mask1)
			{
				if (args[8] is int) mask1 = Convert.ToInt64(args[8]);
				else break;
			}
			var states = new Dictionary<string, bool>();
			int idx = 0;
			foreach (MenuCategory cat in MenuManager.Categories)
			{
				if (cat.Buttons == null) continue;
				if (cat.Name == "Sound" || cat.Name == "Video") continue;
				foreach (ButtonInfo btn in cat.Buttons)
				{
					if (btn.nontoggleable != true)
					{
						long mask = (idx < 64) ? mask0 : mask1;
						int bit = (idx < 64) ? idx : (idx - 64);
						states[btn.buttonText] = (mask & (1L << bit)) != 0L;
						idx++;
					}
				}
			}
			Mods.ReceiveRemoteMenuState(sender, category, page, colorIdx, pos, rot, states, remoteAnimationsEnabled);
			break;
		}
		case "chudmenu_pos":
			if (args.Length >= 3 && args[1] is Vector3 && args[2] is Quaternion r2)
			{
				if (!float.IsNaN(r2.x) && !float.IsInfinity(r2.x) &&
					!float.IsNaN(r2.y) && !float.IsInfinity(r2.y) &&
					!float.IsNaN(r2.z) && !float.IsInfinity(r2.z) &&
					!float.IsNaN(r2.w) && !float.IsInfinity(r2.w))
					Mods.ReceiveRemoteMenuPosition(sender, (Vector3)args[1], r2);
			}
			break;
		case "chudmenu_heartbeat":
			Mods.ReceiveRemoteMenuHeartbeat(sender);
			break;
		case "chudobject":
			if (args.Length >= 8 && args[2] is Vector3 && args[3] is Vector3 && args[4] is Vector3)
			{
				float r = (args[5] is float fr) ? fr : 0f;
				float g = (args[6] is float fg) ? fg : 0f;
				float b = (args[7] is float fb) ? fb : 0f;
				r = Mathf.Clamp01(r); g = Mathf.Clamp01(g); b = Mathf.Clamp01(b);
				Mods.ReceiveRemoteObject((args[1] as string) ?? "cube", (Vector3)args[2], (Vector3)args[3], (Vector3)args[4], new Color(r, g, b));
			}
			break;
		case "chudmenu_close":
			Mods.ReceiveRemoteMenuClose(sender);
			break;
		case "chudmenu_click":
			if (args.Length >= 4 && args[1] is int sound && args[2] is bool rightClick && args[3] is int soundIdx)
				Mods.ReceiveRemoteButtonClick(sender, sound, rightClick, soundIdx);
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

	public static void SimulateLocalChudEvent(object[] args, string command)
	{
		Player self = PhotonNetwork.LocalPlayer;
		if (self != null)
			HandleChudEvent(self, args, command);
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
