using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using GTAG_NotificationLib;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal class WristMenu : MonoBehaviour
{
	public static string MenuTitle = "Chud Menu";

	public static Font MenuFont;

	public static Texture2D menuImage;

	public static AudioClip customButtonClick;

	public static int buttonSoundIndex = 0;

	private static AudioSource buttonClickAudioSource;

	private static bool MenuFontInitialized = false;

	private static bool customAudioLoaded = false;

	public static string[] CustomBoardTexts = new string[4] { "CHUD MENU USERS, LOOK HERE", "CHUD MENU", "Monkeys can climb. Crickets can leap. Horses can race. Owls can seek. Cheetahs can run. Eagles can fly. People can try. But that's about it.", "if u get banned with this, its on u, not me" };

	public static string FolderName = "Chud Menu";

	public static bool Close = false;

	private const float OPEN_ANIMATION_SPEED = 0.3f;

	private const float CLOSE_ANIMATION_SPEED = 0.3f;

	public static bool animationsEnabled = true;

	public static bool ChangingColors = false;

	public static Color FirstColor = Color.blue;

	public static Color NormalColor = new Color(0.15f, 0.15f, 0.15f);

	public static Color ButtonColorDisable = new Color(0.25f, 0.25f, 0.25f);

	public static Color ButtonColorEnabled = new Color(0.5f, 0.5f, 0.5f);

	public static Color EnableTextColor = Color.white;

	public static Color DIsableTextColor = new Color(0.75f, 0.75f, 0.75f);

	public static Color MenuTitleColor = Color.white;

	public static Color ToolTipColor = new Color(0.8f, 0.8f, 0.8f);

	public static Color DisconnectButtonColor = new Color(0.5f, 0f, 0f);

	public static Color DisconnectTextColor = Color.white;

	public static Color NextPrevButtonColor = new Color(0.15f, 0.15f, 0.15f);

	public static Color NextPrevTextColor = Color.white;

	public static bool roundedObjects = false;

	private static Dictionary<string, List<Renderer>> roundedRenderers = new Dictionary<string, List<Renderer>>();

	private const float bevelWidth = 0.02f;

	public static Vector3 PointerScale = new Vector3(0.01f, 0.01f, 0.01f);

	public static Vector3 PointerPos = new Vector3(0f, -0.1f, 0f);

	public static bool gripDownR;

	public static bool triggerDownR;

	public static bool abuttonDown;

	public static bool bbuttonDown;

	public static bool xbuttonDown;

	public static bool ybuttonDown;

	public static bool gripDownL;

	public static bool triggerDownL;

	public static Vector2 joy;

	public static Vector2 joyL;

	public static GameObject menu = null;

	private static float lastButtonPressTime = -1f;

	private const float buttonCooldown = 0.1f;

	public static GameObject canvasObj = null;

	public static GameObject reference = null;

	public static int pageNumber = 0;

	public static WristMenu instance;

	public static GameObject menuObj;

	public static Text fpsText;

	private static DateTime sessionStartTime = DateTime.Now;

	private static string bottomBarStr = "FPS: 0 | 12:00 AM | 0:00";

	private static float fpsAccumulator;

	private static int fpsFrameCount;

	private static int cachedFPS = 0;

	public static bool toggle = false;

	public static bool toggle1 = false;

	public static int pageSize = 4;

	public static int ClickCooldown = 10;

	public static bool custom = true;

	private static int _frameCounter;

	private static bool _adminInitialized;

	private static bool _mouseWasPressed;

	private static Camera _tpc;

	public static bool showFPS = true;

	public static bool showSessionTime = true;

	public static Text titiel;

	public static IEnumerator LoadMenuImage()
	{
		UnityWebRequest req = UnityWebRequestTexture.GetTexture(ServerData.MenuImageURL);
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result == 1)
			{
				menuImage = DownloadHandlerTexture.GetContent(req);
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static IEnumerator LoadCustomButtonClickAudio()
	{
		if (customAudioLoaded)
		{
			yield break;
		}
		string url = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/button%20click.mp3";
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, (AudioType)13);
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result == 1)
			{
				customButtonClick = DownloadHandlerAudioClip.GetContent(req);
				customAudioLoaded = true;
				NotifiLib.SendNotification("[<color=green>CHUD</color>] Custom button sound loaded", 2);
			}
			else
			{
				NotifiLib.SendNotification("[<color=red>CHUD</color>] Failed to load custom button sound", 2);
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static void PlayButtonClickSound(bool rightHand)
	{
		if (buttonSoundIndex == 1 && (Object)(object)customButtonClick != (Object)null)
		{
			if ((Object)(object)buttonClickAudioSource == (Object)null)
			{
				GameObject val = new GameObject("ChudButtonAudio");
				buttonClickAudioSource = val.AddComponent<AudioSource>();
				buttonClickAudioSource.spatialBlend = 0f;
				buttonClickAudioSource.playOnAwake = false;
				Object.DontDestroyOnLoad((Object)(object)val);
			}
			buttonClickAudioSource.PlayOneShot(customButtonClick, 0.5f);
		}
		else
		{
			VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, !rightHand, 0.1f);
		}
	}

	public static void InitMenuFont()
	{
		if (!MenuFontInitialized)
		{
			MenuFontInitialized = true;
			MenuFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 200);
			if ((Object)(object)MenuFont == (Object)null)
			{
				MenuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
		}
	}

	public static void InitCategories()
	{
		MenuManager.AddCategory("Main", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Settings",
				method = delegate
				{
					MenuManager.ToggleCategory("Settings");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Settings"
			},
			new ButtonInfo
			{
				buttonText = "Enabled Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Enabled Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "View enabled mods"
			},
			new ButtonInfo
			{
				buttonText = "Movement Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Movement Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Movement Mods"
			},
			new ButtonInfo
			{
				buttonText = "Visual Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Visual Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Visual Mods"
			},
			new ButtonInfo
			{
				buttonText = "Fun Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Fun Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Fun Mods"
			},
			new ButtonInfo
			{
				buttonText = "Useful Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Useful Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Useful Mods"
			},
			new ButtonInfo
			{
				buttonText = "Rig Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Rig Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Rig Mods"
			},
			new ButtonInfo
			{
				buttonText = "Infection Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Infection Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Infection Mods"
			},
			new ButtonInfo
			{
				buttonText = "Master Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Master Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Master Mods"
			},
			new ButtonInfo
			{
				buttonText = "Credits",
				method = delegate
				{
					MenuManager.ToggleCategory("Credits");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Credits"
			}
		});
		MenuManager.AddCategory("Settings", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Settings",
				method = delegate
				{
					MenuManager.ToggleCategory("Settings");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Change Menu Color",
				method = delegate
				{
					Mods.CycleMenuColor();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle menu color scheme"
			},
			new ButtonInfo
			{
				buttonText = "Menu Animations",
				method = delegate
				{
					animationsEnabled = true;
					Mods.AutoSave();
				},
				disableMethod = delegate
				{
					animationsEnabled = false;
					Mods.AutoSave();
				},
				enabled = true,
				toolTip = "Toggle menu open/close and button press animations"
			},
			new ButtonInfo
			{
				buttonText = "Rounded Menu",
				method = delegate
				{
					roundedObjects = true;
					DestroyMenu();
					instance.Draw();
				},
				disableMethod = delegate
				{
					roundedObjects = false;
					DestroyMenu();
					instance.Draw();
				},
				enabled = false,
				toolTip = "Round the menu corners"
			},
			new ButtonInfo
			{
				buttonText = "Right Hand",
				method = delegate
				{
					Mods.EnableRightHand();
				},
				disableMethod = delegate
				{
					Mods.DisableRightHand();
				},
				enabled = false,
				toolTip = "Move menu to right hand"
			},
			new ButtonInfo
			{
				buttonText = "Toggle Notifications",
				method = delegate
				{
					Mods.ToggleNotifications();
				},
				disableMethod = delegate
				{
					Mods.DisableNotifications();
				},
				enabled = true,
				toolTip = "Show/hide notifications"
			},
			new ButtonInfo
			{
				buttonText = "Clear Notifications",
				method = delegate
				{
					Mods.ClearNotifications();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Remove all on-screen notifications"
			},
			new ButtonInfo
			{
				buttonText = "Notification Time",
				method = delegate
				{
					Mods.CycleNotificationTime();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle how long notifications stay"
			},
			new ButtonInfo
			{
				buttonText = "Show FPS",
				method = delegate
				{
					showFPS = true;
				},
				disableMethod = delegate
				{
					showFPS = false;
				},
				enabled = true,
				toolTip = "Show FPS counter"
			},
			new ButtonInfo
			{
				buttonText = "Show Session Time",
				method = delegate
				{
					showSessionTime = true;
				},
				disableMethod = delegate
				{
					showSessionTime = false;
				},
				enabled = true,
				toolTip = "Show session duration"
			},
			new ButtonInfo
			{
				buttonText = "Change Button Click Sound",
				method = delegate
				{
					buttonSoundIndex = (buttonSoundIndex + 1) % 2;
					NotifiLib.SendNotification("[<color=green>CHUD</color>] Button sound: " + ((buttonSoundIndex == 0) ? "Normal" : "Custom"), 2);
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle button click sound"
			},
			new ButtonInfo
			{
				buttonText = "Change Speed Amount",
				method = delegate
				{
					Mods.ChangeSpeedBoostAmount();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle speed boost multiplier"
			},
			new ButtonInfo
			{
				buttonText = "Change Fly Speed",
				method = delegate
				{
					Mods.ChangeFlySpeed();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle fly speed (max 20)"
			},
			new ButtonInfo
			{
				buttonText = "Change WASD Sense",
				method = delegate
				{
					Mods.ChangeWASDFlyMouseSense();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle WASD fly sensitivity"
			},
			new ButtonInfo
			{
				buttonText = "Change Pull Power",
				method = delegate
				{
					Mods.ChangePullModPower();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Cycle pull mod strength"
			},
			new ButtonInfo
			{
				buttonText = "PC Guns",
				method = delegate
				{
					Mods.EnablePCGuns();
				},
				disableMethod = delegate
				{
					Mods.DisablePCGuns();
				},
				enabled = false,
				toolTip = "Use guns with mouse"
			},
			new ButtonInfo
			{
				buttonText = "Network Menu",
				method = delegate
				{
					Mods.EnableNetworkMenu();
				},
				disableMethod = delegate
				{
					Mods.DisableNetworkMenu();
				},
				enabled = false,
				toolTip = "Share menu across network"
			}
		});
		MenuManager.AddCategory("Enabled Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Enabled Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Enabled Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			}
		});
		MenuManager.AddCategory("Movement Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Movement Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Movement Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Fly",
				method = delegate
				{
					Mods.EnableFly();
				},
				disableMethod = delegate
				{
					Mods.DisableFly();
				},
				enabled = false,
				toolTip = "Hold B"
			},
			new ButtonInfo
			{
				buttonText = "Joystick Fly",
				method = delegate
				{
					Mods.JoystickFly();
				},
				disableMethod = delegate
				{
					Mods.DisableJoystickFly();
				},
				enabled = false,
				toolTip = "Fly with joystick"
			},
			new ButtonInfo
			{
				buttonText = "WASD Fly",
				method = delegate
				{
					Mods.EnableWASDFly();
				},
				disableMethod = delegate
				{
					Mods.DisableWASDFly();
				},
				enabled = false,
				toolTip = "Fly with WASD keys"
			},
			new ButtonInfo
			{
				buttonText = "Speed Boost",
				method = delegate
				{
					Mods.SpeedBoost();
				},
				disableMethod = delegate
				{
					Mods.DisableSpeedBoost();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Faster movement"
			},
			new ButtonInfo
			{
				buttonText = "No Gravity",
				method = delegate
				{
					Mods.NoGravity();
				},
				disableMethod = delegate
				{
					Mods.DisableNoGravity();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Disable gravity"
			},
			new ButtonInfo
			{
				buttonText = "Pull Mod",
				method = delegate
				{
					Mods.PullMod();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Pull forward while gripping"
			},
			new ButtonInfo
			{
				buttonText = "Noclip",
				method = delegate
				{
					Mods.Noclip();
				},
				disableMethod = delegate
				{
					Mods.NoclipOff();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Hold B to noclip"
			},
			new ButtonInfo
			{
				buttonText = "Platforms",
				method = delegate
				{
					Mods.Platforms();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Place platforms"
			},
			new ButtonInfo
			{
				buttonText = "TP Gun",
				method = delegate
				{
					Mods.TPGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot to teleport"
			},
			new ButtonInfo
			{
				buttonText = "Minos Prime",
				method = delegate
				{
					Mods.MinosPrime();
				},
				disableMethod = delegate
				{
					Mods.DisableMinosPrime();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Right B to jump, then Right A to slam"
			},
			new ButtonInfo
			{
				buttonText = "Teleport to Stump",
				method = delegate
				{
					Mods.TeleportToSpawn();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Teleport to the forest stump"
			}
		});
		MenuManager.AddCategory("Visual Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Visual Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Visual Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Name Tags",
				method = delegate
				{
					Mods.NameTags();
				},
				disableMethod = delegate
				{
					Mods.DisableNameTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show names above heads"
			},
			new ButtonInfo
			{
				buttonText = "ID Name Tags",
				method = delegate
				{
					Mods.IDTags();
				},
				disableMethod = delegate
				{
					Mods.DisableIDTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show IDs above heads"
			},
			new ButtonInfo
			{
				buttonText = "FPS Name Tags",
				method = delegate
				{
					Mods.FPSTags();
				},
				disableMethod = delegate
				{
					Mods.DisableFPSTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show FPS above heads"
			},
			new ButtonInfo
			{
				buttonText = "Platform Name Tags",
				method = delegate
				{
					Mods.PlatformTags();
				},
				disableMethod = delegate
				{
					Mods.DisablePlatformTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show platform above heads"
			},
			new ButtonInfo
			{
				buttonText = "Cosmetic Name Tags",
				method = delegate
				{
					Mods.CosmeticNameTags();
				},
				disableMethod = delegate
				{
					Mods.DisableCosmeticNameTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show cosmetics above heads"
			},
			new ButtonInfo
			{
				buttonText = "ARS Nametags",
				method = delegate
				{
					Mods.EnableARSNameTags();
				},
				disableMethod = delegate
				{
					Mods.DisableARSNameTags();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Show ARS players"
			},
			new ButtonInfo
			{
				buttonText = "Tracers",
				method = delegate
				{
					Mods.Tracers();
				},
				disableMethod = delegate
				{
					Mods.DisableTracers();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Lines towards everyone"
			},
			new ButtonInfo
			{
				buttonText = "2D Box ESP",
				method = delegate
				{
					Mods.BoxEspRender();
				},
				disableMethod = delegate
				{
					Mods.DisableBoxEsp();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Boxes around players"
			},
			new ButtonInfo
			{
				buttonText = "Random Color Spaz",
				method = delegate
				{
					Mods.RandomColorSpaz();
				},
				disableMethod = delegate
				{
					Mods.DisableRandomColorSpaz();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Cycle through colors rapidly"
			},
			new ButtonInfo
			{
				buttonText = "3rd Person",
				method = delegate
				{
					Mods.EnableThirdPerson();
				},
				disableMethod = delegate
				{
					Mods.DisableThirdPerson();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Third person view"
			},
			new ButtonInfo
			{
				buttonText = "Cosmetic Notifier",
				method = delegate
				{
					Mods.CosmeticNotifier();
				},
				disableMethod = delegate
				{
					Mods.DisableCosmeticNotifier();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Alert on tracked cosmetics"
			},
			new ButtonInfo
			{
				buttonText = "Skeleton ESP",
				method = delegate
				{
					Mods.SkeletonEsp();
				},
				disableMethod = delegate
				{
					Mods.DisableSkeletonEsp();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Draw skeleton lines on players"
			}
		});
		MenuManager.AddCategory("Useful Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Useful Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Useful Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Anti Name Ban",
				method = delegate
				{
					Mods.AntiNameBan();
				},
				disableMethod = delegate
				{
					Mods.DisableAntiNameBan();
				},
				enabled = true,
				toolTip = "Prevent name bans"
			},
			new ButtonInfo
			{
				buttonText = "Anti AFK",
				method = delegate
				{
					Mods.AntiAFK();
				},
				disableMethod = delegate
				{
					Mods.DisableAntiAFK();
				},
				enabled = false,
				toolTip = "Prevent AFK kick"
			},
			new ButtonInfo
			{
				buttonText = "Anti Guardian Grab",
				method = delegate
				{
					Mods.AntiGuardianGrab();
				},
				disableMethod = delegate
				{
					Mods.DisableAntiGuardianGrab();
				},
				enabled = false,
				toolTip = "Block guardian grab"
			},
			new ButtonInfo
			{
				buttonText = "Disable Network Triggers",
				method = delegate
				{
					Mods.EnableDisableNetworkTriggers();
				},
				disableMethod = delegate
				{
					Mods.DisableDisableNetworkTriggers();
				},
				enabled = false,
				toolTip = "Change maps without leaving"
			},
			new ButtonInfo
			{
				buttonText = "Disable Quit Box",
				method = delegate
				{
					Mods.EnableDisableQuitBox();
				},
				disableMethod = delegate
				{
					Mods.DisableDisableQuitBox();
				},
				enabled = false,
				toolTip = "Disable quit box"
			},
			new ButtonInfo
			{
				buttonText = "Block jman sounds",
				method = delegate
				{
					Mods.BlockJmanSounds();
				},
				disableMethod = delegate
				{
					Mods.DisableBlockJmanSounds();
				},
				enabled = false,
				toolTip = "Block jman sounds"
			},
			new ButtonInfo
			{
				buttonText = "PC Button Click",
				method = delegate
				{
					Mods.EnablePCButtonClick();
				},
				disableMethod = delegate
				{
					Mods.DisablePCButtonClick();
				},
				enabled = false,
				toolTip = "Click buttons with mouse"
			},
			new ButtonInfo
			{
				buttonText = "Mute Gun",
				method = delegate
				{
					Mods.MuteGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot to mute/unmute"
			},
			new ButtonInfo
			{
				buttonText = "Get ID Self",
				method = delegate
				{
					Mods.GetIDSelf();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Copy your ID"
			},
			new ButtonInfo
			{
				buttonText = "Join Code MODS",
				method = delegate
				{
					Mods.JoinCode("MODS");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Join MODS room"
			},
			new ButtonInfo
			{
				buttonText = "Join Code MOD",
				method = delegate
				{
					Mods.JoinCode("MOD");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Join MOD room"
			},
			new ButtonInfo
			{
				buttonText = "ARS",
				method = delegate
				{
					Mods.EnableARS();
				},
				disableMethod = delegate
				{
					Mods.DisableARS();
				},
				enabled = false,
				toolTip = "Auto-report system"
			}
		});
		MenuManager.AddCategory("Fun Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Fun Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Fun Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Unlock VIM/Subscription",
				method = delegate
				{
					Mods.UnlockVim();
				},
				disableMethod = delegate
				{
					Mods.DisableUnlockVim();
				},
				enabled = false,
				toolTip = "Unlock VIM features"
			},
			new ButtonInfo
			{
				buttonText = "Unlock All Cosmetics",
				method = delegate
				{
					Mods.UnlockAllCosmetics();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Unlocks every cosmetic instantly"
			},
			new ButtonInfo
			{
				buttonText = "Bitcrunch Mic",
				method = delegate
				{
					Mods.BitcrunchMic();
				},
				disableMethod = delegate
				{
					Mods.DisableBitcrunchMic();
				},
				enabled = false,
				toolTip = "Makes ur mic sound bad"
			},
			new ButtonInfo
			{
				buttonText = "Boop",
				method = delegate
				{
					Mods.Boop();
				},
				disableMethod = delegate
				{
					Mods.DisableBoop();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Play's a noise when booping someone"
			},
			new ButtonInfo
			{
				buttonText = "GetPlayerID Gun",
				method = delegate
				{
					Mods.GetPlayerIDGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot to copy ID"
			}
		});
		MenuManager.AddCategory("Rig Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Rig Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Rig Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Ghost Monke",
				method = delegate
				{
					Mods.GhostMonke();
				},
				disableMethod = delegate
				{
					Mods.DisableGhostMonke();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Press B to freeze your rig"
			},
			new ButtonInfo
			{
				buttonText = "Invis Monke",
				method = delegate
				{
					Mods.InvisMonke();
				},
				disableMethod = delegate
				{
					Mods.DisableInvisMonke();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "Press A to be invisible"
			},

		});
		MenuManager.AddCategory("Infection Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Infection Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Infection Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Tag Gun",
				method = delegate
				{
					Mods.TagGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Its tag gun"
			}
		});
		MenuManager.AddCategory("Master Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Master Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Master Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Not master client",
				method = null,
				enabled = false,
				nontoggleable = true,
				toolTip = "Your current master client status"
			},
			new ButtonInfo
			{
				buttonText = "Tag While Not Tagged",
				method = delegate
				{
					Mods.TagWhileNotTagged();
				},
				isFrameCall = true,
				enabled = false,
				toolTip = "tag while not infected"
			},
			new ButtonInfo
			{
				buttonText = "Untag Self",
				method = delegate
				{
					Mods.UntagSelf();
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Remove yourself from infected list"
			},
			new ButtonInfo
			{
				buttonText = "Spaz Self",
				method = delegate
				{
					Mods.SpazSelf();
				},
				disableMethod = delegate
				{
					Mods.DisableSpazSelf();
				},
				enabled = false,
				toolTip = "Toggle self it/infected every 10 frames"
			},
			new ButtonInfo
			{
				buttonText = "Spaz All",
				method = delegate
				{
					Mods.SpazAll();
				},
				disableMethod = delegate
				{
					Mods.DisableSpazAll();
				},
				enabled = false,
				toolTip = "Toggle all players it/infected every 10 frames"
			},
			new ButtonInfo
			{
				buttonText = "Untag Gun",
				method = delegate
				{
					Mods.UntagGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot infected players to untag them"
			}
		});
		MenuManager.AddCategory("Admin Mods", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Admin Mods",
				method = delegate
				{
					MenuManager.ToggleCategory("Admin Mods");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Kick Gun",
				method = delegate
				{
					Mods.KickGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot a player to kick them"
			},
			new ButtonInfo
			{
				buttonText = "Silent Kick Gun",
				method = delegate
				{
					Mods.SilentKickGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot a player to silently kick them"
			},
			new ButtonInfo
			{
				buttonText = "Fling Gun",
				method = delegate
				{
					Mods.FlingGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot a player to fling them"
			},
			new ButtonInfo
			{
				buttonText = "Vibrate Gun",
				method = delegate
				{
					Mods.VibrateGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot a player to vibrate their controllers"
			},
			new ButtonInfo
			{
				buttonText = "TP All Gun",
				method = delegate
				{
					Mods.TPAllGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot to TP everyone to that spot"
			},
			new ButtonInfo
			{
				buttonText = "Lightning Gun",
				method = delegate
				{
					Mods.LightningGun();
				},
				disableMethod = delegate
				{
					Mods.CleanupGun();
				},
				isGun = true,
				enabled = false,
				toolTip = "Shoot to strike lightning"
			},
			new ButtonInfo
			{
				buttonText = "Jail Gun",
				method = delegate
				{
					Mods.JailGun();
				},
				disableMethod = delegate
				{
					Mods.JailGunOff();
				},
				isGun = true,
				enabled = false,
				toolTip = "Trap players in a jail cell"
			},
			new ButtonInfo
			{
				buttonText = "Admin Grab",
				enableMethod = ConsoleMods.AdminGrab.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.AdminGrab.Disable,
				enabled = false,
				toolTip = "Grab players with your hand"
			},
			new ButtonInfo
			{
				buttonText = "Laser",
				enableMethod = ConsoleMods.Laser.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Laser.Disable,
				enabled = false,
				toolTip = "Toggle lasers from your hands"
			},
			new ButtonInfo
			{
				buttonText = "Kick All",
				method = ConsoleMods.KickAll,
				enabled = false,
				nontoggleable = true,
				toolTip = "Kick everyone from lobby"
			},
			new ButtonInfo
			{
				buttonText = "Karambit",
				enableMethod = ConsoleMods.Karambit.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Karambit.Disable,
				enabled = false,
				toolTip = "This is Karambit"
			},
			new ButtonInfo
			{
				buttonText = "Knife",
				enableMethod = ConsoleMods.Knife.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Knife.Disable,
				enabled = false,
				toolTip = "This is Knife"
			},
			new ButtonInfo
			{
				buttonText = "Rblx Carpet",
				enableMethod = ConsoleMods.RblxCarpet.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.RblxCarpet.Disable,
				enabled = false,
				toolTip = "This is Rblx Carpet"
			},
			new ButtonInfo
			{
				buttonText = "MC Sword",
				enableMethod = ConsoleMods.McSword.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.McSword.Disable,
				enabled = false,
				toolTip = "This is MC Sword"
			},
			new ButtonInfo
			{
				buttonText = "Ban Hammer",
				enableMethod = ConsoleMods.BanHammer.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.BanHammer.Disable,
				enabled = false,
				toolTip = "This is Ban Hammer"
			},
			new ButtonInfo
			{
				buttonText = "Roblox Sword",
				enableMethod = ConsoleMods.RobloxSword.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.RobloxSword.Disable,
				enabled = false,
				toolTip = "This is Roblox Sword"
			},
			new ButtonInfo
			{
				buttonText = "Rainbow Sword",
				enableMethod = ConsoleMods.RainbowSword.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.RainbowSword.Disable,
				enabled = false,
				toolTip = "This is Rainbow Sword"
			},
			new ButtonInfo
			{
				buttonText = "Pistol",
				enableMethod = ConsoleMods.Pistol.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Pistol.Disable,
				enabled = false,
				toolTip = "This is Pistol"
			},
			new ButtonInfo
			{
				buttonText = "Physics Gun",
				enableMethod = ConsoleMods.PhysicsGun.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.PhysicsGun.Disable,
				enabled = false,
				toolTip = "This is Physics Gun"
			},
			new ButtonInfo
			{
				buttonText = "Noli Star",
				enableMethod = ConsoleMods.NoliStar.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.NoliStar.Disable,
				enabled = false,
				toolTip = "This is Noli Star"
			},
			new ButtonInfo
			{
				buttonText = "Bag",
				enableMethod = ConsoleMods.Bag.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Bag.Disable,
				enabled = false,
				toolTip = "This is Bag"
			},
			new ButtonInfo
			{
				buttonText = "Kormakur",
				enableMethod = ConsoleMods.Kormakur.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Kormakur.Disable,
				enabled = false,
				toolTip = "This is Kormakur"
			},
			new ButtonInfo
			{
				buttonText = "Coin",
				enableMethod = ConsoleMods.Coin.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Coin.Disable,
				enabled = false,
				toolTip = "This is Coin"
			},
			new ButtonInfo
			{
				buttonText = "Minos Prime Plush",
				enableMethod = ConsoleMods.MinosPrime.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.MinosPrime.Disable,
				enabled = false,
				toolTip = "This is Minos Prime Plush"
			},
			new ButtonInfo
			{
				buttonText = "Boombox",
				enableMethod = ConsoleMods.Boombox.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Boombox.Disable,
				enabled = false,
				toolTip = "This is Boombox"
			},
			new ButtonInfo
			{
				buttonText = "Samsung",
				enableMethod = ConsoleMods.Samsung.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Samsung.Disable,
				enabled = false,
				toolTip = "This is Samsung"
			},
			new ButtonInfo
			{
				buttonText = "TV",
				enableMethod = ConsoleMods.TV.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.TV.Disable,
				enabled = false,
				toolTip = "This is TV"
			},
			new ButtonInfo
			{
				buttonText = "Travis",
				enableMethod = ConsoleMods.Travis.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Travis.Disable,
				enabled = false,
				toolTip = "This is Travis"
			},
			new ButtonInfo
			{
				buttonText = "Travis (Beach)",
				enableMethod = ConsoleMods.TravisBeach.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.TravisBeach.Disable,
				enabled = false,
				toolTip = "This is Travis (Beach)"
			},
			new ButtonInfo
			{
				buttonText = "Travis (Critters)",
				enableMethod = ConsoleMods.TravisCritters.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.TravisCritters.Disable,
				enabled = false,
				toolTip = "This is Travis (Critters)"
			},
			new ButtonInfo
			{
				buttonText = "Travis (City)",
				enableMethod = ConsoleMods.TravisCity.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.TravisCity.Disable,
				enabled = false,
				toolTip = "This is Travis (City)"
			},
			new ButtonInfo
			{
				buttonText = "Shreksophone",
				enableMethod = ConsoleMods.Shreksophone.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Shreksophone.Disable,
				enabled = false,
				toolTip = "This is Shreksophone"
			},
			new ButtonInfo
			{
				buttonText = "Carti",
				enableMethod = ConsoleMods.Carti.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.Carti.Disable,
				enabled = false,
				toolTip = "This is Carti"
			},
			new ButtonInfo
			{
				buttonText = "Destroy All Assets",
				method = ConsoleMods.DestroyAllAssets,
				enabled = false,
				nontoggleable = true,
				toolTip = "Remove all spawned assets"
			},
			new ButtonInfo
			{
				buttonText = "Console Settings",
				method = delegate
				{
					MenuManager.ToggleCategory("Console Settings");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Console settings (laser color, protection, etc)"
			}
		});
		MenuManager.AddCategory("Console Settings", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Console Settings",
				method = delegate
				{
					MenuManager.ToggleCategory("Console Settings");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Back to Admin Mods"
			},
			new ButtonInfo
			{
				buttonText = "Allow Kick Self",
				enableMethod = ConsoleMods.AllowKickSelf.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.AllowKickSelf.Disable,
				enabled = false,
				toolTip = "Allow other admins to kick/tp/fling you"
			},
			new ButtonInfo
			{
				buttonText = "Allow Teleport Self",
				enableMethod = ConsoleMods.AllowTpSelf.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.AllowTpSelf.Disable,
				enabled = true,
				toolTip = "Allow other admins to teleport you"
			},
			new ButtonInfo
			{
				buttonText = "Detect Console Users",
				enableMethod = ConsoleMods.DetectConsoleUsers.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.DetectConsoleUsers.Disable,
				enabled = false,
				toolTip = "Auto detect who has console"
			},
			new ButtonInfo
			{
				buttonText = "No Admin Indicator",
				enableMethod = ConsoleMods.NoAdminIndicator.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.NoAdminIndicator.Disable,
				enabled = false,
				toolTip = "Hide your admin crown"
			},
			new ButtonInfo
			{
				buttonText = "Change Laser Color",
				method = ConsoleMods.Laser.CycleColor,
				enabled = false,
				nontoggleable = true,
				toolTip = "Change laser color"
			},
			new ButtonInfo
			{
				buttonText = "Sound",
				method = delegate
				{
					MenuManager.ToggleCategory("Sound");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Select boombox audio track"
			},
			new ButtonInfo
			{
				buttonText = "Video",
				method = delegate
				{
					MenuManager.ToggleCategory("Video");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Select TV/Samsung video"
			},
			new ButtonInfo
			{
				buttonText = "Full Auto Pistol",
				enableMethod = ConsoleMods.FullAutoPistol.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.FullAutoPistol.Disable,
				enabled = false,
				toolTip = "Toggle full auto mode for pistol"
			},
			new ButtonInfo
			{
				buttonText = "Mute Rainbow Sword",
				enableMethod = ConsoleMods.MuteRainbowSword.Enable,
				method = ConsoleMods.Run,
				disableMethod = ConsoleMods.MuteRainbowSword.Disable,
				enabled = false,
				toolTip = "Replace rainbow sword music with silence"
			}
		});
		MenuManager.AddCategory("Sound", ConsoleMods.BuildSoundCategory());
		MenuManager.AddCategory("Video", ConsoleMods.BuildVideoCategory());
		MenuManager.AddCategory("Credits", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Credits",
				method = delegate
				{
					MenuManager.ToggleCategory("Credits");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			},
			new ButtonInfo
			{
				buttonText = "Jolyne",
				method = null,
				enabled = false,
				nontoggleable = true,
				toolTip = "Menu owner"
			},
			new ButtonInfo
			{
				buttonText = "DeepSeek V4",
				method = null,
				enabled = false,
				nontoggleable = true,
				toolTip = "Made most of the mods on the menu"
			},
			new ButtonInfo
			{
				buttonText = "Seralyth",
				method = null,
				enabled = false,
				nontoggleable = true,
				toolTip = "has skidded code from Seralyth"
			},
			new ButtonInfo
			{
				buttonText = "Industry",
				method = null,
				enabled = false,
				nontoggleable = true,
				toolTip = "ARS system by Industry"
			},
			new ButtonInfo
			{
				buttonText = "Tripple T",
				method = delegate
				{
					MenuManager.ToggleCategory("Tripple T");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "View Tripple T image"
			}
		});
		MenuManager.AddCategory("Tripple T", new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Tripple T",
				method = delegate
				{
					MenuManager.ToggleCategory("Credits");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go back to Credits"
			}
		});
	}

	public static IEnumerator OpenAni()
	{
		if ((Object)(object)menu == (Object)null)
		{
			yield break;
		}
		float elapsed = 0f;
		float playerScale = GTPlayer.Instance.scale;
		if (!animationsEnabled)
		{
			menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * playerScale;
			yield break;
		}
		Vector3 startScale = menu.transform.localScale;
		Vector3 targetScale = new Vector3(0.1f, 0.3f, 0.4f) * playerScale;
		while (elapsed < 0.3f)
		{
			if ((Object)(object)menu == (Object)null)
			{
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			t -= 1f;
			float bounce = t * t * ((s + 1f) * t + s) + 1f;
			menu.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((Object)(object)menu != (Object)null)
		{
			menu.transform.localScale = targetScale;
		}
	}

	public static IEnumerator CloseAni()
	{
		if ((Object)(object)menu == (Object)null || Close)
		{
			yield break;
		}
		if (!animationsEnabled)
		{
			DestroyMenu();
			yield break;
		}
		Close = true;
		float elapsed = 0f;
		Vector3 startScale = menu.transform.localScale;
		Vector3 targetScale = Vector3.zero;
		while (elapsed < 0.3f)
		{
			if ((Object)(object)menu == (Object)null)
			{
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			float bounce = t * t * ((s + 1f) * t - s);
			menu.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((Object)(object)menu != (Object)null)
		{
			Object.Destroy((Object)(object)menu);
		}
		menu = null;
		menuObj = null;
		canvasObj = null;
		if ((Object)(object)reference != (Object)null)
		{
			Object.Destroy((Object)(object)reference);
		}
		reference = null;
		Close = false;
	}

	private void Update()
	{
		try
		{
			gripDownL = ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
			gripDownR = ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
			triggerDownL = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat == 1f;
			triggerDownR = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat == 1f;
			abuttonDown = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
			bbuttonDown = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
			xbuttonDown = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimaryButton;
			ybuttonDown = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerSecondaryButton;
			joy = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimary2DAxis;
			joyL = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimary2DAxis;
			bool qKeyDown = Keyboard.current != null && ((ButtonControl)Keyboard.current.qKey).isPressed;
			if (Mods.change7 == 5 && (Object)(object)menu != (Object)null && !menu.GetComponent<Rigidbody>())
			{
				HandleTriggerPageNav();
			}
			HandleMenuFollow(qKeyDown);
			Mods.UpdateActiveMods();
			_frameCounter++;
			if (_frameCounter % 15 == 0)
			{
				UpdateMasterClientStatus();
				CheckAdminStatus();
			}
			fpsAccumulator += Time.unscaledDeltaTime;
			fpsFrameCount++;
			if (fpsFrameCount >= 30)
			{
				int num = ((fpsAccumulator > 0f) ? Mathf.RoundToInt((float)fpsFrameCount / fpsAccumulator) : 0);
				fpsAccumulator = 0f;
				fpsFrameCount = 0;
				cachedFPS = num;
			}
			TimeSpan timeSpan = DateTime.Now - sessionStartTime;
			List<string> list = new List<string>();
			if (showFPS)
			{
				list.Add("FPS: " + cachedFPS);
			}
			if (showSessionTime)
			{
				list.Add((int)timeSpan.TotalMinutes + ":" + timeSpan.Seconds.ToString("D2"));
			}
			bottomBarStr = string.Join(" | ", list);
			if ((Object)(object)menu != (Object)null && (Object)(object)fpsText != (Object)null)
			{
				fpsText.text = bottomBarStr;
			}
			if (!Directory.Exists(FolderName))
			{
				Directory.CreateDirectory(FolderName);
			}
			if (custom)
			{
				UpdateCustomBoardText();
			}
		}
		catch
		{
		}
	}

	private void HandleTriggerPageNav()
	{
		if (triggerDownL)
		{
			if (!toggle)
			{
				Toggle("PreviousPage");
				VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f);
				toggle = true;
			}
		}
		else
		{
			toggle = false;
		}
		if (triggerDownR)
		{
			if (!toggle1)
			{
				Toggle("NextPage");
				VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f);
				toggle1 = true;
			}
		}
		else
		{
			toggle1 = false;
		}
	}

	private void HandleMenuFollow(bool qKeyDown)
	{
		bool flag = (ybuttonDown && !Mods.right) || (bbuttonDown && Mods.right) || qKeyDown;
		if (flag)
		{
			if ((Object)(object)menu == (Object)null)
			{
				instance.Draw();
				menu.transform.localScale = Vector3.one * 0.001f;
				((MonoBehaviour)instance).StartCoroutine(OpenAni());
			}
			if (qKeyDown)
			{
				if ((Object)(object)_tpc == (Object)null)
				{
					GameObject val = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if ((Object)(object)val != (Object)null)
					{
						_tpc = val.GetComponent<Camera>();
					}
					if ((Object)(object)_tpc == (Object)null)
					{
						val = GameObject.Find("Shoulder Camera");
						if ((Object)(object)val != (Object)null)
						{
							_tpc = val.GetComponent<Camera>();
						}
					}
				}
				if ((Object)(object)_tpc != (Object)null)
				{
					menu.transform.parent = ((Component)_tpc).transform;
					menu.transform.position = ((Component)_tpc).transform.position + ((Component)_tpc).transform.forward * 0.5f;
					menu.transform.rotation = ((Component)_tpc).transform.rotation * Quaternion.Euler(-90f, 90f, 0f);
					if (Mouse.current != null && (Object)(object)reference != (Object)null)
					{
						bool isPressed = Mouse.current.leftButton.isPressed;
						if (isPressed && !_mouseWasPressed)
						{
							Ray val2 = _tpc.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
							RaycastHit val3 = default(RaycastHit);
							if (Physics.Raycast(val2, out val3, 512f, Mods.GetNoInvisLayerMask()) && (Object)(object)val3.transform != (Object)(object)reference.transform)
							{
								BtnCollider component = ((Component)val3.transform).gameObject.GetComponent<BtnCollider>();
								if ((Object)(object)component != (Object)null && !string.IsNullOrEmpty(component.relatedText))
								{
									Toggle(component.relatedText);
								}
							}
						}
						_mouseWasPressed = isPressed;
					}
				}
				else
				{
					menu.transform.parent = ((Component)GTPlayer.Instance.headCollider).transform;
					menu.transform.position = ((Component)GTPlayer.Instance.headCollider).transform.position + ((Component)GTPlayer.Instance.headCollider).transform.forward * 0.5f;
					menu.transform.rotation = ((Component)GTPlayer.Instance.headCollider).transform.rotation * Quaternion.Euler(-90f, 90f, 0f);
				}
				if ((Object)(object)reference == (Object)null)
				{
					reference = GameObject.CreatePrimitive((PrimitiveType)0);
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.RightHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
				reference.GetComponent<Renderer>().material.color = (ChangingColors ? FirstColor : NormalColor);
			}
			else if (ybuttonDown && !Mods.right)
			{
				menu.transform.position = GTPlayer.Instance.LeftHand.controllerTransform.position;
				menu.transform.rotation = GTPlayer.Instance.LeftHand.controllerTransform.rotation;
				if ((Object)(object)reference == (Object)null)
				{
					reference = GameObject.CreatePrimitive((PrimitiveType)0);
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.RightHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
				reference.GetComponent<Renderer>().material.color = (ChangingColors ? FirstColor : NormalColor);
			}
			else if (bbuttonDown && Mods.right)
			{
				menu.transform.position = GTPlayer.Instance.RightHand.controllerTransform.position;
				menu.transform.rotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
				menu.transform.RotateAround(menu.transform.position, menu.transform.forward, 180f);
				if ((Object)(object)reference == (Object)null)
				{
					reference = GameObject.CreatePrimitive((PrimitiveType)0);
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.LeftHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
				reference.GetComponent<Renderer>().material.color = (ChangingColors ? FirstColor : NormalColor);
			}
		}
		else if (!flag && (Object)(object)menu != (Object)null && !Close)
		{
			Mods.SendMenuClose();
			Object.Destroy((Object)(object)reference);
			reference = null;
			((MonoBehaviour)instance).StartCoroutine(CloseAni());
		}
	}

	private void UpdateMasterClientStatus()
	{
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Master Mods");
		if (menuCategory == null || menuCategory.Buttons.Count <= 1)
		{
			return;
		}
		bool isMasterClient = PhotonNetwork.IsMasterClient;
		menuCategory.Buttons[1].buttonText = (isMasterClient ? "You are master client" : "Not master client");
		menuCategory.Buttons[1].toolTip = (isMasterClient ? "You are the master client" : "You are not the master client");
		if (isMasterClient)
		{
			return;
		}
		for (int num = 2; num < menuCategory.Buttons.Count; num++)
		{
			ButtonInfo buttonInfo = menuCategory.Buttons[num];
			if (buttonInfo.enabled != true)
			{
				continue;
			}
			if (buttonInfo.disableMethod != null)
			{
				try
				{
					buttonInfo.disableMethod();
				}
				catch
				{
				}
			}
			buttonInfo.enabled = false;
		}
	}

		private void CheckAdminStatus()
		{
			bool flag = PhotonNetwork.LocalPlayer != null && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
			bool flag2 = MenuManager.Categories.Any((MenuCategory c) => c.Name == "Admin Mods");
			bool flag3 = false;
			MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Main");
			if (menuCategory != null)
			{
				flag3 = menuCategory.Buttons.Any((ButtonInfo b) => b.buttonText == "Admin Mods");
			}
			if (flag && flag2 && !flag3)
			{
				string text = ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId];
				if (!_adminInitialized)
				{
					string text2 = (ServerData.SuperAdministrators.Contains(text) ? "super admin " : "");
					NotifiLib.SendNotification("[<color=green>CHUD</color>] Welcome " + text2 + text, 3);
					_adminInitialized = true;
				}
				if (menuCategory != null && !flag3)
				{
					menuCategory.Buttons.Add(new ButtonInfo
					{
						buttonText = "Admin Mods",
						method = delegate
						{
							MenuManager.ToggleCategory("Admin Mods");
						},
						enabled = false,
						nontoggleable = true,
						toolTip = "Go to Admin Mods!"
					});
				}
			}
			else if (!flag && flag3)
			{
				menuCategory?.Buttons.RemoveAll((ButtonInfo b) => b.buttonText == "Admin Mods");
				if (MenuManager.CurrentCategoryName == "Admin Mods" || MenuManager.CurrentCategoryName == "Console Settings")
				{
					MenuManager.CurrentCategoryName = "Main";
				}
				pageNumber = 0;
				DestroyMenu();
				instance.Draw();
			}
		}

	private void UpdateCustomBoardText()
	{
		((TMP_Text)GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText").GetComponent<TextMeshPro>()).text = CustomBoardTexts[0];
		((TMP_Text)GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText").GetComponent<TextMeshPro>()).text = CustomBoardTexts[1];
		((TMP_Text)GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData").GetComponent<TextMeshPro>()).text = CustomBoardTexts[2];
		if (PhotonNetwork.IsConnectedAndReady)
		{
			((TMP_Text)GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText").GetComponent<TextMeshPro>()).text = CustomBoardTexts[3];
			custom = false;
		}
	}

	public void Draw()
	{
		if (MenuManager.CurrentCategoryName == "Enabled Mods")
		{
			RebuildEnabledMods();
		}
		pageSize = 7;
		menu = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)menu.GetComponent<Rigidbody>());
		Object.Destroy((Object)(object)menu.GetComponent<BoxCollider>());
		Object.Destroy((Object)(object)menu.GetComponent<Renderer>());
		menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
		menuObj = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)menuObj.GetComponent<Rigidbody>());
		Object.Destroy((Object)(object)menuObj.GetComponent<BoxCollider>());
		menuObj.transform.parent = menu.transform;
		menuObj.transform.rotation = Quaternion.identity;
		menuObj.transform.localScale = new Vector3(0.1f, 1f, 1f);
		Renderer component = menuObj.GetComponent<Renderer>();
		component.material.color = NormalColor;
		menuObj.transform.position = new Vector3(0.05f, 0f, 0f);
		Shader val = Shader.Find("GorillaTag/UberShader");
		if ((Object)(object)val != (Object)null)
		{
			component.material.shader = val;
		}
		if (roundedObjects)
		{
			RoundGameObject(menuObj, "__background__");
		}
		canvasObj = new GameObject();
		canvasObj.transform.parent = menu.transform;
		Canvas val2 = canvasObj.AddComponent<Canvas>();
		CanvasScaler val3 = canvasObj.AddComponent<CanvasScaler>();
		canvasObj.AddComponent<GraphicRaycaster>();
		val2.renderMode = (RenderMode)2;
		val3.dynamicPixelsPerUnit = 1900f;
		val3.referencePixelsPerUnit = 100f;
		GameObject val4 = new GameObject();
		val4.transform.parent = canvasObj.transform;
		Text val5 = (titiel = val4.AddComponent<Text>());
		val5.font = MenuFont;
		val5.text = ((MenuManager.CurrentCategoryName == "Tripple T") ? "Tripple T" : ((Mods.menuColorIndex == 4) ? "ii's stupid menu" : MenuTitle));
		val5.fontSize = 200;
		((Graphic)val5).color = MenuTitleColor;
		val5.fontStyle = (FontStyle)2;
		val5.alignment = (TextAnchor)4;
		val5.resizeTextForBestFit = true;
		val5.resizeTextMinSize = 0;
		val5.resizeTextMaxSize = 200;
		RectTransform component2 = ((Component)val5).GetComponent<RectTransform>();
		((Transform)component2).localPosition = Vector3.zero;
		component2.sizeDelta = new Vector2(0.28f, 0.05f);
		((Transform)component2).position = new Vector3(0.06f, 0f, 0.175f);
		((Transform)component2).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val6 = new GameObject();
		val6.transform.parent = canvasObj.transform;
		fpsText = val6.AddComponent<Text>();
		fpsText.font = MenuFont;
		fpsText.text = bottomBarStr;
		fpsText.fontSize = 200;
		((Graphic)fpsText).color = ToolTipColor;
		fpsText.fontStyle = (FontStyle)2;
		fpsText.alignment = (TextAnchor)4;
		fpsText.resizeTextForBestFit = true;
		fpsText.resizeTextMinSize = 0;
		fpsText.resizeTextMaxSize = 200;
		RectTransform component3 = ((Component)fpsText).GetComponent<RectTransform>();
		((Transform)component3).localPosition = Vector3.zero;
		component3.sizeDelta = new Vector2(0.28f, 0.02f);
		((Transform)component3).position = new Vector3(0.06f, 0f, 0.135f);
		((Transform)component3).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		List<ButtonInfo> currentButtons = MenuManager.CurrentButtons;
		GameObject val7 = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)val7.GetComponent<Rigidbody>());
		((Collider)val7.GetComponent<BoxCollider>()).isTrigger = true;
		val7.transform.parent = menu.transform;
		val7.transform.rotation = Quaternion.identity;
		val7.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
		val7.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
		val7.GetComponent<Renderer>().material.color = DisconnectButtonColor;
		val7.AddComponent<BtnCollider>().relatedText = "DisconnectingButton";
		if ((Object)(object)val != (Object)null)
		{
			val7.GetComponent<Renderer>().material.shader = val;
		}
		if (roundedObjects)
		{
			RoundGameObject(val7, "DisconnectingButton");
		}
		GameObject val8 = new GameObject();
		val8.transform.parent = canvasObj.transform;
		Text val9 = val8.AddComponent<Text>();
		val9.font = MenuFont;
		val9.text = "Disconnect";
		val9.fontSize = 200;
		((Graphic)val9).color = DisconnectTextColor;
		val9.alignment = (TextAnchor)4;
		val9.resizeTextForBestFit = true;
		val9.resizeTextMinSize = 0;
		val9.resizeTextMaxSize = 200;
		val9.fontStyle = (FontStyle)2;
		RectTransform component4 = ((Component)val9).GetComponent<RectTransform>();
		((Transform)component4).localPosition = Vector3.zero;
		component4.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component4).localPosition = new Vector3(0.064f, 0f, 0.23f);
		((Transform)component4).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val10 = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)val10.GetComponent<Rigidbody>());
		((Collider)val10.GetComponent<BoxCollider>()).isTrigger = true;
		val10.transform.parent = menu.transform;
		val10.transform.rotation = Quaternion.identity;
		val10.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val10.transform.localPosition = new Vector3(0.56f, 0.65f, 0f);
		val10.GetComponent<Renderer>().material.color = NextPrevButtonColor;
		val10.AddComponent<BtnCollider>().relatedText = "PreviousPage";
		if ((Object)(object)val != (Object)null)
		{
			val10.GetComponent<Renderer>().material.shader = val;
		}
		if (roundedObjects)
		{
			RoundGameObject(val10, "PreviousPage");
		}
		GameObject val11 = new GameObject();
		val11.transform.parent = canvasObj.transform;
		Text val12 = val11.AddComponent<Text>();
		val12.font = MenuFont;
		val12.text = "<";
		val12.fontSize = 200;
		((Graphic)val12).color = NextPrevTextColor;
		val12.fontStyle = (FontStyle)2;
		val12.alignment = (TextAnchor)4;
		val12.resizeTextForBestFit = true;
		val12.resizeTextMinSize = 0;
		val12.resizeTextMaxSize = 200;
		RectTransform component5 = ((Component)val12).GetComponent<RectTransform>();
		((Transform)component5).localPosition = Vector3.zero;
		component5.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component5).localPosition = new Vector3(0.064f, 0.195f, 0f);
		((Transform)component5).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val13 = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)val13.GetComponent<Rigidbody>());
		((Collider)val13.GetComponent<BoxCollider>()).isTrigger = true;
		val13.transform.parent = menu.transform;
		val13.transform.rotation = Quaternion.identity;
		val13.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val13.transform.localPosition = new Vector3(0.56f, -0.65f, 0f);
		val13.GetComponent<Renderer>().material.color = NextPrevButtonColor;
		val13.AddComponent<BtnCollider>().relatedText = "NextPage";
		if ((Object)(object)val != (Object)null)
		{
			val13.GetComponent<Renderer>().material.shader = val;
		}
		if (roundedObjects)
		{
			RoundGameObject(val13, "NextPage");
		}
		GameObject val14 = new GameObject();
		val14.transform.parent = canvasObj.transform;
		Text val15 = val14.AddComponent<Text>();
		val15.font = MenuFont;
		val15.text = ">";
		val15.fontSize = 200;
		((Graphic)val15).color = NextPrevTextColor;
		val15.fontStyle = (FontStyle)2;
		val15.alignment = (TextAnchor)4;
		val15.resizeTextForBestFit = true;
		val15.resizeTextMinSize = 0;
		val15.resizeTextMaxSize = 200;
		RectTransform component6 = ((Component)val15).GetComponent<RectTransform>();
		((Transform)component6).localPosition = Vector3.zero;
		component6.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component6).localPosition = new Vector3(0.064f, -0.195f, 0f);
		((Transform)component6).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		if (currentButtons != null)
		{
			string[] array = (from b in currentButtons.Skip(pageNumber * pageSize).Take(pageSize)
				select b.buttonText).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				float num2 = (float)num * ((pageSize == 7) ? 0.116f : 0.1f);
				GameObject val16 = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val16.GetComponent<Rigidbody>());
				((Collider)val16.GetComponent<BoxCollider>()).isTrigger = true;
				val16.transform.parent = menu.transform;
				val16.transform.rotation = Quaternion.identity;
				val16.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
				val16.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - num2);
				val16.AddComponent<BtnCollider>().relatedText = array[num];
				if ((Object)(object)val != (Object)null)
				{
					val16.GetComponent<Renderer>().material.shader = val;
				}
				int num3 = -1;
				for (int num4 = 0; num4 < currentButtons.Count; num4++)
				{
					if (array[num] == currentButtons[num4].buttonText)
					{
						num3 = num4;
						break;
					}
				}
				bool? flag = null;
				if (num3 >= 0 && num3 < currentButtons.Count)
				{
					flag = currentButtons[num3].enabled;
				}
				val16.GetComponent<Renderer>().material.color = ((flag == true) ? ButtonColorEnabled : ButtonColorDisable);
				if (roundedObjects)
				{
					RoundGameObject(val16, array[num]);
				}
				GameObject val17 = new GameObject();
				val17.transform.parent = canvasObj.transform;
				Text val18 = val17.AddComponent<Text>();
				val18.font = MenuFont;
				val18.text = array[num];
				val18.fontSize = 200;
				val18.supportRichText = true;
				((Graphic)val18).color = ((flag == true) ? EnableTextColor : DIsableTextColor);
				val18.fontStyle = (FontStyle)2;
				val18.alignment = (TextAnchor)4;
				val18.resizeTextForBestFit = true;
				val18.resizeTextMinSize = 0;
				val18.resizeTextMaxSize = 200;
				RectTransform component7 = ((Component)val18).GetComponent<RectTransform>();
				((Transform)component7).localPosition = Vector3.zero;
				component7.sizeDelta = new Vector2(0.2f, 0.03f);
				((Transform)component7).localPosition = new Vector3(0.064f, 0f, 0.111f - num2 / 2.6f);
				((Transform)component7).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			}
		}
		if (MenuManager.CurrentCategoryName == "Tripple T" && (Object)(object)menuImage != (Object)null)
		{
			GameObject val19 = new GameObject("TrippleTImage");
			val19.transform.SetParent(canvasObj.transform);
			RawImage val20 = val19.AddComponent<RawImage>();
			val20.texture = (Texture)(object)menuImage;
			RectTransform component8 = val19.GetComponent<RectTransform>();
			((Transform)component8).localPosition = new Vector3(0.064f, 0f, -0.025f);
			component8.sizeDelta = new Vector2(0.18f, 0.2f);
			((Transform)component8).rotation = Quaternion.Euler(180f, 90f, 90f);
		}
	}

	public static void RoundGameObject(GameObject obj, string identifier)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		float num = 0.02f;
		Vector3 localScale = obj.transform.localScale;
		Vector3 localPosition = obj.transform.localPosition;
		Transform transform = menu.transform;
		Shader val = Shader.Find("GorillaTag/UberShader");
		List<Renderer> list = new List<Renderer>();
		GameObject val2 = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)val2.GetComponent<Rigidbody>());
		Object.Destroy((Object)(object)val2.GetComponent<Collider>());
		val2.transform.parent = transform;
		val2.transform.rotation = Quaternion.identity;
		val2.transform.localPosition = localPosition;
		val2.transform.localScale = new Vector3(localScale.x, localScale.y - num * 2.55f, localScale.z);
		list.Add(val2.GetComponent<Renderer>());
		GameObject val3 = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)val3.GetComponent<Rigidbody>());
		Object.Destroy((Object)(object)val3.GetComponent<Collider>());
		val3.transform.parent = transform;
		val3.transform.rotation = Quaternion.identity;
		val3.transform.localPosition = localPosition;
		val3.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z - num * 2f);
		list.Add(val3.GetComponent<Renderer>());
		for (int i = 0; i < 4; i++)
		{
			float num2 = ((i == 0 || i == 2) ? 1f : (-1f));
			float num3 = ((i == 0 || i == 1) ? 1f : (-1f));
			GameObject val4 = GameObject.CreatePrimitive((PrimitiveType)2);
			Object.Destroy((Object)(object)val4.GetComponent<Rigidbody>());
			Object.Destroy((Object)(object)val4.GetComponent<Collider>());
			val4.transform.parent = transform;
			val4.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 0f, 90f);
			val4.transform.localPosition = localPosition + new Vector3(0f, num2 * (localScale.y / 2f - num * 1.275f), num3 * (localScale.z / 2f - num));
			val4.transform.localScale = new Vector3(num * 2.55f, localScale.x / 2f, num * 2f);
			list.Add(val4.GetComponent<Renderer>());
		}
		Color color = component.material.color;
		foreach (Renderer item in list)
		{
			Material material = item.material;
			material.color = color;
			if ((Object)(object)val != (Object)null)
			{
				material.shader = val;
			}
		}
		roundedRenderers[identifier] = list;
		component.enabled = false;
	}

	public static void DestroyMenu()
	{
		Object.Destroy((Object)(object)menu);
		Object.Destroy((Object)(object)canvasObj);
		Object.Destroy((Object)(object)reference);
		menu = null;
		menuObj = null;
		canvasObj = null;
		reference = null;
		roundedRenderers.Clear();
	}

	private void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		Backend.Console.LoadConsole();
		InitCategories();
		InitMenuFont();
		sessionStartTime = DateTime.Now;
		((MonoBehaviour)this).StartCoroutine(LoadMenuImage());
		((MonoBehaviour)this).StartCoroutine(LoadCustomButtonClickAudio());
		GorillaTagger.OnPlayerSpawned((Action)delegate
		{
			Mods.AutoLoad();
		});
		Draw();
	}

	public static void RebuildEnabledMods()
	{
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Enabled Mods");
		if (menuCategory == null)
		{
			return;
		}
		menuCategory.Buttons.Clear();
		menuCategory.Buttons.Add(new ButtonInfo
		{
			buttonText = "Exit Enabled Mods",
			method = delegate
			{
				MenuManager.ToggleCategory("Enabled Mods");
			},
			enabled = false,
			nontoggleable = true,
			toolTip = "Go to Main"
		});
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Name == "Main" || category.Name == "Enabled Mods" || category.Name == "Admin Mods" || category.Name == "Console Settings")
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled != true || button.disableMethod == null)
				{
					continue;
				}
				string capturedText = button.buttonText;
				menuCategory.Buttons.Add(new ButtonInfo
				{
					buttonText = capturedText,
					method = delegate
					{
						Mods.FindAndToggleButton(capturedText);
						DestroyMenu();
						if ((Object)(object)instance != (Object)null)
						{
							instance.Draw();
						}
					},
					enabled = true,
					nontoggleable = true,
					toolTip = (button.toolTip ?? "")
				});
			}
		}
	}

	public static void Toggle(string relatedText)
	{
		if (Time.time - lastButtonPressTime < 0.1f)
		{
			return;
		}
		lastButtonPressTime = Time.time;
		List<ButtonInfo> currentButtons = MenuManager.CurrentButtons;
		if (currentButtons == null)
		{
			return;
		}
		int count = currentButtons.Count;
		int num = (count + pageSize - 1) / pageSize;
		if (num < 1)
		{
			num = 1;
		}
		switch (relatedText)
		{
		case "NextPage":
			if (pageNumber < num - 1)
			{
				pageNumber++;
			}
			else
			{
				pageNumber = 0;
			}
			DestroyMenu();
			instance.Draw();
			Mods.SendMenuState();
			return;
		case "PreviousPage":
			if (pageNumber > 0)
			{
				pageNumber--;
			}
			else
			{
				pageNumber = num - 1;
			}
			DestroyMenu();
			instance.Draw();
			Mods.SendMenuState();
			return;
		case "DisconnectingButton":
			PhotonNetwork.Disconnect();
			return;
		}
		int num2 = -1;
		for (int i = 0; i < currentButtons.Count; i++)
		{
			if (relatedText == currentButtons[i].buttonText)
			{
				num2 = i;
				break;
			}
		}
		if (num2 < 0 || num2 >= currentButtons.Count || !currentButtons[num2].enabled.HasValue)
		{
			return;
		}
		ButtonInfo buttonInfo = currentButtons[num2];
		if (buttonInfo.nontoggleable == true)
		{
			buttonInfo.method?.Invoke();
			return;
		}
		if (MenuManager.CurrentCategoryName == "Master Mods" && !PhotonNetwork.IsMasterClient)
		{
			NotifiLib.SendNotification("[<color=red>MASTER</color>] You are not master client!");
			return;
		}
		bool value = buttonInfo.enabled.Value;
		buttonInfo.enabled = !value;
		if (buttonInfo.enabled == true)
		{
			if (buttonInfo.enableMethod != null)
			{
				buttonInfo.enableMethod();
			}
			else
			{
				buttonInfo.method?.Invoke();
			}
		}
		else if (buttonInfo.disableMethod != null)
		{
			buttonInfo.disableMethod();
		}
		Mods.AutoSave();
		Mods.SendButtonClick();
		Mods.SendMenuState();
		if (buttonInfo.enabled == true && !string.IsNullOrEmpty(buttonInfo.toolTip) && buttonInfo.toolTip != "This button doesn't have a tooltip/tutorial")
		{
			NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] " + buttonInfo.buttonText + ": " + buttonInfo.toolTip, 2);
		}
		if ((Object)(object)menu != (Object)null)
		{
			UpdateButtonVisual(relatedText, buttonInfo.enabled.Value);
		}
	}

	private static void UpdateButtonVisual(string buttonText, bool isEnabled)
	{
		Shader val = Shader.Find("GorillaTag/UberShader");
		foreach (Transform item in menu.transform)
		{
			Transform val2 = item;
			BtnCollider component = ((Component)val2).GetComponent<BtnCollider>();
			if ((Object)(object)component != (Object)null && component.relatedText == buttonText)
			{
				Renderer component2 = ((Component)val2).GetComponent<Renderer>();
				component2.material.color = (isEnabled ? ButtonColorEnabled : ButtonColorDisable);
				if ((Object)(object)val != (Object)null)
				{
					component2.material.shader = val;
				}
				break;
			}
		}
		if (roundedRenderers.TryGetValue(buttonText, out var value))
		{
			Shader val3 = Shader.Find("GorillaTag/UberShader");
			foreach (Renderer item2 in value)
			{
				Material material = item2.material;
				material.color = (isEnabled ? ButtonColorEnabled : ButtonColorDisable);
				if ((Object)(object)val3 != (Object)null)
				{
					material.shader = val3;
				}
			}
		}
		if (!((Object)(object)canvasObj != (Object)null))
		{
			return;
		}
		foreach (Transform item3 in canvasObj.transform)
		{
			Transform val4 = item3;
			Text component3 = ((Component)val4).GetComponent<Text>();
			if ((Object)(object)component3 != (Object)null && component3.text == buttonText)
			{
				((Graphic)component3).color = (isEnabled ? EnableTextColor : DIsableTextColor);
				break;
			}
		}
	}
}
