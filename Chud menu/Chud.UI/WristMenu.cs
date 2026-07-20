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

	public static Color DisableTextColor = new Color(0.75f, 0.75f, 0.75f);

	public static Color MenuTitleColor = Color.white;

	public static Color ToolTipColor = new Color(0.8f, 0.8f, 0.8f);

	public static Color DisconnectButtonColor = new Color(0.5f, 0f, 0f);

	public static Color DisconnectTextColor = Color.white;

	public static Color NextPrevButtonColor = new Color(0.15f, 0.15f, 0.15f);

	public static Color NextPrevTextColor = Color.white;

	public static bool roundedObjects = false;

	internal static List<Material> gradientMaterials = new List<Material>();

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

	public static bool customBoardsEnabled = true;
	private static bool customBoardsApplied = false;
	private static string[] originalBoardTexts = new string[4];
	private static GameObject[] cachedBoardObjects = new GameObject[4];
	private static TMP_Text[] cachedBoardTexts = new TMP_Text[4];
	private static readonly string[] BoardPaths = new string[]
	{
		"Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText"
	};

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
			Nav("Settings", "Settings"),
			BtnAction("Join Discord", () => Application.OpenURL("https://discord.gg/2J7JrpQTg4"), "Join the Chud Menu Discord"),
			Nav("Enabled Mods", "Enabled Mods"),
			Nav("Movement Mods", "Movement Mods"),
			Nav("Visual Mods", "Visual Mods"),
			Nav("Fun Mods", "Fun Mods"),
			Nav("Useful Mods", "Useful Mods"),
			Nav("Rig Mods", "Rig Mods"),
			Nav("Infection Mods", "Infection Mods"),
			Nav("Master Mods", "Master Mods"),
			Nav("Room mods", "Room mods"),
			Nav("Credits", "Credits"),
			Nav("Soundboard", "Soundboard")
		});
		MenuManager.AddCategory("Settings", new List<ButtonInfo>
		{
			Nav("Exit Settings", "Settings"),
			BtnAction("Save Mods", Mods.Save, "Save all enabled mods and settings"),
			BtnAction("Load Mods", Mods.Load, "Load saved mods and settings"),
			BtnAction("Change Menu Color", Mods.CycleMenuColor, "Change color of menu"),
			BtnToggle("Menu Animations", () => { animationsEnabled = true; }, () => { animationsEnabled = false; }, false, "Toggle menu open/close and button press animations"),
			BtnToggle("Rounded Menu", () => { roundedObjects = true; DestroyMenu(); instance.Draw(); }, () => { roundedObjects = false; DestroyMenu(); instance.Draw(); }, false, "Round the menu corners"),
			BtnToggle("Right Hand", Mods.EnableRightHand, Mods.DisableRightHand, false, "Move menu to right hand"),
			BtnToggle("Network Menu", Mods.EnableNetworkMenu, Mods.DisableNetworkMenu, false, "See each others menus"),
			BtnToggle("Toggle Notifications", Mods.ToggleNotifications, Mods.DisableNotifications, false, "Show/hide notifications"),
			BtnToggle("Custom Boards", () => { customBoardsEnabled = true; customBoardsApplied = false; }, () => { customBoardsEnabled = false; customBoardsApplied = false; if ((Object)(object)instance != (Object)null) instance.RestoreOriginalBoardText(); }, false, "Replace in-game message boards with custom text"),
			BtnAction("Clear Notifications", Mods.ClearNotifications, "Remove all on-screen notifications"),
			BtnAction("Notification Time", Mods.CycleNotificationTime, "how long notifications stay on screen"),
			BtnAction("Tag Aura Range", Mods.TagAuraCycleRange, "Cycle tag aura range (0 / 1.5 / 2m)"),
			BtnToggle("Show FPS", () => showFPS = true, () => showFPS = false, false, "Show FPS counter"),
			BtnToggle("Show Session Time", () => showSessionTime = true, () => showSessionTime = false, false, "Show session duration"),
			BtnAction("Change Button Click Sound", () => { buttonSoundIndex = (buttonSoundIndex + 1) % 2; NotifiLib.SendNotification("[<color=green>CHUD</color>] Button sound: " + ((buttonSoundIndex == 0) ? "Normal" : "Custom"), 2); }, "Cycle button click sound"),
			BtnAction("Change Speed Amount", () => Mods.ChangeSpeedBoostAmount(), "Cycle speed boost multiplier"),
			BtnAction("Change Fly Speed", () => Mods.ChangeFlySpeed(), "Cycle fly speed (max 20)"),
			new ButtonInfo { buttonText = "No Mouse Lock", enableMethod = () => Mods.SetWASDFlyNoMouseLock(true), disableMethod = () => Mods.SetWASDFlyNoMouseLock(false), enabled = false, toolTip = "Prevent WASD fly from locking mouse on right click" },
			BtnAction("Change WASD Sense", Mods.ChangeWASDFlyMouseSense, "Cycle WASD fly sensitivity"),
			BtnAction("Change Pull Power", () => Mods.ChangePullModPower(), "Cycle pull mod strength"),
			BtnToggle("PC Guns", Mods.EnablePCGuns, Mods.DisablePCGuns, false, "Use guns with mouse"),
			BtnToggle("PC Button Click", Mods.EnablePCButtonClick, Mods.DisablePCButtonClick, false, "Click buttons with mouse"),
			BtnToggle("see anti cheat reports", Mods.EnableSeeAntiCheatReports, Mods.DisableSeeAntiCheatReports, false, "Show anti-cheat reports")
		});
		MenuManager.AddCategory("Enabled Mods", new List<ButtonInfo>
		{
			Nav("Exit Enabled Mods", "Enabled Mods")
		});
		MenuManager.AddCategory("Movement Mods", new List<ButtonInfo>
		{
			Nav("Exit Movement Mods", "Movement Mods"),
			BtnToggle("Fly", Mods.EnableFly, Mods.DisableFly, false, "Hold B"),
			BtnToggle("Joystick Fly", Mods.JoystickFly, Mods.DisableJoystickFly, false, "Fly with joystick"),
			BtnToggle("WASD Fly", Mods.EnableWASDFly, Mods.DisableWASDFly, false, "Fly with WASD keys"),
			BtnFrameToggle("Speed Boost", Mods.SpeedBoost, Mods.DisableSpeedBoost, "Hold grip to run fast"),
			BtnFrameToggle("No Gravity", Mods.NoGravity, Mods.DisableNoGravity, "Disable gravity"),
			BtnFrameToggle("Noclip", Mods.EnableNoclip, Mods.DisableNoclip, "Walk through walls"),
			BtnFrameAction("Pull Mod", Mods.PullMod, "Pull forward while gripping"),
			BtnFrameAction("Platforms", Mods.Platforms, "Place platforms"),
			BtnFrameAction("Sticky Platforms", Mods.StickyPlatforms, "Sticky ver of plats"),
			BtnGun("TP Gun", Mods.TPGun, Mods.CleanupGun, "Shoot to teleport"),
			BtnAction("Teleport to Stump", Mods.TeleportToSpawn, "Teleport to the forest stump"),
			BtnFrameToggle("Minos Prime", Mods.MinosPrime, Mods.DisableMinosPrime, "Right B to jump, then Right A to slam")
		});
		MenuManager.AddCategory("Visual Mods", new List<ButtonInfo>
		{
			Nav("Exit Visual Mods", "Visual Mods"),
			BtnFrameToggle("Name Tags", Mods.NameTags, Mods.DisableNameTags, "Show names above heads"),
			BtnFrameToggle("ID Name Tags", Mods.IDTags, Mods.DisableIDTags, "Show IDs above heads"),
			BtnFrameToggle("FPS Name Tags", Mods.FPSTags, Mods.DisableFPSTags, "Show FPS above heads"),
			BtnFrameToggle("Platform Name Tags", Mods.PlatformTags, Mods.DisablePlatformTags, "Show platform above heads"),
			BtnFrameToggle("Cosmetic Name Tags", Mods.CosmeticNameTags, Mods.DisableCosmeticNameTags, "Show cosmetics above heads"),
			BtnFrameToggle("ARS Nametags", Mods.EnableARSNameTags, Mods.DisableARSNameTags, "Show people on ARS"),
			BtnFrameToggle("Tracers", Mods.Tracers, Mods.DisableTracers, "Lines towards everyone"),
			BtnFrameToggle("2D Box ESP", Mods.BoxEspRender, Mods.DisableBoxEsp, "Boxes around players"),
			BtnFrameToggle("Skeleton ESP", Mods.SkeletonEsp, Mods.DisableSkeletonEsp, "Draw skeleton lines on players"),
			BtnFrameToggle("Random Color Spaz", Mods.RandomColorSpaz, Mods.DisableRandomColorSpaz, "Change colors fast"),
			BtnFrameToggle("3rd Person", Mods.EnableThirdPerson, Mods.DisableThirdPerson, "Third person view"),
			BtnFrameToggle("Cosmetic Notifier", Mods.CosmeticNotifier, Mods.DisableCosmeticNotifier, "The notis show who has a special cosmetics"),
			BtnAction("lowercase name", () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", "").ToLower(); PhotonNetwork.LocalPlayer.NickName = n; if ((Object)(object)VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, "Make ur name lowercase"),
			BtnAction("Random Capital Name", () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", ""); char[] c = n.ToCharArray(); for (int i = 0; i < c.Length; i++) c[i] = (i % 2 == 0) ? char.ToUpper(c[i]) : char.ToLower(c[i]); PhotonNetwork.LocalPlayer.NickName = new string(c); if ((Object)(object)VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, "make ur name alternating case")
		});
		MenuManager.AddCategory("Useful Mods", new List<ButtonInfo>
		{
			Nav("Exit Useful Mods", "Useful Mods"),
			BtnToggle("Anti Name Ban", Mods.AntiNameBan, Mods.DisableAntiNameBan, false, "Prevent name bans"),
			BtnToggle("Anti AFK", Mods.AntiAFK, Mods.DisableAntiAFK, false, "Prevent AFK kick"),
			BtnToggle("Anti Guardian Grab", Mods.AntiGuardianGrab, Mods.DisableAntiGuardianGrab, false, "Block guardian grab"),
			BtnToggle("Disable Quit Box", Mods.EnableDisableQuitBox, Mods.DisableDisableQuitBox, false, "Disable quit box"),
			BtnToggle("Disable Network Triggers", Mods.EnableDisableNetworkTriggers, Mods.DisableDisableNetworkTriggers, false, "Change maps without leaving"),
			BtnToggle("Block jman sounds", Mods.BlockJmanSounds, Mods.DisableBlockJmanSounds, false, "Block jman sounds"),
			BtnGun("Mute Gun", Mods.MuteGun, Mods.CleanupGun, "Shoot to mute/unmute"),
			BtnAction("Get ID Self", Mods.GetIDSelf, "Copy your ID"),
			BtnToggle("ARS", Mods.EnableARS, Mods.DisableARS, false, "Auto-report system"),
		});
		MenuManager.AddCategory("Room mods", new List<ButtonInfo>
		{
			Nav("Exit Room mods", "Room mods"),
			BtnAction("Join Code MODS", () => Mods.JoinCode("MODS"), "Join MODS room"),
			BtnAction("Join Code MOD", () => Mods.JoinCode("MOD"), "Join MOD room"),
			BtnAction("Join Code chud", () => Mods.JoinCode("chud"), "Join chud room"),
			BtnAction("Create pub K.K.K", () => Mods.CreateRoom("K.K.K", true), ""),
			BtnAction("Create pub P.e.n.i.s676767", () => Mods.CreateRoom("P.e.n.i.s676767", true), ""),
			BtnAction("Create pub FEMBOYS :3", () => Mods.CreateRoom("FEMBOYS :3", true), ""),
		});
		MenuManager.AddCategory("Fun Mods", new List<ButtonInfo>
		{
			Nav("Exit Fun Mods", "Fun Mods"),
			BtnToggle("Unlock VIM/Subscription", Mods.UnlockVim, Mods.DisableUnlockVim, false, "Unlock VIM features"),
			BtnAction("Unlock All Cosmetics", () => { Mods.UnlockAllCosmetics(); UnlockAllCosmeticsPatch.enabled = true; }, "Unlocks all cosmetics and lets you see others' Cosmetx cosmetics"),
			BtnToggle("Bitcrunch Mic", Mods.BitcrunchMic, Mods.DisableBitcrunchMic, false, "Makes ur mic sound bad"),
			BtnFrameToggle("Boop", Mods.Boop, Mods.DisableBoop, "Play's a noise when booping someone"),
			BtnGun("GetPlayerID Gun", Mods.GetPlayerIDGun, Mods.CleanupGun, "Shoot to copy ID"),
			BtnGun("Lag Gun", Mods.LagGun, Mods.StopLagGunFull, "Lags whoever u shoot, not very good only works on quest"),


			new ButtonInfo { buttonText = "Paintbrawl Aimbot", enableMethod = () => GetLaunchPatch.enabled = true, disableMethod = () => GetLaunchPatch.enabled = false, enabled = false, toolTip = "Redirects your slingshot to the closest player" },
		});
		MenuManager.AddCategory("Rig Mods", new List<ButtonInfo>
		{
			Nav("Exit Rig Mods", "Rig Mods"),
		BtnFrameToggle("Ghost Monke", Mods.GhostMonke, Mods.DisableGhostMonke, "Press B to freeze your rig"),
		BtnFrameToggle("Invis Monke", Mods.InvisMonke, Mods.DisableInvisMonke, "Press A to be invisible"),
			BtnToggle("Backflip", Mods.EnableBackflip, Mods.DisableBackflip, false, "Press B"),
			BtnToggle("Frontflip", Mods.EnableFrontflip, Mods.DisableFrontflip, false, "Press B"),
		});
		MenuManager.AddCategory("Infection Mods", new List<ButtonInfo>
		{
			Nav("Exit Infection Mods", "Infection Mods"),
			BtnLockOnGun("Tag Gun", Mods.TagGun, Mods.CleanupGun, "Its tag gun"),
			BtnFrameToggle("Tag All", Mods.TagAll, Mods.DisableTagAll, "Tags everyone"),
			BtnFrameToggle("Tag Aura", Mods.TagAura, Mods.DisableTagAura, "Auto-tag players around you"),
			BtnFrameToggle("Tag Aura Visual", Mods.TagAuraVisual, Mods.DisableTagAuraVisual, "Show aura range visual")
		});
		MenuManager.AddCategory("Master Mods", new List<ButtonInfo>
		{
			Nav("Exit Master Mods", "Master Mods"),
			new ButtonInfo { buttonText = "Not master client", method = null, enabled = false, nontoggleable = true, toolTip = "Your current master client status" },
			BtnFrameAction("Tag While Not Tagged", Mods.TagWhileNotTagged, "tag while not infected"),
			BtnAction("Untag Self", Mods.UntagSelf, "untag urself"),
			BtnToggle("Spaz Self", Mods.SpazSelf, Mods.DisableSpazSelf, false, "Tag and untag urself"),
			BtnToggle("Spaz All", Mods.SpazAll, Mods.DisableSpazAll, false, "Tag and untag everyone"),
			BtnGun("Untag Gun", Mods.UntagGun, Mods.CleanupGun, "Shoot infected players to untag them"),
			BtnFrameToggle("Grab All Bugs", Mods.GrabAllBugs, Mods.DisableGrabAllBugs, "Grab all bugs with your hand"),
			BtnFrameToggle("Grab Green Bug", Mods.GrabGreenBug, Mods.DisableGrabGreenBug, "Grab green bugs with your hand"),
			BtnFrameToggle("Grab Doug the Bug", Mods.GrabDougBug, Mods.DisableGrabDougBug, "Grab doug the bug with your hand"),
			BtnFrameToggle("Grab Gold Bug", Mods.GrabGoldBug, Mods.DisableGrabGoldBug, "Grab gold bugs with your hand"),
			BtnFrameToggle("Spaz Bugs", Mods.SpazBugs, Mods.DisableSpazBugs, "Bugs spaz between your hands with random rotation"),
			BtnToggle("Gold Doug Cosmetic", Mods.GoldDougCosmetic, Mods.DisableGoldDougCosmetic, false, "Gold bug sits on your right arm"),
			BtnToggle("Break Guardian", Mods.BreakGuardian, Mods.DisableBreakGuardian, false, "No Guardian??"),
			BtnAction("Guardian Self", Mods.GuardianSelf, "Make yourself guardian"),
			BtnFrameToggle("Guardian Grab All", Mods.GuardianGrabAll, Mods.DisableGuardianGrabAll, "Pull players toward your hand on grip")
		});
		MenuManager.AddCategory("Console Mods", new List<ButtonInfo>
		{
			Nav("Exit Console Mods", "Console Mods"),
			BtnGun("Kick Gun", ConsoleMods.KickGun, Mods.CleanupGun, "Shoot a player to kick them"),
			BtnGun("Silent Kick Gun", ConsoleMods.SilentKickGun, Mods.CleanupGun, "Shoot a player to silently kick them"),
			BtnGun("Fling Gun", ConsoleMods.FlingGun, Mods.CleanupGun, "Shoot a player to fling them"),
			BtnGun("Vibrate Gun", ConsoleMods.VibrateGun, Mods.CleanupGun, "Shoot a player to vibrate their controllers"),
			BtnGun("Lightning Gun", ConsoleMods.LightningGun, Mods.CleanupGun, "Shoot to strike lightning"),
			BtnGun("Jail Gun", ConsoleMods.JailGun, ConsoleMods.JailGunOff, "Trap players in a jail cell"),
			BtnGun("TP All Gun", ConsoleMods.TPAllGun, Mods.CleanupGun, "Teleport everyone to your aim point"),
			BtnGun("Freeze Gun", ConsoleMods.FreezeGun.Fire, ConsoleMods.FreezeGun.Disable, "Hit to freeze/unfreeze players"),
			BtnConsoleToggle("Scale Self", ConsoleMods.ScaleSelf.Enable, ConsoleMods.ScaleSelf.Disable, false, "Right trigger bigger, left trigger smaller"),
			BtnConsoleToggle("Admin Grab", ConsoleMods.AdminGrab.Enable, ConsoleMods.AdminGrab.Disable, false, "Grab players with your hand"),
			BtnConsoleToggle("Admin Grab All", ConsoleMods.AdminGrabAll.Enable, ConsoleMods.AdminGrabAll.Disable, false, "Grab all players at once no matter distance"),
			BtnConsoleToggle("Laser", ConsoleMods.Laser.Enable, ConsoleMods.Laser.Disable, false, "Toggle lasers from your hands"),
			BtnAction("Kick All", ConsoleMods.KickAll, "Kick everyone from lobby"),
			BtnConsoleToggle("Karambit", ConsoleMods.Karambit.Enable, ConsoleMods.Karambit.Disable, false, "This is Karambit"),
			BtnConsoleToggle("Knife", ConsoleMods.Knife.Enable, ConsoleMods.Knife.Disable, false, "This is Knife"),
			BtnConsoleToggle("Rblx Carpet", ConsoleMods.RblxCarpet.Enable, ConsoleMods.RblxCarpet.Disable, false, "This is Rblx Carpet"),
			BtnConsoleToggle("MC Sword", ConsoleMods.McSword.Enable, ConsoleMods.McSword.Disable, false, "This is MC Sword"),
			BtnConsoleToggle("Ban Hammer", ConsoleMods.BanHammer.Enable, ConsoleMods.BanHammer.Disable, false, "This is Ban Hammer"),
			BtnConsoleToggle("Roblox Sword", ConsoleMods.RobloxSword.Enable, ConsoleMods.RobloxSword.Disable, false, "This is Roblox Sword"),
			BtnConsoleToggle("Rainbow Sword", ConsoleMods.RainbowSword.Enable, ConsoleMods.RainbowSword.Disable, false, "This is Rainbow Sword"),
			BtnConsoleToggle("Weird Ender Sword", ConsoleMods.WeirdEnderSword.Enable, ConsoleMods.WeirdEnderSword.Disable, false, "This is Weird Ender Sword"),
			BtnConsoleToggle("Pistol", ConsoleMods.Pistol.Enable, ConsoleMods.Pistol.Disable, false, "This is Pistol"),
			BtnConsoleToggle("Physics Gun", ConsoleMods.PhysicsGun.Enable, ConsoleMods.PhysicsGun.Disable, false, "This is Physics Gun"),
			BtnConsoleToggle("Noli Star", ConsoleMods.NoliStar.Enable, ConsoleMods.NoliStar.Disable, false, "This is Noli Star"),
			BtnConsoleToggle("Bag", ConsoleMods.Bag.Enable, ConsoleMods.Bag.Disable, false, "This is Bag"),
			BtnConsoleToggle("Kormakur", ConsoleMods.Kormakur.Enable, ConsoleMods.Kormakur.Disable, false, "This is Kormakur"),
			BtnConsoleToggle("Coin", ConsoleMods.Coin.Enable, ConsoleMods.Coin.Disable, false, "This is Coin"),
			BtnConsoleToggle("Minos Prime Plush", ConsoleMods.MinosPrime.Enable, ConsoleMods.MinosPrime.Disable, false, "This is Minos Prime Plush"),
			BtnConsoleToggle("Boombox", ConsoleMods.Boombox.Enable, ConsoleMods.Boombox.Disable, false, "This is Boombox"),
			BtnConsoleToggle("Samsung", ConsoleMods.Samsung.Enable, ConsoleMods.Samsung.Disable, false, "This is Samsung"),
			BtnConsoleToggle("TV", ConsoleMods.TV.Enable, ConsoleMods.TV.Disable, false, "This is TV"),
			BtnConsoleToggle("Travis", ConsoleMods.Travis.Enable, ConsoleMods.Travis.Disable, false, "This is Travis"),
			BtnConsoleToggle("Travis (Beach)", ConsoleMods.TravisBeach.Enable, ConsoleMods.TravisBeach.Disable, false, "This is Travis (Beach)"),
			BtnConsoleToggle("Travis (Critters)", ConsoleMods.TravisCritters.Enable, ConsoleMods.TravisCritters.Disable, false, "This is Travis (Critters)"),
			BtnConsoleToggle("Travis (City)", ConsoleMods.TravisCity.Enable, ConsoleMods.TravisCity.Disable, false, "This is Travis (City)"),
			BtnConsoleToggle("Shreksophone", ConsoleMods.Shreksophone.Enable, ConsoleMods.Shreksophone.Disable, false, "This is Shreksophone"),
			BtnConsoleToggle("Carti", ConsoleMods.Carti.Enable, ConsoleMods.Carti.Disable, false, "This is Carti"),
			BtnConsoleToggle("Cherry Bomb", ConsoleMods.CherryBomb.Enable, ConsoleMods.CherryBomb.Disable, false, "This is Cherry Bomb"),

			BtnAction("Destroy All Assets", ConsoleMods.DestroyAllAssets, "Remove all spawned assets"),
			Nav("Console Settings", "Console Settings")
		});
		MenuManager.AddCategory("Console Settings", new List<ButtonInfo>
		{
			Nav("Exit Console Settings", "Console Settings"),
			BtnConsoleToggle("Allow Kick Self", ConsoleMods.AllowKickSelf.Enable, ConsoleMods.AllowKickSelf.Disable, false, "Allow other admins to kick/tp/fling you"),
			BtnConsoleToggle("Allow Teleport Self", ConsoleMods.AllowTpSelf.Enable, ConsoleMods.AllowTpSelf.Disable, false, "Allow other admins to teleport you"),
			BtnConsoleToggle("Detect Console Users", ConsoleMods.DetectConsoleUsers.Enable, ConsoleMods.DetectConsoleUsers.Disable, false, "Auto detect who has console"),
			BtnConsoleToggle("Console Logging", ConsoleMods.ConsoleLogging.Enable, ConsoleMods.ConsoleLogging.Disable, false, "Log console commands, asset spawns, and errors to BepInEx + notification"),
			BtnConsoleToggle("No Admin Indicator", ConsoleMods.NoAdminIndicator.Enable, ConsoleMods.NoAdminIndicator.Disable, false, "Hide your admin crown"),
			BtnAction("Change Laser Color", ConsoleMods.Laser.CycleColor, "Change laser color"),
			BtnConsoleToggle("Full Auto Pistol", ConsoleMods.FullAutoPistol.Enable, ConsoleMods.FullAutoPistol.Disable, false, "Toggle full auto mode for pistol"),
			Nav("Sound", "Sound"),
			Nav("Video", "Video"),
		});
		MenuManager.AddCategory("Sound", ConsoleMods.BuildSoundCategory());
		MenuManager.AddCategory("Video", ConsoleMods.BuildVideoCategory());
		MenuManager.AddCategory("Soundboard", Mods.BuildSoundboardCategory());
		MenuManager.AddCategory("Credits", new List<ButtonInfo>
		{
			Nav("Exit Credits", "Credits"),
			BtnAction("Jolyne", () => NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] Jolyne: Menu owner", 2), "Menu owner"),
			BtnAction("DeepSeek V4", () => NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] DeepSeek V4: Made most of the mods on the menu", 2), "Made most of the mods on the menu"),
			BtnAction("Seralyth", () => NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] Seralyth: has skidded code from Seralyth", 2), "has skidded code from Seralyth"),
			BtnAction("Industry", () => NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] Industry: ARS system by Industry", 2), "ARS system by Industry"),
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
			if (customBoardsEnabled && !customBoardsApplied)
			{
				UpdateCustomBoardText();
				customBoardsApplied = true;
			}
			else if (!customBoardsEnabled)
			{
				customBoardsApplied = false;
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
		bool flag2 = (ybuttonDown && !Mods.right) || (bbuttonDown && Mods.right) || qKeyDown;
		if (flag2)
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
		else if (!flag2 && (Object)(object)menu != (Object)null && !Close)
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
			Mods.InvalidateActiveButtonsCache();
		}
	}

		private void CheckAdminStatus()
		{
			bool flag = PhotonNetwork.LocalPlayer != null && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
			bool flag2 = MenuManager.Categories.Any((MenuCategory c) => c.Name == "Console Mods");
			bool flag3 = false;
			MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Main");
			if (menuCategory != null)
			{
				flag3 = menuCategory.Buttons.Any((ButtonInfo b) => b.buttonText == "Console Mods");
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
						buttonText = "Console Mods",
						method = delegate
						{
							MenuManager.ToggleCategory("Console Mods");
						},
						enabled = false,
						nontoggleable = true,
						toolTip = "Go to Console Mods!"
					});
				}
			}
			else if (!flag && flag3)
			{
				menuCategory?.Buttons.RemoveAll((ButtonInfo b) => b.buttonText == "Console Mods");
				if (MenuManager.CurrentCategoryName == "Console Mods" || MenuManager.CurrentCategoryName == "Console Settings")
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
		for (int i = 0; i < BoardPaths.Length; i++)
		{
			if ((Object)(object)cachedBoardObjects[i] == (Object)null)
			{
				cachedBoardObjects[i] = GameObject.Find(BoardPaths[i]);
				if ((Object)(object)cachedBoardObjects[i] != (Object)null)
					cachedBoardTexts[i] = cachedBoardObjects[i].GetComponent<TextMeshPro>();
			}
			if ((Object)(object)cachedBoardTexts[i] != (Object)null)
			{
				if (string.IsNullOrEmpty(originalBoardTexts[i]))
					originalBoardTexts[i] = cachedBoardTexts[i].text;
				cachedBoardTexts[i].text = CustomBoardTexts[i];
			}
		}
	}

	private void RestoreOriginalBoardText()
	{
		for (int i = 0; i < BoardPaths.Length; i++)
		{
			if (string.IsNullOrEmpty(originalBoardTexts[i])) continue;
			if ((Object)(object)cachedBoardObjects[i] == (Object)null)
			{
				cachedBoardObjects[i] = GameObject.Find(BoardPaths[i]);
				if ((Object)(object)cachedBoardObjects[i] != (Object)null)
					cachedBoardTexts[i] = cachedBoardObjects[i].GetComponent<TextMeshPro>();
			}
			if ((Object)(object)cachedBoardTexts[i] != (Object)null)
				cachedBoardTexts[i].text = originalBoardTexts[i];
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
		Object.Destroy((Object)(object)menu.GetComponent<Collider>());
		Object.Destroy((Object)(object)menu.GetComponent<Renderer>());
		menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
		menuObj = GameObject.CreatePrimitive((PrimitiveType)3);
		Object.Destroy((Object)(object)menuObj.GetComponent<Rigidbody>());
		Object.Destroy((Object)(object)menuObj.GetComponent<Collider>());
		menuObj.transform.parent = menu.transform;
		menuObj.transform.rotation = Quaternion.identity;
		menuObj.transform.localScale = new Vector3(0.1f, 1f, 1f);
		Renderer bgRenderer = menuObj.GetComponent<Renderer>();
		Color bgTop = NormalColor * 0.35f;
		Color bgBot = NormalColor;
		bgRenderer.material = MakeGradientMat(bgTop, bgBot);
		menuObj.transform.position = new Vector3(0.05f, 0f, 0f);
		Shader val = Shader.Find("GorillaTag/UberShader");
		if (roundedObjects)
		{
			RoundGameObject(menuObj, "__background__", bgTop, bgBot);
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
		val5.text = MenuTitle;
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
		Collider val7c = val7.GetComponent<Collider>();
		if (val7c != null) val7c.isTrigger = true;
		val7.transform.parent = menu.transform;
		val7.transform.rotation = Quaternion.identity;
		val7.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
		val7.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
		Color dcTop = DisconnectButtonColor * 0.35f;
		Color dcBot = DisconnectButtonColor;
		val7.GetComponent<Renderer>().material = MakeGradientMat(dcTop, dcBot);
		val7.AddComponent<BtnCollider>().relatedText = "DisconnectingButton";
		if (roundedObjects)
		{
			RoundGameObject(val7, "DisconnectingButton", dcTop, dcBot);
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
		Collider val10c = val10.GetComponent<Collider>();
		if (val10c != null) val10c.isTrigger = true;
		val10.transform.parent = menu.transform;
		val10.transform.rotation = Quaternion.identity;
		val10.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val10.transform.localPosition = new Vector3(0.56f, 0.65f, 0f);
		Color npTop = NextPrevButtonColor * 0.35f;
		Color npBot = NextPrevButtonColor;
		val10.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		val10.AddComponent<BtnCollider>().relatedText = "PreviousPage";
		if (roundedObjects)
		{
			RoundGameObject(val10, "PreviousPage", npTop, npBot);
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
		Collider val13c = val13.GetComponent<Collider>();
		if (val13c != null) val13c.isTrigger = true;
		val13.transform.parent = menu.transform;
		val13.transform.rotation = Quaternion.identity;
		val13.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val13.transform.localPosition = new Vector3(0.56f, -0.65f, 0f);
		val13.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		val13.AddComponent<BtnCollider>().relatedText = "NextPage";
		if (roundedObjects)
		{
			RoundGameObject(val13, "NextPage", npTop, npBot);
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
				Collider val16c = val16.GetComponent<Collider>();
				if (val16c != null) val16c.isTrigger = true;
				val16.transform.parent = menu.transform;
				val16.transform.rotation = Quaternion.identity;
				val16.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
				val16.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - num2);
				val16.AddComponent<BtnCollider>().relatedText = array[num];
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
				Color btnBase = (flag == true) ? ButtonColorEnabled : ButtonColorDisable;
				Color btnTop = btnBase * 0.35f;
				Color btnBot = btnBase;
				val16.GetComponent<Renderer>().material = MakeGradientMat(btnTop, btnBot);
				if (roundedObjects)
				{
					RoundGameObject(val16, array[num], btnTop, btnBot);
				}
				GameObject val17 = new GameObject();
				val17.transform.parent = canvasObj.transform;
				Text val18 = val17.AddComponent<Text>();
				val18.font = MenuFont;
				val18.text = array[num];
				val18.fontSize = 200;
				val18.supportRichText = true;
				((Graphic)val18).color = ((flag == true) ? EnableTextColor : DisableTextColor);
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
		menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f);
	}

	public static Material MakeGradientMat(Color top, Color bot)
	{
		int h = 16;
		Texture2D tex = new Texture2D(2, h, TextureFormat.RGBA32, false);
		Color highlight = Color.Lerp(bot, Color.white, 0.075f);
		for (int y = 0; y < h; y++)
		{
			float t = 1f - Mathf.Abs((float)y / (h - 1) * 2f - 1f);
			tex.SetPixel(0, y, Color.Lerp(bot, highlight, t));
			tex.SetPixel(1, y, Color.Lerp(bot, highlight, t));
		}
		tex.Apply();
		tex.wrapMode = TextureWrapMode.Repeat;
		Material mat = new Material(Shader.Find("Unlit/Texture"));
		mat.mainTexture = tex;
		mat.color = Color.white;
		mat.mainTextureScale = new Vector2(1, 0.5f);
		gradientMaterials.Add(mat);
		return mat;
	}

	internal static void UpdateGradientAnimations(float time)
	{
		float offsetY = time * 0.2f;
		Vector2 offset = new Vector2(0f, offsetY);
		for (int i = gradientMaterials.Count - 1; i >= 0; i--)
		{
			if ((Object)(object)gradientMaterials[i] == (Object)null)
				gradientMaterials.RemoveAt(i);
			else
				gradientMaterials[i].mainTextureOffset = offset;
		}
	}

	public static void RoundGameObject(GameObject obj, string identifier, Color gradientTop, Color gradientBot)
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
		Color midColor = Color.Lerp(gradientTop, gradientBot, 0.5f);
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
		Shader val = Shader.Find("GorillaTag/UberShader");
		foreach (Renderer item in list)
		{
			Material material = item.material;
			material.color = midColor;
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
		gradientMaterials.Clear();
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
			if (category.Name == "Main" || category.Name == "Enabled Mods" || category.Name == "Console Mods" || category.Name == "Console Settings")
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
		if (Time.time - lastButtonPressTime < 0.4f)
		{
			return;
		}
		lastButtonPressTime = Time.time;
		PlayButtonClickSound(Mods.right);
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
		Mods.InvalidateActiveButtonsCache();
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

	internal static void UpdateButtonVisual(string buttonText, bool isEnabled)
	{
		foreach (Transform item in menu.transform)
		{
			Transform val2 = item;
			BtnCollider component = ((Component)val2).GetComponent<BtnCollider>();
			if ((Object)(object)component != (Object)null && component.relatedText == buttonText)
			{
				Renderer component2 = ((Component)val2).GetComponent<Renderer>();
				Color baseColor = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
				component2.material = MakeGradientMat(baseColor * 0.35f, baseColor);
				break;
			}
		}
		Color bc = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
		Color bt = bc * 0.35f;
		Color mid = Color.Lerp(bt, bc, 0.5f);
		if (roundedRenderers.TryGetValue(buttonText, out var value))
		{
			foreach (Renderer item2 in value)
			{
				if ((Object)(object)item2 != (Object)null)
				{
					item2.material.color = mid;
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
				((Graphic)component3).color = (isEnabled ? EnableTextColor : DisableTextColor);
				break;
			}
		}
	}

	internal static void RefreshButtonVisuals()
	{
		if ((Object)(object)menu == (Object)null) return;
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Buttons == null) continue;
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled.HasValue)
					UpdateButtonVisual(button.buttonText, button.enabled.Value);
			}
		}
	}

	private static ButtonInfo Nav(string text, string category) => new()
	{
		buttonText = text,
		method = () => MenuManager.ToggleCategory(category),
		enabled = false,
		nontoggleable = true,
		toolTip = $"Go to {category}"
	};

	private static ButtonInfo BtnAction(string text, Action action, string tip) => new()
	{
		buttonText = text,
		method = action,
		enabled = false,
		nontoggleable = true,
		toolTip = tip
	};

	private static ButtonInfo BtnAction(string text, Action action, string tip, bool isGun, bool isFrameCall) => new()
	{
		buttonText = text,
		method = action,
		enabled = false,
		nontoggleable = true,
		toolTip = tip,
		isGun = isGun,
		isFrameCall = isFrameCall
	};

	private static ButtonInfo BtnToggle(string text, Action enable, Action disable, bool startEnabled, string tip) => new()
	{
		buttonText = text,
		method = enable,
		disableMethod = disable,
		enabled = startEnabled,
		toolTip = tip
	};

	private static ButtonInfo BtnFrameToggle(string text, Action enable, Action disable, string tip) => new()
	{
		buttonText = text,
		method = enable,
		disableMethod = disable,
		isFrameCall = true,
		enabled = false,
		toolTip = tip
	};

	private static ButtonInfo BtnFrameAction(string text, Action action, string tip) => new()
	{
		buttonText = text,
		method = action,
		isFrameCall = true,
		enabled = false,
		toolTip = tip
	};

	private static ButtonInfo BtnGun(string text, Action action, Action cleanup, string tip) => new()
	{
		buttonText = text,
		method = action,
		disableMethod = cleanup,
		isGun = true,
		enabled = false,
		toolTip = tip
	};

	private static ButtonInfo BtnLockOnGun(string text, Action action, Action cleanup, string tip) => new()
	{
		buttonText = text,
		method = action,
		disableMethod = cleanup,
		isGun = true,
		isLockOn = true,
		enabled = false,
		toolTip = tip
	};

	private static ButtonInfo BtnConsoleToggle(string text, Action enable, Action disable, bool startEnabled, string tip) => new()
	{
		buttonText = text,
		enableMethod = enable,
		method = ConsoleMods.Run,
		disableMethod = disable,
		enabled = startEnabled,
		toolTip = tip
	};
}
