using System;
using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaTag;
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

	private static readonly Dictionary<int, Dictionary<string, bool>> lastSentButtonStates = new Dictionary<int, Dictionary<string, bool>>();

	private static readonly Dictionary<int, float> lastSentPositionTime = new Dictionary<int, float>();

	private static readonly Queue<LineRenderer> laserLinePool = new Queue<LineRenderer>();

	private const int MAX_LASER_POOL_SIZE = 20;

	private void Awake()
	{
		instance = this;
		PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
		NetworkSystem obj = NetworkSystem.Instance;
		obj.OnPlayerLeft = (DelegateListProcessorPlusMinus<DelegateListProcessor<NetPlayer>, Action<NetPlayer>>)(object)obj.OnPlayerLeft + (Action<NetPlayer>)OnPlayerLeftRoom;
	}

	private void OnDestroy()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
		NetworkSystem obj = NetworkSystem.Instance;
		obj.OnPlayerLeft = (DelegateListProcessorPlusMinus<DelegateListProcessor<NetPlayer>, Action<NetPlayer>>)(object)obj.OnPlayerLeft - (Action<NetPlayer>)OnPlayerLeftRoom;
		ClearLaserPool();
	}

	public static bool IsValidPlayer(Player player)
	{
		return player != null && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players.ContainsKey(player.ActorNumber);
	}

	private void OnEventReceived(EventData data)
	{
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

	private static void HandleChudEvent(Player sender, object[] args, string command)
	{
		switch (command)
		{
		case "chudmenu_state":
		{
			if (args.Length < 5)
			{
				break;
			}
			string category = (args[1] as string) ?? "Main";
			int page = (int)args[2];
			int colorIdx = (int)args[3];
			Vector3 pos = (Vector3)args[4];
			Quaternion rot = (Quaternion)((args.Length > 5) ? ((Quaternion)args[5]) : Quaternion.identity);
			bool remoteAnimationsEnabled = args.Length <= 6 || (bool)args[6];
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			for (int i = 7; i + 1 < args.Length; i += 2)
			{
				if (args[i] is string key && args[i + 1] is int num)
				{
					dictionary[key] = num == 1;
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
		case "chudgun_update":
			if (args.Length >= 6)
			{
				Mods.ReceiveRemoteGunUpdate(sender.ActorNumber, (Vector3)args[1], (Vector3)args[2], new Color((float)args[3], (float)args[4], (float)args[5]));
			}
			break;
		case "chudgun_hide":
			Mods.ReceiveRemoteGunHide(sender.ActorNumber);
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

	public static void SendMenuFullState()
	{
		if (!Mods.NetworkMenuEnabled || !PhotonNetwork.InRoom)
		{
			return;
		}
		string currentCategoryName = MenuManager.CurrentCategoryName;
		int pageNumber = WristMenu.pageNumber;
		Vector3 menuPosition = Mods.GetMenuPosition();
		Quaternion menuRotation = Mods.GetMenuRotation();
		List<object> list = new List<object>
		{
			"chudmenu_state",
			currentCategoryName,
			pageNumber,
			Mods.menuColorIndex,
			menuPosition,
			menuRotation,
			WristMenu.animationsEnabled
		};
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Buttons == null)
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.nontoggleable != true)
				{
					list.Add(button.buttonText);
					list.Add((button.enabled == true) ? 1 : 0);
				}
			}
		}
		PhotonNetwork.RaiseEvent((byte)69, (object)list.ToArray(), new RaiseEventOptions
		{
			Receivers = (ReceiverGroup)0
		}, SendOptions.SendReliable);
	}

	public static void SendMenuPosition()
	{
		if (Mods.NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			Vector3 menuPosition = Mods.GetMenuPosition();
			Quaternion menuRotation = Mods.GetMenuRotation();
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[3] { "chudmenu_pos", menuPosition, menuRotation }, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			}, SendOptions.SendUnreliable);
		}
	}

	public static void SendMenuClose()
	{
		if (Mods.NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[1] { "chudmenu_close" }, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			}, SendOptions.SendReliable);
		}
	}

	public static void SendButtonClick()
	{
		if (Mods.NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[4]
			{
				"chudmenu_click",
				Mods.ButtonSound,
				Mods.right,
				WristMenu.buttonSoundIndex
			}, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			}, SendOptions.SendReliable);
		}
	}

	public static void SendPlatformSpawn(Vector3 pos, Quaternion rot, Vector3 scale, Color color, bool invis, bool sticky, string hand)
	{
		if (Mods.NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[10] { "chudplat_create", hand, pos, rot, scale, color.r, color.g, color.b, invis, sticky }, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			}, SendOptions.SendReliable);
		}
	}

	public static void SendPlatformDestroy(string hand)
	{
		if (Mods.NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[2] { "chudplat_destroy", hand }, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			}, SendOptions.SendReliable);
		}
	}

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

	private static void OnPlayerLeftRoom(NetPlayer player)
	{
		Player playerRef = player.GetPlayerRef();
		int num = ((playerRef != null) ? playerRef.ActorNumber : (-1));
		if (num >= 0)
		{
			lastSentButtonStates.Remove(num);
			lastSentPositionTime.Remove(num);
		}
	}

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
