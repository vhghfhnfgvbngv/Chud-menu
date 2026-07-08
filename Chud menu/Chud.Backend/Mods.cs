using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GTAG_NotificationLib;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Pointer = UnityEngine.InputSystem.Pointer;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Random = UnityEngine.Random;

namespace Chud.Backend;

internal class Mods : MonoBehaviour
{
	private struct TransformSnapshot
	{
		public Vector3 headPos;

		public Quaternion headRot;

		public Vector3 leftHandPos;

		public Quaternion leftHandRot;

		public Vector3 rightHandPos;

		public Quaternion rightHandRot;

		public float leftIndexT;

		public float leftMiddleT;

		public float leftThumbT;

		public float rightIndexT;

		public float rightMiddleT;

		public float rightThumbT;
	}

	[Serializable]
	public class ModConfig
	{
		public List<string> EnabledButtons = new List<string>();

		public float FlySpeed = 8f;

		public int SpeedboostCycle = 1;

		public int PullPowerInt = 0;

		public int LaserColorIndex = 0;

		public float WasdFlyMouseSense = 1f;

		public bool Right = false;

		public int MenuColorIndex = 0;

		public int NotificationTimeIndex = 3;

		public int ButtonSoundIndex = 0;

		public bool AnimationsEnabled = true;

		public bool ConsoleAllowKickSelf = false;
		public bool ConsoleAllowTpSelf = true;
		public bool ConsoleDisableFlingSelf = false;
		public bool ConsoleLaserEnabled = false;
		public bool ConsoleAutoDetectConsoleUsers = false;
		public bool ConsoleFullAutoPistol = false;
		public bool ConsoleMuteRainbowSword = false;

		public int SelectedSoundIndex = 0;
		public int SelectedVideoIndex = 0;

		public bool RoundedObjects = false;
		public bool ShowFPS = true;
		public bool ShowSessionTime = true;
	}

	public struct MenuColors
	{
		public Color NormalColor;

		public Color ButtonColorEnabled;

		public Color ButtonColorDisable;

		public Color EnableTextColor;

		public Color DisableTextColor;

		public Color NextPrevButtonColor;

		public Color MenuTitleColor;
	}

	public class RemoteMenuState
	{
		public Player player;

		public GameObject displayObject;

		public string category = "Main";

		public int page;

		public MenuColors menuColors;

		public int menuColorIndex;

		public Vector3 position;

		public Quaternion rotation;

		public Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();

		public float lastStateTime;

		public float lastPosTime;

		public bool closing;

		public bool animationsEnabled = true;
	}

	public static Mods instance;

	private static Shader _cachedGuiTextShader;
	private static Shader CachedGuiTextShader
	{
		get
		{
			if ((Object)(object)_cachedGuiTextShader == (Object)null)
				_cachedGuiTextShader = Shader.Find("GUI/Text Shader");
			return _cachedGuiTextShader;
		}
	}

	private static bool joystickFlyActive = false;

	public static float flySpeed = 8f;

	private static bool wasdFlyActive = false;

	private static bool wasdFlyNoMouseLock = false;

	private static float wasdFlyMouseSense = 1f;

	private static float wasdPitch;

	private static bool flyActive = false;

	public static int speedboostCycle = 1;

	public static float jspeed = 7.5f;

	public static float jmulti = 1.1f;

	public static int pullPowerInt;

	private static float pullPower = 0.05f;

	private static readonly Dictionary<bool, bool> previousTouchingGround = new Dictionary<bool, bool>();

	internal static bool ghostMonkeOn = false;

	private static bool ghostMonkeLastPress = false;

	private static Vector3 ghostMonkeFrozenPos;

	private static Quaternion ghostMonkeFrozenRot;

	private static TransformSnapshot ghostMonkeSnapshot;

	internal static bool invisMonkeOn = false;

	private static Vector3 invisMonkeSavedPos;

	private static bool invisMonkeLastPress = false;

	private static bool invisMonkeSkinsDisabled = false;

	private static bool antiNameBanApplied = true;

	private static bool bitcrunchMicActive = false;

	private static int bitcrunchOrigSampleRate = 16000;

	private static int bitcrunchOrigBitrate = 24000;

	private static bool boopActive = false;

	private static bool boopLastL;

	private static bool boopLastR;

	private static float boopCooldown;

	private static int randomColorSpazTick;

	private static bool minosPrimedForSlam = false;

	private static bool minosWaitingForImpact = false;

	private static AudioClip minosCrushClip = null;

	private static AudioClip minosSlamClip = null;

	private static bool minosClipsLoaded = false;

	private static bool minosSecondaryWasDown = false;

	private static bool minosPrimaryWasDown = false;

	private static Coroutine minosRestoreCoroutine = null;

	private static AudioSource minosLocalSource = null;

	private static string MinosSoundDir => Path.Combine(Environment.CurrentDirectory, WristMenu.FolderName) + "\\";

	private const string MinosCrushUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/CRUSH%20!.mp3";

	private const string MinosSlamUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/slam%20sound.mp3";

	private static GameObject FreeCamObject;

	private static readonly Dictionary<VRRig, GameObject> boxEspObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> nameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> fpsNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> idNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> platformNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> arsTagObjects = new Dictionary<VRRig, GameObject>();

	private static HashSet<string> arsPlayersToReport = new HashSet<string>();

	private static bool arsActive = false;

	private static bool arsDownloaded = false;

	private static bool arsDownloading = false;

	private static readonly HttpClient arsHttpClient = new HttpClient();

	public static Font comicSansFont;

	private static FieldInfo _fpsField;

	private static readonly Dictionary<VRRig, int> tagStackCounter = new Dictionary<VRRig, int>();

	private static int tagStackFrame = -1;

	private static readonly Dictionary<string, string> cosmeticNames = new Dictionary<string, string>
	{
		{ "LBAAK.", "Dev stick" },
		{ "LBANI.", "AA BADGE" },
		{ "LMAPY.", "Forest guide" },
		{ "LBADE.", "Finger painter" },
		{ "LBAGS.", "illustrator" },
		{ "LMAYQ.", "Golden gorilla ticket" },
		{ "LBARJ.", "COMMUNITY RIBBON" },
		{ "LBASS.", "PARTY ILLUSTRATOR BADGE" },
		{ "LMAJA.", "GT MONKE PLUSH" },
		{ "LMAYT.", "LAVA MONKE DOUGHBOI" },
		{ "LMBAO.", "Gorillacon golden phone" }
	};

	private static readonly Dictionary<VRRig, GameObject> cosmeticNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static FieldInfo _ownedCosmeticsField;

	private static bool arsNameTagsActive = false;

	private static string arsLastCheckedRoom = "";

	private static bool cosmeticNotifierActive = false;

	private static HashSet<string> cosmeticNotifierNotified = new HashSet<string>();

	private static bool notificationsEnabled = true;

	public static int menuColorIndex = 0;

	private static int menuColorUpdateCounter = 0;

	private static int notificationDecayTime = 150;

	private static int notificationTimeIndex = 3;

	private static readonly int[] notificationTimeValues = new int[6] { 75, 100, 150, 200, 300, 500 };

	private static readonly string[] notificationTimeNames = new string[6] { "1.5s", "2s", "3s", "4s", "6s", "10s" };

	private static Coroutine flingGunCoroutine;

	private static int flingTargetActor;

	private static float laserDelayLeft;

	private static float laserDelayRight;

	private static bool lastLaserLeft;

	private static bool lastLaserRight;

	private static bool laserApplied = false;

	private static bool lastPistolTrigger;

	private static float pistolFireDelay;

	private static bool lastCoinSecondary;

	private static float pauseSfxBH;
	private static float slashDelayBH;
	private static bool lastVelTooHighBH;
	private static float pauseSfxRS;
	private static float slashDelayRS;
	private static bool lastVelTooHighRS;
	private static VRRig physGunTargetHoldVRRig;
	private static float physGunRigDistance;
	private static float physGunStandaloneTriggerDelay;
	private static float physGunPositionDelay;
	private static GameObject physGunCrosshair;
	private static bool physGunLastGrip;

	private static int banHammerId = -1;

	private static int pistolId = -1;

	private static int rainbowSwordId = -1;

	private static int physicsGunId = -1;

	private static int coinId = -1;

	private static int jailId = -1;

	private static int laserColorIndex = 0;

	private static readonly Color[] laserColors = (Color[])(object)new Color[6]
	{
		new Color(0f, 0f, 1f),
		new Color(1f, 0f, 0f),
		new Color(0.5f, 0.2f, 0.8f),
		new Color(0.9f, 0.4f, 0.9f),
		new Color(0.9f, 0.7f, 0.1f),
		new Color(0.4f, 0.4f, 0.4f)
	};



	private static Vector3 launchPlayerGunReturnPos;

	private static int launchPlayerGunFramesLeft = 0;

	private static Harmony vimHarmony;

	private static float lastUntagNotif = 0f;

	private static int tagGunFramesUntilTag;
	private static bool tagGunTriggerWasDown = false;
	private static VRRig tagGunLockedTarget = null;

	private static VRRig tagAllTarget;
	private static int tagAllFramesUntilTag;
	private static List<VRRig> tagAllTargets;
	private static int tagAllIndex;

	private static float lastUntagSelfTime;

	private static float tagUntaggedCooldown = 0f;

	private static Vector3 stumpPosition = new Vector3(-66.871f, 12.086f, -82.637f);

	private static bool spazAllActive = false;

	private static int spazAllFrameCounter = 0;

	private static bool spazSelfActive = false;


	private static bool gunTriggerWasDown = false;

	private static Camera pcGunCamera;

	public static bool blockJmanSounds = false;

	public static bool antiGuardianGrab = false;

	public static bool seeAntiCheatReports = false;

	public static readonly Dictionary<string, int> antiCheatReportCounts = new Dictionary<string, int>();

	private static bool pcButtonClickEnabled = false;

	private static Vector3? pcButtonOldLocalPosition;

	private static int? noInvisLayerMask;

	private static bool pcGunsEnabled = false;

	public static bool NetworkMenuEnabled = false;

	private static readonly Dictionary<int, RemoteMenuState> remoteMenus = new Dictionary<int, RemoteMenuState>();

	private static float networkMenuSyncTimer;

	private const float NETWORK_MENU_POS_INTERVAL = 0f;

	private const float NETWORK_MENU_FULL_INTERVAL = 0f;

	private static readonly Dictionary<string, GameObject> remotePlatforms = new Dictionary<string, GameObject>();

	public static int change7 = 3;

	public static bool right = false;

	public static int ButtonSound = 67;

	public static GameObject pointer = null;

	public static LineRenderer Line;

	public static RaycastHit raycastHit;

	public static bool hand = false;

	public static bool hand1 = false;

	private static readonly Dictionary<Player, LineRenderer> tracerLines = new Dictionary<Player, LineRenderer>();

	private static readonly Dictionary<Player, LineRenderer[]> skeletonLines = new Dictionary<Player, LineRenderer[]>();

	private static int noclipCacheFrame = 0;

	private static MeshCollider[] noclipCache = (MeshCollider[])(object)new MeshCollider[0];

	private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);

	private static bool once_left;

	private static bool once_right;

	private static bool once_left_false;

	private static bool once_right_false;

	private static GameObject jump_left_local = null;

	private static GameObject jump_right_local = null;

	private static bool stickyRightActive = false;

	private static bool stickyLeftActive = false;

	public static bool RPlat;

	public static bool LPlat;

	private static bool grabGreenBugActive = false;

	private static bool grabDougBugActive = false;

	private static bool grabGoldBugActive = false;

	private static bool grabAllBugsActive = false;

	private static bool grabSpazBugActive = false;

	private static bool goldDougCosmeticActive = false;
	private static ThrowableBug goldDougBug = null;
	private static RequestableOwnershipGuard goldDougGuard = null;
	private static IRequestableOwnershipGuardCallbacks goldDougDeny = null;
	private static bool goldDougWasHeld = false;

	private static float grabBugLastScan;

	private static readonly List<ThrowableBug> cachedGrabBugs = new List<ThrowableBug>();

	private const float GRAB_BUG_SCAN_INTERVAL = 3f;


	public static string ConfigPath => WristMenu.FolderName + "\\Config.json";

	private static Transform LeftHandTransform => GTPlayer.Instance.LeftHand.controllerTransform;

	private void Awake()
	{
		instance = this;
	}

	public static void JoystickFly()
	{
		joystickFlyActive = true;
	}

	public static void ChangeFlySpeed(bool positive = true)
	{
		float[] array = new float[7] { 2f, 5f, 8f, 11f, 14f, 17f, 20f };
		string[] array2 = new string[7] { "2", "5", "8", "11", "14", "17", "20" };
		int num = Array.IndexOf(array, flySpeed);
		if (num < 0)
		{
			num = 2;
		}
		num = ((!positive) ? (num - 1) : (num + 1));
		num %= array.Length;
		if (num < 0)
		{
			num = array.Length - 1;
		}
		flySpeed = array[num];
		NotifiLib.SendNotification("[<color=white>[</color><color=blue>SETTINGS</color><color=white>] Fly Speed: " + array2[num] + "</color>");
	}

	public static void DisableJoystickFly()
	{
		((Component)GTPlayer.Instance).GetComponent<Rigidbody>().useGravity = true;
		joystickFlyActive = false;
	}

	public static void EnableWASDFly()
	{
		wasdFlyActive = true;
		wasdPitch = 0f;
	}

	public static void DisableWASDFly()
	{
		wasdFlyActive = false;
		Cursor.lockState = (CursorLockMode)0;
		Cursor.visible = true;
	}

	public static void EnableFly()
	{
		flyActive = true;
	}

	public static void DisableFly()
	{
		flyActive = false;
	}

	private static void UpdateFly()
	{
		if (flyActive)
		{
			Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
			if (!((Object)(object)rigidbody == (Object)null) && (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton)
			{
				Transform transform = ((Component)GTPlayer.Instance).transform;
				transform.position += ((Component)GorillaTagger.Instance.headCollider).transform.forward * (Time.deltaTime * flySpeed);
				rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	public static void SetWASDFlyNoMouseLock(bool on)
	{
		wasdFlyNoMouseLock = on;
		NotifiLib.SendNotification("[<color=white>[</color><color=blue>SETTINGS</color><color=white>] No Mouse Lock: " + (wasdFlyNoMouseLock ? "ON" : "OFF") + "</color>");
	}

	public static void ChangeWASDFlyMouseSense()
	{
		float[] array = new float[5] { 0.5f, 1f, 1.5f, 2f, 3f };
		int num = Array.IndexOf(array, wasdFlyMouseSense);
		if (num < 0)
		{
			num = 1;
		}
		num = (num + 1) % array.Length;
		wasdFlyMouseSense = array[num];
		NotifiLib.SendNotification("[<color=white>[</color><color=blue>SETTINGS</color><color=white>] WASD Mouse Sense: " + wasdFlyMouseSense.ToString("0.0") + "</color>");
	}

	private static void UpdateWASDFly()
	{
		if (!wasdFlyActive)
		{
			return;
		}
		Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
		if ((Object)(object)rigidbody == (Object)null)
		{
			return;
		}
		rigidbody.useGravity = false;
		Transform transform = ((Component)GTPlayer.Instance.headCollider).transform;
		Transform transform2 = ((Component)GTPlayer.Instance).transform;
		Vector3 val = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		Vector3 normalized = val.normalized;
		val = Vector3.ProjectOnPlane(transform.right, Vector3.up);
		Vector3 normalized2 = val.normalized;
		Vector3 val2 = Vector3.zero;
		bool flag = false;
		Keyboard current = Keyboard.current;
		if (current != null)
		{
			if (((ButtonControl)current.wKey).isPressed)
			{
				val2 += normalized;
			}
			if (((ButtonControl)current.sKey).isPressed)
			{
				val2 -= normalized;
			}
			if (((ButtonControl)current.aKey).isPressed)
			{
				val2 -= normalized2;
			}
			if (((ButtonControl)current.dKey).isPressed)
			{
				val2 += normalized2;
			}
			if (((ButtonControl)current.spaceKey).isPressed)
			{
				val2 += Vector3.up;
				flag = true;
			}
			if (current.ctrlKey.isPressed)
			{
				val2 -= Vector3.up;
				flag = true;
			}
		}
		if (!flag)
		{
			rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z);
		}
		Vector3 val3 = ((val2.sqrMagnitude > 0.01f) ? (val2.normalized * flySpeed) : Vector3.zero);
		rigidbody.linearVelocity = Vector3.Lerp(rigidbody.linearVelocity, val3, 0.12875f);
		Mouse current2 = Mouse.current;
		if (current2 != null && current2.rightButton.isPressed)
		{
			if (!wasdFlyNoMouseLock)
			{
				Cursor.lockState = (CursorLockMode)1;
				Cursor.visible = false;
			}
			Vector2 val4 = ((InputControl<Vector2>)(object)((Pointer)current2).delta).ReadValue() * wasdFlyMouseSense * 0.15f;
			transform2.Rotate(Vector3.up, val4.x, (Space)0);
			wasdPitch = Mathf.Clamp(wasdPitch - val4.y, -90f, 90f);
			transform.localRotation = Quaternion.Euler(wasdPitch, 0f, 0f);
		}
		else if (!wasdFlyNoMouseLock && (int)Cursor.lockState == 1)
		{
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = true;
		}
	}

	public static void NoGravity()
	{
		Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
		component.useGravity = false;
		if (component.linearVelocity.y < 0f)
		{
			component.linearVelocity = new Vector3(component.linearVelocity.x, 0f, component.linearVelocity.z);
		}
	}

	public static void DisableNoGravity()
	{
		((Component)GTPlayer.Instance).GetComponent<Rigidbody>().useGravity = true;
	}

	public static void Platforms()
	{
		PlatformsThing(invis: false, false);
	}

	public static void StickyPlatforms()
	{
		PlatformsThing(invis: false, true);
	}

	public static void GrabGreenBug()
	{
		grabGreenBugActive = !grabGreenBugActive;
	}

	public static void DisableGrabGreenBug()
	{
		grabGreenBugActive = false;
	}

	public static void GrabDougBug()
	{
		grabDougBugActive = !grabDougBugActive;
	}

	public static void DisableGrabDougBug()
	{
		grabDougBugActive = false;
	}

	public static void GrabGoldBug()
	{
		grabGoldBugActive = !grabGoldBugActive;
	}

	public static void DisableGrabGoldBug()
	{
		grabGoldBugActive = false;
	}

	public static void GrabAllBugs()
	{
		grabAllBugsActive = !grabAllBugsActive;
	}

	public static void DisableGrabAllBugs()
	{
		grabAllBugsActive = false;
	}

	public static void SpazBugs()
	{
		grabSpazBugActive = !grabSpazBugActive;
	}

	public static void DisableSpazBugs()
	{
		grabSpazBugActive = false;
	}

	public static void GoldDougCosmetic()
	{
		goldDougCosmeticActive = true;
		goldDougBug = null;
		goldDougGuard = null;
		goldDougDeny = null;
		goldDougWasHeld = false;
	}

	public static void DisableGoldDougCosmetic()
	{
		goldDougCosmeticActive = false;
		if (goldDougGuard != null && goldDougDeny != null)
		{
			goldDougGuard.RemoveCallbackTarget(goldDougDeny);
		}
		goldDougBug = null;
		goldDougGuard = null;
		goldDougDeny = null;
	}

	private static void UpdateGoldDougCosmetic()
	{
		if (!goldDougCosmeticActive)
			return;

		if ((Object)(object)goldDougBug == (Object)null || (Object)(object)goldDougBug.gameObject == (Object)null)
		{
			ThrowableBug[] allBugs = Resources.FindObjectsOfTypeAll<ThrowableBug>();
			foreach (ThrowableBug bug in allBugs)
			{
				if (bug.name != "Floating Bug Holdable") continue;
				try
				{
					Transform model = bug.transform.Find("model/PlumpBeetle");
					if ((Object)(object)model == (Object)null) continue;
					SkinnedMeshRenderer renderer = model.GetComponent<SkinnedMeshRenderer>();
					if ((Object)(object)renderer == (Object)null || (Object)(object)renderer.material == (Object)null) continue;
					if (!renderer.material.name.Contains("PlumpBeetle_Gold")) continue;
					goldDougBug = bug;
					break;
				}
				catch { }
			}
			if ((Object)(object)goldDougBug == (Object)null)
				return;
		}

		ThrowableBug b = goldDougBug;
		if ((Object)(object)b == (Object)null || (Object)(object)b.gameObject == (Object)null)
		{
			goldDougBug = null;
			return;
		}

		if (!b.IsMyItem())
			b.WorldShareableRequestOwnership();

		b.disableStealing = true;

		if (goldDougGuard == null)
		{
			goldDougGuard = b.GetComponentInParent<RequestableOwnershipGuard>();
			if ((Object)(object)goldDougGuard != (Object)null)
			{
				goldDougDeny = new GoldDougDenyOwnership();
				goldDougGuard.AddCallbackTarget(goldDougDeny);
			}
		}

		bool inHand = b.currentState == TransferrableObject.PositionState.InRightHand || b.currentState == TransferrableObject.PositionState.InLeftHand;

		if (inHand)
		{
			goldDougWasHeld = true;
			return;
		}

		if (goldDougWasHeld)
		{
			goldDougWasHeld = false;
			b.disableStealing = true;
		}

		Transform arm = VRRig.LocalRig.myBodyDockPositions.rightArmTransform;
		if ((Object)(object)arm != (Object)null)
		{
			b.transform.SetParent(arm);
			b.transform.localPosition = new Vector3(0f, -0.06f, -0.02f);
			b.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		}
	}

	private sealed class GoldDougDenyOwnership : IRequestableOwnershipGuardCallbacks
	{
		public bool OnOwnershipRequest(NetPlayer fromPlayer) => false;
		public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer) => false;
		public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer) { }
		public void OnMyOwnerLeft() { }
		public void OnMyCreatorLeft() { }
	}

	public static void UpdateGrabBugs()
	{
		if (!grabGreenBugActive && !grabDougBugActive && !grabGoldBugActive && !grabAllBugsActive && !grabSpazBugActive)
			return;

		bool rightGrip = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
		bool leftGrip = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
		bool anyGrip = rightGrip || leftGrip;

		if (!anyGrip && !grabSpazBugActive)
			return;

		if (Time.time > grabBugLastScan + GRAB_BUG_SCAN_INTERVAL)
		{
			grabBugLastScan = Time.time;
			cachedGrabBugs.Clear();
			cachedGrabBugs.AddRange(Resources.FindObjectsOfTypeAll<ThrowableBug>());
		}

		Transform rightHand = GorillaTagger.Instance.rightHandTransform;
		Transform leftHand = GorillaTagger.Instance.leftHandTransform;
		Transform hand = rightGrip ? rightHand : leftHand;

		for (int i = cachedGrabBugs.Count - 1; i >= 0; i--)
		{
			ThrowableBug bug = cachedGrabBugs[i];
			if ((Object)(object)bug == (Object)null)
			{
				cachedGrabBugs.RemoveAt(i);
				continue;
			}
			if (bug.name != "Floating Bug Holdable")
				continue;

			try
			{
				if (grabSpazBugActive)
				{
					if (!bug.IsMyItem())
						bug.WorldShareableRequestOwnership();
					float phase = (float)(bug.GetInstanceID() % 97) * 0.010309f;
					float t = (Mathf.Sin((Time.time + phase) * 12f) + 1f) * 0.5f;
					bug.transform.position = Vector3.Lerp(leftHand.position, rightHand.position, t);
					bug.transform.rotation = Random.rotation;
					continue;
				}

				if (!anyGrip)
					continue;

				Transform model = bug.transform.Find("model/PlumpBeetle");
				if ((Object)(object)model == (Object)null) continue;
				SkinnedMeshRenderer renderer = model.GetComponent<SkinnedMeshRenderer>();
				if ((Object)(object)renderer == (Object)null || (Object)(object)renderer.material == (Object)null) continue;
				string matName = renderer.material.name;
				bool isGold = matName.Contains("PlumpBeetle_Gold");
				bool isGreen = !isGold && matName.Contains("PlumpBeetle2");
				bool isDoug = !isGold && !isGreen && matName.Contains("PlumpBeetle");
				bool shouldGrab = grabAllBugsActive || (grabGreenBugActive && isGreen) || (grabDougBugActive && isDoug) || (grabGoldBugActive && isGold);

				if (!shouldGrab)
					continue;

				if (!bug.IsMyItem())
					bug.WorldShareableRequestOwnership();

				Rigidbody rb = bug.GetComponent<Rigidbody>();
				if ((Object)(object)rb != (Object)null)
					rb.position = hand.position;
				else
					bug.transform.position = hand.position;

				if (!float.IsPositiveInfinity(bug.maxDistanceFromOriginBeforeRespawn))
					bug.maxDistanceFromOriginBeforeRespawn = float.MaxValue;
				if (!float.IsPositiveInfinity(bug.maxDistanceFromTargetPlayerBeforeRespawn))
					bug.maxDistanceFromTargetPlayerBeforeRespawn = float.MaxValue;
			}
			catch { }
		}
	}

	public static void Noclip()
	{
		noclipCacheFrame++;
		if (noclipCacheFrame % 60 == 0 || noclipCache.Length == 0)
		{
			noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
		}
		MeshCollider[] array = noclipCache;
		foreach (MeshCollider val in array)
		{
			if (!((Object)(object)val == (Object)null))
			{
				((Collider)val).enabled = !WristMenu.bbuttonDown;
			}
		}
	}

	public static void NoclipOff()
	{
		noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
		MeshCollider[] array = noclipCache;
		foreach (MeshCollider val in array)
		{
			if (!((Object)(object)val == (Object)null))
			{
				((Collider)val).enabled = true;
			}
		}
	}

	public static void ChangeSpeedBoostAmount(bool positive = true)
	{
		float[] array = new float[5] { 2f, 7.5f, 8f, 9f, 200f };
		float[] array2 = new float[5] { 0.5f, 1.1f, 1.5f, 2f, 10f };
		string[] array3 = new string[5] { "Slow", "Normal", "Middle", "Fast", "Ultra Fast" };
		if (positive)
		{
			speedboostCycle++;
		}
		else
		{
			speedboostCycle--;
		}
		speedboostCycle %= array.Length;
		if (speedboostCycle < 0)
		{
			speedboostCycle = array.Length - 1;
		}
		jspeed = array[speedboostCycle];
		jmulti = array2[speedboostCycle];
		NotifiLib.SendNotification("[<color=orange>MOVEMENT</color>] Speed: " + array3[speedboostCycle]);
	}

	public static void SpeedBoost()
	{
		float maxJumpSpeed = jspeed;
		float jumpMultiplier = jmulti;
		GTPlayer.Instance.maxJumpSpeed = maxJumpSpeed;
		GTPlayer.Instance.jumpMultiplier = jumpMultiplier;
		Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
		if (GTPlayer.Instance.BodyOnGround && component.linearVelocity.y > 0f)
		{
			component.linearVelocity = new Vector3(component.linearVelocity.x, 0f, component.linearVelocity.z);
		}
	}

	public static void DisableSpeedBoost()
	{
		GTPlayer.Instance.maxJumpSpeed = 6.5f;
		GTPlayer.Instance.jumpMultiplier = 1.1f;
	}

	public static void ChangePullModPower(bool positive = true)
	{
		float[] array = new float[4] { 0.05f, 0.1f, 0.2f, 0.4f };
		string[] array2 = new string[4] { "Normal", "Medium", "Strong", "Powerful" };
		if (positive)
		{
			pullPowerInt++;
		}
		else
		{
			pullPowerInt--;
		}
		pullPowerInt %= array2.Length;
		if (pullPowerInt < 0)
		{
			pullPowerInt = array2.Length - 1;
		}
		pullPower = array[pullPowerInt];
		NotifiLib.SendNotification("[<color=orange>MOVEMENT</color>] Pull power: " + array2[pullPowerInt]);
	}

	private static void ProcessPullHand(bool left)
	{
		if (!(left ? (!((ControllerInputPoller)ControllerInputPoller.instance).leftGrab) : (!((ControllerInputPoller)ControllerInputPoller.instance).rightGrab)))
		{
			bool flag = GTPlayer.Instance.IsHandTouching(left);
			previousTouchingGround.TryGetValue(left, out var value);
			if (!flag && value)
			{
				Vector3 up = Vector3.up;
				Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
				Vector3 val = GTVector3Extensions.X_Z(component.linearVelocity);
				Transform transform = ((Component)GTPlayer.Instance).transform;
				Vector3 position = transform.position;
				Vector3 val2 = val - up * Vector3.Dot(val, up);
				transform.position = position + val2.normalized * (val.magnitude / GTPlayer.Instance.maxJumpSpeed * (pullPower * 5f)) * GTPlayer.Instance.scale;
			}
			previousTouchingGround[left] = flag;
		}
	}

	public static void PullMod()
	{
		ProcessPullHand(left: false);
		ProcessPullHand(left: true);
	}

	private static void TakeRigSnapshot(out TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		s = default(TransformSnapshot);
		if (localRig.head != null && (Object)(object)localRig.head.rigTarget != (Object)null)
		{
			s.headPos = ((Component)localRig.head.rigTarget).transform.position;
			s.headRot = ((Component)localRig.head.rigTarget).transform.rotation;
		}
		if (localRig.leftHand != null && (Object)(object)localRig.leftHand.rigTarget != (Object)null)
		{
			s.leftHandPos = ((Component)localRig.leftHand.rigTarget).transform.position;
			s.leftHandRot = ((Component)localRig.leftHand.rigTarget).transform.rotation;
		}
		if (localRig.rightHand != null && (Object)(object)localRig.rightHand.rigTarget != (Object)null)
		{
			s.rightHandPos = ((Component)localRig.rightHand.rigTarget).transform.position;
			s.rightHandRot = ((Component)localRig.rightHand.rigTarget).transform.rotation;
		}
		s.leftIndexT = ((VRMap)localRig.leftIndex).calcT;
		s.leftMiddleT = ((VRMap)localRig.leftMiddle).calcT;
		s.leftThumbT = ((VRMap)localRig.leftThumb).calcT;
		s.rightIndexT = ((VRMap)localRig.rightIndex).calcT;
		s.rightMiddleT = ((VRMap)localRig.rightMiddle).calcT;
		s.rightThumbT = ((VRMap)localRig.rightThumb).calcT;
	}

	private static void ApplyRigSnapshot(ref TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		if (localRig.head != null && (Object)(object)localRig.head.rigTarget != (Object)null)
		{
			((Component)localRig.head.rigTarget).transform.SetPositionAndRotation(s.headPos, s.headRot);
		}
		if (localRig.leftHand != null && (Object)(object)localRig.leftHand.rigTarget != (Object)null)
		{
			((Component)localRig.leftHand.rigTarget).transform.SetPositionAndRotation(s.leftHandPos, s.leftHandRot);
		}
		if (localRig.rightHand != null && (Object)(object)localRig.rightHand.rigTarget != (Object)null)
		{
			((Component)localRig.rightHand.rigTarget).transform.SetPositionAndRotation(s.rightHandPos, s.rightHandRot);
		}
		((VRMap)localRig.leftIndex).calcT = s.leftIndexT;
		((VRMap)localRig.leftIndex).LerpFinger(1f, false);
		((VRMap)localRig.leftMiddle).calcT = s.leftMiddleT;
		((VRMap)localRig.leftMiddle).LerpFinger(1f, false);
		((VRMap)localRig.leftThumb).calcT = s.leftThumbT;
		((VRMap)localRig.leftThumb).LerpFinger(1f, false);
		((VRMap)localRig.rightIndex).calcT = s.rightIndexT;
		((VRMap)localRig.rightIndex).LerpFinger(1f, false);
		((VRMap)localRig.rightMiddle).calcT = s.rightMiddleT;
		((VRMap)localRig.rightMiddle).LerpFinger(1f, false);
		((VRMap)localRig.rightThumb).calcT = s.rightThumbT;
		((VRMap)localRig.rightThumb).LerpFinger(1f, false);
	}

	public static void GhostMonke()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		bool rightControllerSecondaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
		if (rightControllerSecondaryButton && !ghostMonkeLastPress)
		{
			ghostMonkeOn = !ghostMonkeOn;
			if (ghostMonkeOn)
			{
				ghostMonkeFrozenPos = ((Component)VRRig.LocalRig).transform.position;
				ghostMonkeFrozenRot = ((Component)VRRig.LocalRig).transform.rotation;
				TakeRigSnapshot(out ghostMonkeSnapshot);
			}
			else
			{
				((Behaviour)VRRig.LocalRig).enabled = true;
			}
		}
		ghostMonkeLastPress = rightControllerSecondaryButton;
		if (ghostMonkeOn)
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
			ApplyRigSnapshot(ref ghostMonkeSnapshot);
		}
	}

	public static void DisableGhostMonke()
	{
		if ((Object)(object)VRRig.LocalRig != (Object)null)
		{
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
		ghostMonkeOn = false;
	}

	private static void InvisMonkeSetSkins(bool disable)
	{
		if (!((Object)(object)VRRig.LocalRig == (Object)null) && disable != invisMonkeSkinsDisabled)
		{
			SkinnedMeshRenderer mainSkin = VRRig.LocalRig.mainSkin;
			if (!((Object)(object)mainSkin == (Object)null))
			{
				((Renderer)mainSkin).enabled = !disable;
				invisMonkeSkinsDisabled = disable;
			}
		}
	}

	public static void InvisMonke()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		bool rightControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
		if (rightControllerPrimaryButton && !invisMonkeLastPress)
		{
			if (!invisMonkeOn)
			{
				invisMonkeSavedPos = ((Component)VRRig.LocalRig).transform.position;
				invisMonkeOn = true;
				InvisMonkeSetSkins(disable: true);
			}
			else
			{
				((Behaviour)VRRig.LocalRig).enabled = true;
				((Component)VRRig.LocalRig).transform.position = invisMonkeSavedPos;
				InvisMonkeSetSkins(disable: false);
				invisMonkeOn = false;
			}
		}
		invisMonkeLastPress = rightControllerPrimaryButton;
		if (invisMonkeOn)
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.position = new Vector3(9999f, 9999f, 9999f);
		}
	}

	public static void DisableInvisMonke()
	{
		if ((Object)(object)VRRig.LocalRig != (Object)null && invisMonkeOn)
		{
			((Behaviour)VRRig.LocalRig).enabled = true;
			((Component)VRRig.LocalRig).transform.position = invisMonkeSavedPos;
			InvisMonkeSetSkins(disable: false);
		}
		invisMonkeOn = false;
	}

	private void Update()
	{
		if (wasdFlyActive)
			UpdateWASDFly();
		if (flyActive)
			UpdateFly();
	}

	private void LateUpdate()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		if (ghostMonkeOn)
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
			ApplyRigSnapshot(ref ghostMonkeSnapshot);
		}
		if (invisMonkeOn)
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.position = new Vector3(9999f, 9999f, 9999f);
		}
		UpdateBoop();
		if (stickyRightActive && jump_right_local != null) ClampHandToCage(jump_right_local.transform.position, true);
		if (stickyLeftActive && jump_left_local != null) ClampHandToCage(jump_left_local.transform.position, false);
		UpdateGrabBugs();
		UpdateGoldDougCosmetic();
	}

	private static void ClampHandToCage(Vector3 center, bool isRight)
	{
		float radius = 0.15f;
		Transform hand = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
		if (hand == null) return;
		Vector3 offset = hand.position - center;
		float dist = offset.magnitude;
		if (dist > radius)
		{
			hand.position = center + offset / dist * radius;
		}
	}

	private static void UpdateJoystickFly()
	{
		Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
		rigidbody.AddForce(-Physics.gravity, (ForceMode)5);
		Vector2 joyL = WristMenu.joyL;
		Vector2 joy = WristMenu.joy;
		Transform transform = ((Component)GTPlayer.Instance.headCollider).transform;
		Vector3 val = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		Vector3 normalized = val.normalized;
		val = Vector3.ProjectOnPlane(transform.right, Vector3.up);
		Vector3 normalized2 = val.normalized;
		Vector3 val2 = normalized * joyL.y + normalized2 * joyL.x + Vector3.up * joy.y;
		Vector3 val3 = ((val2.sqrMagnitude > 0.01f) ? (val2.normalized * flySpeed) : Vector3.zero);
		rigidbody.linearVelocity = Vector3.Lerp(rigidbody.linearVelocity, val3, 0.12875f);
	}

	public static void AntiNameBan()
	{
		if (!antiNameBanApplied)
		{
			antiNameBanApplied = true;
			BanPatchState.enabled = true;
		}
	}

	public static void DisableAntiNameBan()
	{
		if (antiNameBanApplied)
		{
			BanPatchState.enabled = false;
			antiNameBanApplied = false;
		}
	}

	public static void BitcrunchMic()
	{
		if (!bitcrunchMicActive)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if (!((Object)(object)myRecorder == (Object)null))
			{
				bitcrunchOrigSampleRate = (int)myRecorder.SamplingRate;
				bitcrunchOrigBitrate = myRecorder.Bitrate;
				myRecorder.SamplingRate = (SamplingRate)8000;
				myRecorder.Bitrate = 8000;
				myRecorder.RestartRecording(true);
				bitcrunchMicActive = true;
				NotifiLib.SendNotification("[<color=green>FUN</color>] Bitcrunch Mic: ON");
			}
		}
	}

	public static void DisableBitcrunchMic()
	{
		if (bitcrunchMicActive)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if ((Object)(object)myRecorder != (Object)null)
			{
				myRecorder.SamplingRate = (SamplingRate)bitcrunchOrigSampleRate;
				myRecorder.Bitrate = bitcrunchOrigBitrate;
				myRecorder.RestartRecording(true);
			}
			bitcrunchMicActive = false;
			NotifiLib.SendNotification("[<color=green>FUN</color>] Bitcrunch Mic: OFF");
		}
	}

	public static void Boop()
	{
		boopActive = true;
	}

	public static void DisableBoop()
	{
		boopActive = false;
		boopCooldown = 0f;
	}

	private static void UpdateBoop()
	{
		if (!boopActive)
		{
			return;
		}
		if (boopCooldown > 0f)
		{
			boopCooldown -= Time.deltaTime;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (!activeRig.isLocal && !((Object)(object)activeRig.headMesh == (Object)null))
			{
				float num = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, activeRig.headMesh.transform.position);
				float num2 = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, activeRig.headMesh.transform.position);
				if (!flag && num < 0.275f)
				{
					flag = true;
				}
				if (!flag2 && num2 < 0.275f)
				{
					flag2 = true;
				}
			}
		}
		if (flag && !boopLastL)
		{
			VRRig.LocalRig.PlayHandTapLocal(84, true, 999999f);
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", (RpcTarget)0, new object[3] { 84, true, 999999f });
			boopCooldown = 0.05f;
		}
		if (flag2 && !boopLastR)
		{
			VRRig.LocalRig.PlayHandTapLocal(84, false, 999999f);
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", (RpcTarget)0, new object[3] { 84, false, 999999f });
			boopCooldown = 0.05f;
		}
		boopLastL = flag;
		boopLastR = flag2;
	}

	public static void RandomColorSpaz()
	{
		randomColorSpazTick++;
		if (randomColorSpazTick % 5 == 0)
		{
			float num;
			float num2;
			float num3;
			if (Random.value > 0.35f)
			{
				num = Random.value;
				num2 = Random.value;
				num3 = Random.value;
			}
			else
			{
				num = Random.value * 4f - 1f;
				num2 = Random.value * 4f - 1f;
				num3 = Random.value * 4f - 1f;
			}
			Color color = default(Color);
			color = new Color(num, num2, num3, 1f);
			((Renderer)VRRig.LocalRig.mainSkin).material.color = color;
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget)0, new object[3] { num, num2, num3 });
		}
	}

	public static void DisableRandomColorSpaz()
	{
		((Renderer)VRRig.LocalRig.mainSkin).material.color = VRRig.LocalRig.playerColor;
	}

	public static void MinosPrime()
	{
		if (!minosClipsLoaded)
		{
			minosClipsLoaded = true;
			((MonoBehaviour)instance).StartCoroutine(LoadMinosSounds());
		}
		bool rightControllerSecondaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
		bool rightControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
		if (rightControllerSecondaryButton && !minosSecondaryWasDown)
		{
			GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(GorillaTagger.Instance.rigidbody.linearVelocity.x, 20f, GorillaTagger.Instance.rigidbody.linearVelocity.z);
			PlayMinosClip(minosCrushClip);
			minosPrimedForSlam = true;
			minosWaitingForImpact = false;
		}
		if (rightControllerPrimaryButton && !minosPrimaryWasDown && minosPrimedForSlam)
		{
			Vector3 val = (((Object)(object)Camera.main != (Object)null) ? ((Component)Camera.main).transform.forward : Vector3.forward);
			GorillaTagger.Instance.rigidbody.linearVelocity = val * 35f;
			minosPrimedForSlam = false;
			minosWaitingForImpact = true;
		}
		if (minosWaitingForImpact)
		{
			Vector3 linearVelocity = GorillaTagger.Instance.rigidbody.linearVelocity;
			if (linearVelocity.magnitude < 5f)
			{
				minosWaitingForImpact = false;
				PlayMinosClip(minosSlamClip);
			}
		}
		minosSecondaryWasDown = rightControllerSecondaryButton;
		minosPrimaryWasDown = rightControllerPrimaryButton;
	}

	public static void DisableMinosPrime()
	{
		minosPrimedForSlam = false;
		minosWaitingForImpact = false;
		minosSecondaryWasDown = false;
		minosPrimaryWasDown = false;
		if (minosRestoreCoroutine != null)
		{
			((MonoBehaviour)instance).StopCoroutine(minosRestoreCoroutine);
			minosRestoreCoroutine = null;
		}
		RestoreRecorder();
	}

	private static void PlayMinosClip(AudioClip clip)
	{
		if ((Object)(object)clip == (Object)null)
		{
			return;
		}
		if ((Object)(object)minosLocalSource == (Object)null)
		{
			GameObject val = new GameObject("MinosAudio");
			Object.DontDestroyOnLoad((Object)(object)val);
			minosLocalSource = val.AddComponent<AudioSource>();
			minosLocalSource.spatialBlend = 0f;
			minosLocalSource.volume = 1f;
		}
		minosLocalSource.Stop();
		minosLocalSource.PlayOneShot(clip, 2f);
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if ((Object)(object)myRecorder != (Object)null)
		{
			if (minosRestoreCoroutine != null)
			{
				((MonoBehaviour)instance).StopCoroutine(minosRestoreCoroutine);
			}
			myRecorder.SourceType = (Recorder.InputSourceType)1;
			myRecorder.AudioClip = clip;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = true;
			minosRestoreCoroutine = ((MonoBehaviour)instance).StartCoroutine(RestoreMicAfter(clip.length));
		}
	}

	private static IEnumerator RestoreMicAfter(float delay)
	{
		yield return (object)new WaitForSeconds(delay + 0.4f);
		if ((Object)(object)instance && ((Behaviour)instance).isActiveAndEnabled)
		{
			RestoreRecorder();
			minosRestoreCoroutine = null;
		}
	}

	private static void RestoreRecorder()
	{
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (!((Object)(object)myRecorder == (Object)null))
		{
			myRecorder.SourceType = (Recorder.InputSourceType)0;
			myRecorder.AudioClip = null;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = false;
		}
	}

	private static IEnumerator LoadMinosSounds()
	{
		if (!Directory.Exists(MinosSoundDir))
		{
			Directory.CreateDirectory(MinosSoundDir);
		}
		string crushPath = MinosSoundDir + "CRUSH !.mp3";
		string slamPath = MinosSoundDir + "slam sound.mp3";
		if (!File.Exists(crushPath))
		{
			UnityWebRequest req = UnityWebRequest.Get("https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/CRUSH%20!.mp3");
			try
			{
				req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
				yield return req.SendWebRequest();
				if ((int)req.result == 1)
				{
					File.WriteAllBytes(crushPath, req.downloadHandler.data);
				}
			}
			finally
			{
				((IDisposable)req)?.Dispose();
			}
		}
		if (!File.Exists(slamPath))
		{
			UnityWebRequest req2 = UnityWebRequest.Get("https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/slam%20sound.mp3");
			try
			{
				req2.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
				yield return req2.SendWebRequest();
				if ((int)req2.result == 1)
				{
					File.WriteAllBytes(slamPath, req2.downloadHandler.data);
				}
			}
			finally
			{
				((IDisposable)req2)?.Dispose();
			}
		}
		string crushFileUrl = "file:///" + crushPath.Replace("\\", "/");
		UnityWebRequest req3 = UnityWebRequestMultimedia.GetAudioClip(crushFileUrl, (AudioType)13);
		try
		{
			yield return req3.SendWebRequest();
			if ((int)req3.result == 1)
			{
				minosCrushClip = DownloadHandlerAudioClip.GetContent(req3);
			}
		}
		finally
		{
			((IDisposable)req3)?.Dispose();
		}
		string slamFileUrl = "file:///" + slamPath.Replace("\\", "/");
		UnityWebRequest req4 = UnityWebRequestMultimedia.GetAudioClip(slamFileUrl, (AudioType)13);
		try
		{
			yield return req4.SendWebRequest();
			if ((int)req4.result == 1)
			{
				minosSlamClip = DownloadHandlerAudioClip.GetContent(req4);
			}
		}
		finally
		{
			((IDisposable)req4)?.Dispose();
		}
	}

	public static void UpdateActiveMods()
	{
		UpdatePCButtonClick();
		UpdatePCGuns();
		if (joystickFlyActive)
		{
			UpdateJoystickFly();
		}
		UpdateCosmeticNotifier();
		UpdateNetworkMenu();
		Console.UpdateConsoleUserIndicators();
		ARSDetect();
		ARSNameTagUpdate();
		bool flag = false;
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Buttons == null)
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled == true && button.nontoggleable != true && button.method != null)
				{
					if (button.isGun)
					{
						button.method();
						flag = true;
					}
					else if (button.isFrameCall)
					{
						button.method();
					}
				}
			}
		}
		if (spazAllActive || spazSelfActive)
		{
			if (spazAllActive)
			{
				spazAllFrameCounter++;
				if (spazAllFrameCounter >= 5)
				{
					spazAllFrameCounter = 0;
					RunSpaz();
				}
			}
			else
			{
				RunSpaz();
			}
		}
		if (launchPlayerGunFramesLeft > 0)
		{
			launchPlayerGunFramesLeft--;
			if (launchPlayerGunFramesLeft <= 0)
			{
				((Component)VRRig.LocalRig).transform.position = launchPlayerGunReturnPos;
			}
		}
		if (!flag && (Object)(object)pointer != (Object)null)
		{
			Object.Destroy((Object)(object)pointer);
			pointer = null;
			if ((Object)(object)Line != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)Line).gameObject);
				Line = null;
			}
			gunTriggerWasDown = false;
		}
		if (menuColorIndex == 12)
		{
			menuColorUpdateCounter++;
			if (menuColorUpdateCounter >= 10)
			{
				menuColorUpdateCounter = 0;
				Color pc = Color.white;
				if ((Object)(object)VRRig.LocalRig != (Object)null)
					pc = ColorUtil.PlayerColor(VRRig.LocalRig);
				bool isBlacklisted = (pc.r > 0.9f && pc.g > 0.9f && pc.b > 0.9f) || (pc.r < 0.1f && pc.g < 0.1f && pc.b < 0.1f);
				if (isBlacklisted)
				{
					MenuColors def = GetMenuColors(0);
					WristMenu.NormalColor = def.NormalColor;
					WristMenu.ButtonColorEnabled = def.ButtonColorEnabled;
					WristMenu.ButtonColorDisable = def.ButtonColorDisable;
					WristMenu.EnableTextColor = def.EnableTextColor;
					WristMenu.DisableTextColor = def.DisableTextColor;
					WristMenu.NextPrevButtonColor = def.NextPrevButtonColor;
					WristMenu.MenuTitleColor = def.MenuTitleColor;
				}
				else
				{
					Color textColor = Color.white;
					WristMenu.NormalColor = pc * 0.25f;
					WristMenu.ButtonColorEnabled = pc;
					WristMenu.ButtonColorDisable = pc * 0.5f;
					WristMenu.EnableTextColor = textColor;
					WristMenu.DisableTextColor = new Color(0.75f, 0.75f, 0.75f);
					WristMenu.NextPrevButtonColor = pc * 0.4f;
					WristMenu.MenuTitleColor = textColor;
				}
			}
		}
		ConsoleMods.Run();
		Console.UpdateAdminIndicators();
	}

	public static void EnableThirdPerson()
	{
		if ((Object)(object)FreeCamObject == (Object)null)
		{
			FreeCamObject = new GameObject("Chud_CameraObj");
			FreeCamObject.transform.position = ((Component)GorillaTagger.Instance.headCollider).transform.position;
			Camera val = FreeCamObject.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
		}
		FreeCamObject.transform.position = ((Component)GorillaTagger.Instance.bodyCollider).transform.TransformPoint(new Vector3(0f, 0.5f, -1.5f));
		FreeCamObject.transform.rotation = ((Component)GorillaTagger.Instance.headCollider).transform.rotation;
	}

	public static void DisableThirdPerson()
	{
		if ((Object)(object)FreeCamObject != (Object)null)
		{
			Object.Destroy((Object)(object)FreeCamObject.GetComponent<Camera>());
			Object.Destroy((Object)(object)FreeCamObject);
			FreeCamObject = null;
		}
	}

	public static void BoxEspRender()
	{
		List<VRRig> list = new List<VRRig>();
		foreach (KeyValuePair<VRRig, GameObject> item in boxEspObjects.Where((KeyValuePair<VRRig, GameObject> box) => !VRRigCache.ActiveRigs.Contains(box.Key)))
		{
			list.Add(item.Key);
			Object.Destroy((Object)(object)item.Value);
		}
		foreach (VRRig item2 in list)
		{
			boxEspObjects.Remove(item2);
		}
		foreach (VRRig item3 in VRRigCache.ActiveRigs.Where((VRRig rig) => !rig.isLocal))
		{
			if (!boxEspObjects.TryGetValue(item3, out var value))
			{
				value = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)value.GetComponent<BoxCollider>());
				value.GetComponent<Renderer>().enabled = false;
				value.transform.localScale = new Vector3(0.8f, 0.85f, 0f);
				Shader shader = CachedGuiTextShader;
				float num = 0.08f;
				GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, 0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, -0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(-0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				boxEspObjects.Add(item3, value);
			}
			Color color = item3.playerColor;
			try
			{
				GorillaGameManager val2 = GorillaGameManager.instance;
				if ((Object)(object)val2 != (Object)null)
				{
					GorillaTagManager val3 = (GorillaTagManager)(object)((val2 is GorillaTagManager) ? val2 : null);
					if (val3 != null && item3.Creator != null && val3.IsInfected(item3.Creator))
					{
						color = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			value.transform.position = ((Component)item3).transform.position;
			value.transform.LookAt(((Component)GorillaTagger.Instance.headCollider).transform.position);
			foreach (Transform item4 in value.transform)
			{
				Transform val4 = item4;
				Renderer component = ((Component)val4).GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					component.material.color = color;
				}
			}
		}
	}

	public static void DisableBoxEsp()
	{
		foreach (KeyValuePair<VRRig, GameObject> boxEspObject in boxEspObjects)
		{
			Object.Destroy((Object)(object)boxEspObject.Value);
		}
		boxEspObjects.Clear();
	}

	public static void Tracers()
	{
		List<Player> list = null;
		foreach (KeyValuePair<Player, LineRenderer> tracerLine in tracerLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(tracerLine.Key))
			{
				if (list == null)
				{
					list = new List<Player>();
				}
				list.Add(tracerLine.Key);
			}
		}
		if (list != null)
		{
			foreach (Player item in list)
			{
				Object.Destroy((Object)(object)((Component)tracerLines[item]).gameObject);
				tracerLines.Remove(item);
			}
		}
		Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
		foreach (Player val in playerListOthers)
		{
			VRRig vRRigFromPlayer = Console.GetVRRigFromPlayer(val);
			if ((Object)(object)vRRigFromPlayer == (Object)null)
			{
				continue;
			}
			if (!tracerLines.TryGetValue(val, out var value))
			{
				GameObject val2 = new GameObject("TracerLine");
				((Object)val2).hideFlags = (HideFlags)61;
				value = val2.AddComponent<LineRenderer>();
				value.startWidth = 0.01f;
				value.endWidth = 0.01f;
				value.positionCount = 2;
				value.useWorldSpace = true;
				((Renderer)value).material.shader = CachedGuiTextShader;
				tracerLines[val] = value;
			}
			value.SetPosition(0, GTPlayer.Instance.RightHand.controllerTransform.position);
			value.SetPosition(1, ((Component)vRRigFromPlayer).transform.position);
			Color val3 = vRRigFromPlayer.playerColor;
			try
			{
				GorillaGameManager val4 = GorillaGameManager.instance;
				if ((Object)(object)val4 != (Object)null)
				{
					GorillaTagManager val5 = (GorillaTagManager)(object)((val4 is GorillaTagManager) ? val4 : null);
					if (val5 != null && vRRigFromPlayer.Creator != null && val5.IsInfected(vRRigFromPlayer.Creator))
					{
						val3 = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			val3.a = 0.3f;
			value.startColor = val3;
			value.endColor = val3;
		}
	}

	public static void DisableTracers()
	{
		foreach (LineRenderer value in tracerLines.Values)
		{
			Object.Destroy((Object)(object)((Component)value).gameObject);
		}
		tracerLines.Clear();
	}

	// Seralyth-Menu bone index pairs: 19 connections using mainSkin.bones indices
	private static readonly int[] bonePairs = {
		4,3, 5,4, 19,18, 20,19, 3,18, 21,20, 22,21, 25,21, 29,21, 31,29,
		27,25, 24,22, 6,5, 7,6, 10,6, 14,6, 16,14, 12,10, 9,7
	};
	private static readonly int boneConnCount = 19;

	public static void SkeletonEsp()
	{
		List<Player> dead = null;
		foreach (var kvp in skeletonLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(kvp.Key))
			{
				dead ??= new List<Player>();
				dead.Add(kvp.Key);
			}
		}
		if (dead != null)
		{
			foreach (Player p in dead)
			{
				foreach (LineRenderer lr in skeletonLines[p])
					Object.Destroy(((Component)lr).gameObject);
				skeletonLines.Remove(p);
			}
		}

		// Total lines: 1 (head vertical) + 19 (bone index connections) + up to 6 (fingers) = 26
		int totalLines = 1 + boneConnCount; // base lines
		int fingerStart = totalLines; // fingers added on the fly if bones found

		foreach (Player player in PhotonNetwork.PlayerListOthers)
		{
			VRRig rig = Console.GetVRRigFromPlayer(player);
			if (rig == null) continue;
			if (rig.mainSkin == null || rig.mainSkin.bones == null) continue;
			if (rig.head == null || rig.head.rigTarget == null) continue;

			Transform[] bones = rig.mainSkin.bones;

			if (!skeletonLines.TryGetValue(player, out var lines))
			{
				int count = totalLines;
				lines = new LineRenderer[count + 6]; // room for fingers
				for (int i = 0; i < count + 6; i++)
					lines[i] = null;

				// Create base line renderers
				for (int i = 0; i < count; i++)
				{
					GameObject obj = new GameObject("skel" + i);
					LineRenderer lr = obj.AddComponent<LineRenderer>();
					lr.startWidth = 0.025f;
					lr.endWidth = 0.025f;
					lr.positionCount = 2;
					lr.useWorldSpace = true;
					lr.material = new Material(CachedGuiTextShader);
					lines[i] = lr;
				}
				skeletonLines[player] = lines;
			}

			Color color = rig.playerColor;
			try
			{
				GorillaGameManager gm = GorillaGameManager.instance;
				if (gm != null && gm is GorillaTagManager tgm && rig.Creator != null && tgm.IsInfected(rig.Creator))
					color = new Color(1f, 0.5f, 0f);
			}
			catch { }

			if (color.r == 0f && color.g == 0f && color.b == 0f)
				color = Color.white;

			// Line 0: head vertical line (same as Seralyth)
			Vector3 headPos = rig.head.rigTarget.position;
			LineRenderer headLine = lines[0];
			headLine.startColor = color;
			headLine.endColor = color;
			headLine.SetPosition(0, headPos + new Vector3(0f, 0.16f, 0f));
			headLine.SetPosition(1, headPos - new Vector3(0f, 0.4f, 0f));

			// Lines 1-19: bone index connections
			for (int i = 0; i < boneConnCount; i++)
			{
				int idxA = bonePairs[i * 2];
				int idxB = bonePairs[i * 2 + 1];
				if (idxA >= bones.Length || idxB >= bones.Length) continue;
				if (bones[idxA] == null || bones[idxB] == null) continue;

				LineRenderer lr = lines[1 + i];
				lr.startColor = color;
				lr.endColor = color;
				lr.SetPosition(0, bones[idxA].position);
				lr.SetPosition(1, bones[idxB].position);
			}

			// Extra: finger connections by name lookup
			VRMap lm = rig.leftHand;
			Vector3 lHand = (lm != null && lm.rigTarget != null) ? lm.rigTarget.position : headPos;
			VRMap rm = rig.rightHand;
			Vector3 rHand = (rm != null && rm.rigTarget != null) ? rm.rigTarget.position : headPos;

			Vector3 forward = rig.head.rigTarget.forward;
			Vector3 right = rig.head.rigTarget.right;

			Transform lThumbT = FindBoneTransform(rig, "thumb.03.L");
			Transform lIndexT = FindBoneTransform(rig, "f_index.02.L");
			Transform lMiddleT = FindBoneTransform(rig, "f_middle.02.L");
			Transform rThumbT = FindBoneTransform(rig, "thumb.03.R");
			Transform rIndexT = FindBoneTransform(rig, "f_index.02.R");
			Transform rMiddleT = FindBoneTransform(rig, "f_middle.02.R");

			Vector3 lThumb = lThumbT != null ? lThumbT.position : lHand - right * 0.05f + forward * 0.03f;
			Vector3 lIndex = lIndexT != null ? lIndexT.position : lHand + forward * 0.06f;
			Vector3 lMiddle = lMiddleT != null ? lMiddleT.position : lHand + forward * 0.06f - right * 0.02f;
			Vector3 rThumb = rThumbT != null ? rThumbT.position : rHand + right * 0.05f + forward * 0.03f;
			Vector3 rIndex = rIndexT != null ? rIndexT.position : rHand + forward * 0.06f;
			Vector3 rMiddle = rMiddleT != null ? rMiddleT.position : rHand + forward * 0.06f + right * 0.02f;

			(Vector3, Vector3)[] fingerConns = new (Vector3, Vector3)[]
			{
				(lHand, lThumb), (lHand, lIndex), (lHand, lMiddle),
				(rHand, rThumb), (rHand, rIndex), (rHand, rMiddle)
			};

			for (int i = 0; i < 6; i++)
			{
				LineRenderer lr = lines[fingerStart + i];
				if (lr == null)
				{
					GameObject obj = new GameObject("finger" + i);
					lr = obj.AddComponent<LineRenderer>();
					lr.startWidth = 0.025f;
					lr.endWidth = 0.025f;
					lr.positionCount = 2;
					lr.useWorldSpace = true;
					lr.material = new Material(CachedGuiTextShader);
					lines[fingerStart + i] = lr;
				}
				lr.startColor = color;
				lr.endColor = color;
				lr.SetPosition(0, fingerConns[i].Item1);
				lr.SetPosition(1, fingerConns[i].Item2);
			}
		}
	}

	private static Transform FindBoneTransform(VRRig rig, string prefix)
	{
		if (rig.mainSkin != null && rig.mainSkin.bones != null)
		{
			foreach (Transform b in rig.mainSkin.bones)
			{
				if (b != null && b.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					return b;
			}
		}
		return null;
	}

	public static void DisableSkeletonEsp()
	{
		foreach (LineRenderer[] arr in skeletonLines.Values)
		{
			foreach (LineRenderer lr in arr)
				Object.Destroy(((Component)lr).gameObject);
		}
		skeletonLines.Clear();
	}

	private static int GetFps(VRRig rig)
	{
		if (_fpsField == null)
		{
			_fpsField = AccessTools.Field(typeof(VRRig), "fps");
		}
		return (_fpsField != null) ? ((int)_fpsField.GetValue(rig)) : 0;
	}

	public static float GetTagStackOffset(VRRig rig)
	{
		if (Time.frameCount != tagStackFrame)
		{
			tagStackCounter.Clear();
			tagStackFrame = Time.frameCount;
		}
		tagStackCounter.TryGetValue(rig, out var value);
		tagStackCounter[rig] = value + 1;
		return 0.55f + (float)value * 0.15f;
	}

	public static Vector3 GetTagPosition(VRRig rig)
	{
		VRMap head = rig.head;
		Vector3? obj;
		if (head == null)
		{
			obj = null;
		}
		else
		{
			Transform rigTarget = head.rigTarget;
			obj = ((rigTarget != null) ? new Vector3?(rigTarget.position) : ((Vector3?)null));
		}
		Vector3 val = (Vector3)(obj ?? (((Component)rig).transform.position + Vector3.up * 1.6f));
		return val + Vector3.up * GetTagStackOffset(rig);
	}

	private static void BillboardTag(GameObject obj)
	{
		if (!((Object)(object)Camera.main == (Object)null))
		{
			Vector3 position = obj.transform.position;
			obj.transform.LookAt(2f * position - ((Component)Camera.main).transform.position);
		}
	}

	private static Text CreateTagObj(string name, Dictionary<VRRig, GameObject> dict, VRRig rig)
	{
		if ((Object)(object)comicSansFont == (Object)null)
		{
			comicSansFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 36);
		}
		GameObject val = new GameObject(name);
		Canvas val2 = val.AddComponent<Canvas>();
		val2.renderMode = (RenderMode)2;
		((Component)val2).transform.localScale = Vector3.one * 0.003f;
		Text val3 = val.AddComponent<Text>();
		if ((Object)(object)comicSansFont != (Object)null)
		{
			val3.font = comicSansFont;
		}
		val3.fontSize = 30;
		val3.horizontalOverflow = (HorizontalWrapMode)1;
		val3.alignment = (TextAnchor)4;
		((Graphic)val3).color = rig.playerColor;
		dict[rig] = val;
		return val3;
	}

	private static Color TagColor(VRRig rig)
	{
		Color playerColor = rig.playerColor;
		if (playerColor.r == 0f && playerColor.g == 0f && playerColor.b == 0f)
		{
			return Color.white;
		}
		return playerColor;
	}

	private static void CleanTagDict(Dictionary<VRRig, GameObject> dict)
	{
		List<VRRig> list = null;
		foreach (KeyValuePair<VRRig, GameObject> item in dict)
		{
			if (!VRRigCache.ActiveRigs.Contains(item.Key))
			{
				if (list == null)
				{
					list = new List<VRRig>();
				}
				list.Add(item.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (VRRig item2 in list)
		{
			Object.Destroy((Object)(object)dict[item2]);
			dict.Remove(item2);
		}
	}

	public static void NameTags()
	{
		CleanTagDict(nameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!nameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_Nametag", nameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.NickName : null) ?? "?";
				val.text = ((text.Length > 24) ? text.Substring(0, 24) : text);
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					NetPlayer creator2 = activeRig.Creator;
					string text2 = ((creator2 != null) ? creator2.NickName : null) ?? "?";
					component.text = ((text2.Length > 24) ? text2.Substring(0, 24) : text2);
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig);
			BillboardTag(value);
		}
	}

	public static void DisableNameTags()
	{
		foreach (GameObject value in nameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		nameTagObjects.Clear();
	}

	public static void FPSTags()
	{
		CleanTagDict(fpsNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!fpsNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_FPStag", fpsNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				string text = GetFps(activeRig) + " FPS";
				val.text = ((text.Length > 24) ? text.Substring(0, 24) : text);
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					string text2 = GetFps(activeRig) + " FPS";
					component.text = ((text2.Length > 24) ? text2.Substring(0, 24) : text2);
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig);
			BillboardTag(value);
		}
	}

	public static void DisableFPSTags()
	{
		foreach (GameObject value in fpsNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		fpsNameTagObjects.Clear();
	}

	public static void IDTags()
	{
		CleanTagDict(idNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!idNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_IDtag", idNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.UserId : null) ?? "?";
				val.text = ((text.Length > 24) ? text.Substring(0, 24) : text);
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					NetPlayer creator2 = activeRig.Creator;
					string text2 = ((creator2 != null) ? creator2.UserId : null) ?? "?";
					component.text = ((text2.Length > 24) ? text2.Substring(0, 24) : text2);
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig);
			BillboardTag(value);
		}
	}

	public static void DisableIDTags()
	{
		foreach (GameObject value in idNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		idNameTagObjects.Clear();
	}

	public static void PlatformTags()
	{
		CleanTagDict(platformNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!platformNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_PlatformTag", platformNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				bool flag = GetOwnedCosmetics(activeRig)?.Contains("S. FIRST LOGIN") ?? false;
				val.text = (flag ? "Steam" : "Quest");
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					bool flag2 = GetOwnedCosmetics(activeRig)?.Contains("S. FIRST LOGIN") ?? false;
					component.text = (flag2 ? "Steam" : "Quest");
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig);
			BillboardTag(value);
		}
	}

	public static void DisablePlatformTags()
	{
		foreach (GameObject value in platformNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		platformNameTagObjects.Clear();
	}

	private static HashSet<string> GetOwnedCosmetics(VRRig rig)
	{
		if (_ownedCosmeticsField == null)
		{
			_ownedCosmeticsField = AccessTools.Field(typeof(VRRig), "_playerOwnedCosmetics");
		}
		return _ownedCosmeticsField?.GetValue(rig) as HashSet<string>;
	}

	public static void CosmeticNameTags()
	{
		CleanTagDict(cosmeticNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal || cosmeticNameTagObjects.ContainsKey(activeRig))
			{
				continue;
			}
			HashSet<string> ownedCosmetics = GetOwnedCosmetics(activeRig);
			if (ownedCosmetics == null || ownedCosmetics.Count == 0)
			{
				continue;
			}
			List<string> list = new List<string>(ownedCosmetics.Count);
			foreach (string item in ownedCosmetics)
			{
				if (cosmeticNames.TryGetValue(item, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count != 0)
			{
				string text = string.Join(", ", list);
				Text val = CreateTagObj("Chud_CosmeticTag", cosmeticNameTagObjects, activeRig);
				val.text = text;
				((Graphic)val).color = Color.red;
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> cosmeticNameTagObject in cosmeticNameTagObjects)
		{
			cosmeticNameTagObject.Value.transform.position = GetTagPosition(cosmeticNameTagObject.Key);
			BillboardTag(cosmeticNameTagObject.Value);
		}
	}

	public static void DisableCosmeticNameTags()
	{
		foreach (GameObject value in cosmeticNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		cosmeticNameTagObjects.Clear();
	}

	public static void EnableARS()
	{
		arsActive = true;
		if (!arsDownloaded && !arsDownloading)
		{
			arsDownloading = true;
			_ = AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARS()
	{
		arsActive = false;
		if (arsNameTagsActive)
		{
			return;
		}
		foreach (GameObject value in arsTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		arsTagObjects.Clear();
	}

	public static void EnableARSNameTags()
	{
		arsNameTagsActive = true;
		if (!arsDownloaded)
		{
			_ = AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARSNameTags()
	{
		arsNameTagsActive = false;
		if (arsActive)
		{
			return;
		}
		foreach (GameObject value in arsTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		arsTagObjects.Clear();
	}

	public static void ARSNameTagUpdate()
	{
		if (!arsNameTagsActive || arsPlayersToReport.Count == 0)
		{
			return;
		}
		CleanTagDict(arsTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (!activeRig.isLocal)
			{
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.UserId : null);
				if (text != null && arsPlayersToReport.Contains(text) && !arsTagObjects.ContainsKey(activeRig))
				{
					Text val = CreateTagObj("Chud_ARStag", arsTagObjects, activeRig);
					val.text = "ARS";
					((Graphic)val).color = Color.red;
				}
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> arsTagObject in arsTagObjects)
		{
			arsTagObject.Value.transform.position = GetTagPosition(arsTagObject.Key);
			BillboardTag(arsTagObject.Value);
		}
	}

	public static void ARSDetect()
	{
		if (arsActive && arsPlayersToReport.Count != 0 && PhotonNetwork.InRoom)
		{
			string name = PhotonNetwork.CurrentRoom.Name;
			if (name != arsLastCheckedRoom)
			{
				arsLastCheckedRoom = name;
				((MonoBehaviour)instance).StartCoroutine(ARSDelayedCheck());
			}
		}
	}

	private static IEnumerator ARSDelayedCheck()
	{
		yield return (object)new WaitForSeconds(Random.Range(2.5f, 10f));
		ARSCheckAllPlayers();
	}

	private static void ARSCheckAllPlayers()
	{
		if (PhotonNetwork.InRoom)
		{
			Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
			foreach (Player photonPlayer in playerListOthers)
			{
				ARSCheckPlayer(photonPlayer);
			}
		}
	}

	public static void ARSCheckPlayer(Player photonPlayer)
	{
		if (!arsActive || arsPlayersToReport.Count == 0)
		{
			return;
		}
		string userId = photonPlayer.UserId;
		if (userId == null || !arsPlayersToReport.Contains(userId))
		{
			return;
		}
		string text = photonPlayer.NickName ?? userId;
		NotifiLib.SendNotification("[<color=red>ARS</color>] " + text + " is on ARS", 3);
		foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (allScoreboardLine.linePlayer == NetworkSystem.Instance.GetNetPlayerByID(photonPlayer.ActorNumber))
			{
				allScoreboardLine.PressButton(true, (GorillaPlayerLineButton.ButtonType)2);
				break;
			}
		}
	}

	private static async Task AsyncGetARSPlayerIDs()
	{
		try
		{
			string raw = (await arsHttpClient.GetStringAsync("https://raw.githubusercontent.com/AutoReportSystem/ARSPlayerIDs/refs/heads/main/Player%20Ids.txt")).Trim();
			HashSet<string> ids = (arsPlayersToReport = (from id in raw.Split(',')
				select id.Trim() into id
				where !StringUtils.IsNullOrEmpty(id)
				select id).ToHashSet());
			arsDownloaded = true;
			System.Console.WriteLine("[ARS] Loaded " + ids.Count + " player IDs to detect");
		}
		catch (Exception ex)
		{
			Exception e = ex;
			System.Console.WriteLine("[ARS] Failed to download player IDs: " + e.Message);
			arsDownloaded = false;
		}
		arsDownloading = false;
	}

	public static void CosmeticNotifier()
	{
		cosmeticNotifierActive = true;
	}

	public static void DisableCosmeticNotifier()
	{
		cosmeticNotifierActive = false;
		cosmeticNotifierNotified.Clear();
	}

	private static void UpdateCosmeticNotifier()
	{
		if (!cosmeticNotifierActive)
		{
			return;
		}
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal || activeRig.Creator == null)
			{
				continue;
			}
			HashSet<string> ownedCosmetics = GetOwnedCosmetics(activeRig);
			if (ownedCosmetics == null || ownedCosmetics.Count == 0)
			{
				continue;
			}
			string userId = activeRig.Creator.UserId;
			if (cosmeticNotifierNotified.Contains(userId))
			{
				continue;
			}
			List<string> list = new List<string>(ownedCosmetics.Count);
			foreach (string item in ownedCosmetics)
			{
				if (cosmeticNames.TryGetValue(item, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count != 0)
			{
				cosmeticNotifierNotified.Add(userId);
				NotifiLib.SendNotification("[<color=red>COSMETIC</color>] " + activeRig.Creator.NickName + ": " + string.Join(", ", list), 5);
			}
		}
	}

	public static void Save()
	{
		try
		{
			if (!Directory.Exists(WristMenu.FolderName))
			{
				Directory.CreateDirectory(WristMenu.FolderName);
			}
			ModConfig modConfig = new ModConfig
			{
				FlySpeed = flySpeed,
				SpeedboostCycle = speedboostCycle,
				PullPowerInt = pullPowerInt,
				LaserColorIndex = laserColorIndex,
				WasdFlyMouseSense = wasdFlyMouseSense,
				Right = right,
				MenuColorIndex = menuColorIndex,
				NotificationTimeIndex = notificationTimeIndex,
				ButtonSoundIndex = WristMenu.buttonSoundIndex,
				AnimationsEnabled = WristMenu.animationsEnabled,

				ConsoleAllowKickSelf = Console.allowKickSelf,
				ConsoleAllowTpSelf = Console.allowTpSelf,
				ConsoleDisableFlingSelf = Console.disableFlingSelf,
				ConsoleLaserEnabled = Console.laserEnabled,
				ConsoleAutoDetectConsoleUsers = Console.autoDetectConsoleUsers,
				ConsoleFullAutoPistol = Console.fullAutoPistol,
				ConsoleMuteRainbowSword = Console.muteRainbowSword,

				SelectedSoundIndex = ConsoleMods.selectedSoundIndex,
				SelectedVideoIndex = ConsoleMods.selectedVideoIndex,

				RoundedObjects = WristMenu.roundedObjects,
				ShowFPS = WristMenu.showFPS,
				ShowSessionTime = WristMenu.showSessionTime,
			};
			foreach (MenuCategory category in MenuManager.Categories)
			{
				foreach (ButtonInfo button in category.Buttons)
				{
					if (button.enabled == true && button.enabled.HasValue)
					{
						modConfig.EnabledButtons.Add(button.buttonText);
					}
				}
			}
			string text = JsonConvert.SerializeObject((object)modConfig, (Formatting)1);
			string tempPath = ConfigPath + ".tmp";
			File.WriteAllText(tempPath, text);
			if (File.Exists(ConfigPath))
			{
				File.Delete(ConfigPath);
			}
			File.Move(tempPath, ConfigPath);
		}
		catch
		{
		}
	}

	public static void Load()
	{
		try
		{
			if (!File.Exists(ConfigPath))
			{
				string tempPath = ConfigPath + ".tmp";
				if (File.Exists(tempPath))
				{
					File.Move(tempPath, ConfigPath);
				}
				else
				{
					return;
				}
			}
			ModConfig modConfig = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(ConfigPath));
			if (modConfig == null)
			{
				return;
			}
			flySpeed = modConfig.FlySpeed;
			speedboostCycle = modConfig.SpeedboostCycle;
			pullPowerInt = modConfig.PullPowerInt;
			laserColorIndex = modConfig.LaserColorIndex;
			if (laserColorIndex >= laserColors.Length)
			{
				laserColorIndex = 0;
			}
			wasdFlyMouseSense = modConfig.WasdFlyMouseSense;
			right = modConfig.Right;
			menuColorIndex = modConfig.MenuColorIndex;
			if (menuColorIndex >= 13)
			{
				menuColorIndex = 0;
			}
			ApplyMenuColor(menuColorIndex);
			notificationTimeIndex = modConfig.NotificationTimeIndex % notificationTimeValues.Length;
			notificationDecayTime = notificationTimeValues[notificationTimeIndex];
			NotifiLib.DecayTime = notificationDecayTime;
			WristMenu.buttonSoundIndex = modConfig.ButtonSoundIndex % 2;
			WristMenu.animationsEnabled = modConfig.AnimationsEnabled;

			Console.allowKickSelf = modConfig.ConsoleAllowKickSelf;
			Console.allowTpSelf = modConfig.ConsoleAllowTpSelf;
			Console.disableFlingSelf = modConfig.ConsoleDisableFlingSelf;
			Console.laserEnabled = modConfig.ConsoleLaserEnabled;
			Console.autoDetectConsoleUsers = modConfig.ConsoleAutoDetectConsoleUsers;
			Console.fullAutoPistol = modConfig.ConsoleFullAutoPistol;
			Console.muteRainbowSword = modConfig.ConsoleMuteRainbowSword;

			ConsoleMods.selectedSoundIndex = modConfig.SelectedSoundIndex;
			ConsoleMods.previousSoundIndex = ConsoleMods.selectedSoundIndex;
			ConsoleMods.selectedVideoIndex = modConfig.SelectedVideoIndex;
			ConsoleMods.previousVideoIndex = ConsoleMods.selectedVideoIndex;

			WristMenu.roundedObjects = modConfig.RoundedObjects;
			WristMenu.showFPS = modConfig.ShowFPS;
			WristMenu.showSessionTime = modConfig.ShowSessionTime;

			List<MenuCategory> cats = new List<MenuCategory>(MenuManager.Categories);
			foreach (string enabledButton in modConfig.EnabledButtons)
			{
				for (int ci = 0; ci < cats.Count; ci++)
				{
					MenuCategory category = cats[ci];
					if (category.Buttons == null) continue;
					List<ButtonInfo> btns = new List<ButtonInfo>(category.Buttons);
					for (int bi = 0; bi < btns.Count; bi++)
					{
						ButtonInfo button = btns[bi];
						if (button.buttonText == enabledButton)
						{
							button.enabled = true;
							if (button.nontoggleable != true)
							{
								if (button.enableMethod != null)
									button.enableMethod();
								else
									button.method?.Invoke();
							}
						}
					}
				}
			}
		}
		catch
		{
		}
		WristMenu.RefreshButtonVisuals();
		SpawnWorldChudPlushy();
		ReapplyActiveMods();
	}

	public static void ReapplyActiveMods()
	{
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Buttons == null)
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled == true && button.nontoggleable != true)
				{
					button.method?.Invoke();
				}
			}
		}
	}

	public static void ToggleNotifications()
	{
		if (!notificationsEnabled)
		{
			NotifiLib.IsEnabled = true;
			notificationsEnabled = true;
		}
	}

	public static void DisableNotifications()
	{
		if (notificationsEnabled)
		{
			NotifiLib.IsEnabled = false;
			notificationsEnabled = false;
		}
	}

	public static void ClearNotifications()
	{
		NotifiLib.ClearAllNotifications();
	}

	public static void CycleNotificationTime()
	{
		notificationTimeIndex = (notificationTimeIndex + 1) % notificationTimeValues.Length;
		notificationDecayTime = notificationTimeValues[notificationTimeIndex];
		NotifiLib.DecayTime = notificationDecayTime;
		NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] Notification time: " + notificationTimeNames[notificationTimeIndex]);
	}

	private static void ApplyMenuColor(int index)
	{
		if (index == 12)
		{
			menuColorUpdateCounter = 10;
			return;
		}
		MenuColors menuColors = GetMenuColors(index);
		WristMenu.NormalColor = menuColors.NormalColor;
		WristMenu.ButtonColorEnabled = menuColors.ButtonColorEnabled;
		WristMenu.ButtonColorDisable = menuColors.ButtonColorDisable;
		WristMenu.EnableTextColor = menuColors.EnableTextColor;
		WristMenu.DisableTextColor = menuColors.DisableTextColor;
		WristMenu.NextPrevButtonColor = menuColors.NextPrevButtonColor;
		WristMenu.MenuTitleColor = menuColors.MenuTitleColor;
		WristMenu.ToolTipColor = new Color(0.8f, 0.8f, 0.8f);
		WristMenu.NextPrevTextColor = Color.white;
		WristMenu.DisconnectButtonColor = new Color(0.5f, 0f, 0f);
		WristMenu.DisconnectTextColor = Color.white;
	}

	public static void CycleMenuColor()
	{
		menuColorIndex = (menuColorIndex + 1) % 13;
		ApplyMenuColor(menuColorIndex);
		string[] colorNames = new string[] { "Gray", "Dark Gray", "Light Gray", "Red", "Orange", "Teal", "Cyan", "Blue", "Purple", "Magenta", "Pink", "Brown", "Player Color" };
		string name = (menuColorIndex >= 0 && menuColorIndex < colorNames.Length) ? colorNames[menuColorIndex] : "Unknown";
		NotifiLib.SendNotification("[<color=#00ccff>COLOR</color>] Menu Color: " + name, 2);
		WristMenu.DestroyMenu();
		WristMenu.instance.Draw();
	}

	private static void PlatformsThing(bool invis, bool sticky)
	{
		RPlat = WristMenu.gripDownR;
		LPlat = WristMenu.gripDownL;
		if (RPlat)
		{
			if (!once_right && (Object)(object)jump_right_local == (Object)null)
			{
				if (sticky)
				{
					Vector3 handPosR = GorillaTagger.Instance.rightHandTransform.position;
					jump_right_local = new GameObject("StickyRight");
					jump_right_local.transform.position = handPosR;
					jump_right_local.transform.rotation = Quaternion.identity;
					jump_right_local.transform.localScale = Vector3.one;
					GameObject plat = GameObject.CreatePrimitive((PrimitiveType)3);
					plat.transform.SetParent(jump_right_local.transform);
					plat.transform.localScale = scale;
					plat.transform.localPosition = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.RightHand.controllerTransform.position - handPosR;
					plat.transform.localRotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
					plat.AddComponent<GorillaSurfaceOverride>().overrideIndex = 0;
					plat.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
					int boxCount = 25;
					float cageRadius = 0.15f;
					float boxSize = 0.08f;
					float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;
					for (int i = 0; i < boxCount; i++)
					{
						float theta = Mathf.Acos(1f - 2f * (i + 0.5f) / boxCount);
						float phi = 2f * Mathf.PI * i / goldenRatio;
						Vector3 dir = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi), Mathf.Cos(theta));
						GameObject box = GameObject.CreatePrimitive((PrimitiveType)3);
						box.transform.SetParent(jump_right_local.transform);
						Object.Destroy((Object)(object)box.GetComponent<Renderer>());
						Object.Destroy((Object)(object)box.GetComponent<Rigidbody>());
						box.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
						box.transform.localPosition = dir * cageRadius;
					}
					stickyRightActive = true;
					if (NetworkMenuEnabled)
					{
						SendPlatformSpawn(plat.transform.position, plat.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "R");
					}
				}
				else
				{
					jump_right_local = GameObject.CreatePrimitive((PrimitiveType)3);
					jump_right_local.transform.localScale = scale;
					jump_right_local.transform.position = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.RightHand.controllerTransform.position;
					jump_right_local.transform.rotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
					GorillaSurfaceOverride surf = jump_right_local.AddComponent<GorillaSurfaceOverride>();
					surf.overrideIndex = 0;
					jump_right_local.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
					if (NetworkMenuEnabled)
					{
						SendPlatformSpawn(jump_right_local.transform.position, jump_right_local.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "R");
					}
				}
				once_right = true;
				once_right_false = false;
			}
		}
		else if (!once_right_false && (Object)(object)jump_right_local != (Object)null)
		{
			if (NetworkMenuEnabled && !sticky)
			{
				SendPlatformDestroy("R");
			}
			Object.Destroy((Object)(object)jump_right_local);
			jump_right_local = null;
			stickyRightActive = false;
			once_right = false;
			once_right_false = true;
		}
		if (LPlat)
		{
			if (!once_left && (Object)(object)jump_left_local == (Object)null)
			{
				if (sticky)
				{
					Vector3 handPosL = GorillaTagger.Instance.leftHandTransform.position;
					jump_left_local = new GameObject("StickyLeft");
					jump_left_local.transform.position = handPosL;
					jump_left_local.transform.rotation = Quaternion.identity;
					jump_left_local.transform.localScale = Vector3.one;
					GameObject plat = GameObject.CreatePrimitive((PrimitiveType)3);
					plat.transform.SetParent(jump_left_local.transform);
					plat.transform.localScale = scale;
					plat.transform.localPosition = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.LeftHand.controllerTransform.position - handPosL;
					plat.transform.localRotation = GTPlayer.Instance.LeftHand.controllerTransform.rotation;
					plat.AddComponent<GorillaSurfaceOverride>().overrideIndex = 0;
					plat.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
					int boxCount = 25;
					float cageRadius = 0.15f;
					float boxSize = 0.08f;
					float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;
					for (int i = 0; i < boxCount; i++)
					{
						float theta = Mathf.Acos(1f - 2f * (i + 0.5f) / boxCount);
						float phi = 2f * Mathf.PI * i / goldenRatio;
						Vector3 dir = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi), Mathf.Cos(theta));
						GameObject box = GameObject.CreatePrimitive((PrimitiveType)3);
						box.transform.SetParent(jump_left_local.transform);
						Object.Destroy((Object)(object)box.GetComponent<Renderer>());
						Object.Destroy((Object)(object)box.GetComponent<Rigidbody>());
						box.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
						box.transform.localPosition = dir * cageRadius;
					}
					stickyLeftActive = true;
					if (NetworkMenuEnabled)
					{
						SendPlatformSpawn(plat.transform.position, plat.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "L");
					}
				}
				else
				{
					jump_left_local = GameObject.CreatePrimitive((PrimitiveType)3);
					jump_left_local.transform.localScale = scale;
					jump_left_local.transform.position = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.LeftHand.controllerTransform.position;
					jump_left_local.transform.rotation = GTPlayer.Instance.LeftHand.controllerTransform.rotation;
					GorillaSurfaceOverride surf = jump_left_local.AddComponent<GorillaSurfaceOverride>();
					surf.overrideIndex = 0;
					jump_left_local.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
					if (NetworkMenuEnabled)
					{
						SendPlatformSpawn(jump_left_local.transform.position, jump_left_local.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "L");
					}
				}
				once_left = true;
				once_left_false = false;
			}
		}
		else if (!once_left_false && (Object)(object)jump_left_local != (Object)null)
		{
			if (NetworkMenuEnabled && !sticky)
			{
				SendPlatformDestroy("L");
			}
			Object.Destroy((Object)(object)jump_left_local);
			jump_left_local = null;
			stickyLeftActive = false;
			once_left = false;
			once_left_false = true;
		}

	}

	public static ButtonInfo GetButton(string name)
	{
		foreach (MenuCategory category in MenuManager.Categories)
		{
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.buttonText == name)
				{
					return button;
				}
			}
		}
		return null;
	}

	public static void KickGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null) Console.ExecuteCommand("kick", (ReceiverGroup)1, rig.Creator.UserId);
		});
	}

	public static void SilentKickGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null) Console.ExecuteCommand("silkick", (ReceiverGroup)1, rig.Creator.UserId);
		});
	}

	public static void TPGun()
	{
		MakeRightHandGun(delegate
		{
			if ((Object)(object)GTPlayer.Instance != (Object)null && (Object)(object)pointer != (Object)null)
			{
				Vector3 pos = pointer.transform.position;
				Vector3 playerPos = ((Component)GorillaTagger.Instance).transform.position - ((Component)GorillaTagger.Instance.bodyCollider).transform.position + pos;
				GTPlayer.Instance.TeleportTo(playerPos, ((Component)GTPlayer.Instance).transform.rotation, true, false);
				((Component)VRRig.LocalRig).transform.position = pos;
			}
		});
	}


	public static void FlingGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					flingTargetActor = player.ActorNumber;
					if (flingGunCoroutine != null) ((MonoBehaviour)instance).StopCoroutine(flingGunCoroutine);
					flingGunCoroutine = ((MonoBehaviour)instance).StartCoroutine(FlingGunLoop());
				}
			}
		}, delegate
		{
			if (flingGunCoroutine != null)
			{
				((MonoBehaviour)instance).StopCoroutine(flingGunCoroutine);
				flingGunCoroutine = null;
			}
		});
	}

	private static IEnumerator FlingGunLoop()
	{
		while (true)
		{
			Vector3 flingDir = Random.onUnitSphere * 30f + Vector3.up * 15f;
			Console.ExecuteCommand("vel", flingTargetActor, flingDir);
			yield return (object)new WaitForSeconds(0.5f);
		}
	}

	public static void LightningGun()
	{
		MakeRightHandGun(delegate
		{
			Console.ExecuteCommand("strike", (ReceiverGroup)1, pointer.transform.position);
		});
	}

	public static void VibrateGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null) Console.ExecuteCommand("vibrate", player.ActorNumber, 3, 5f);
			}
		});
	}

	public static void NotifyAll()
	{
		Console.ExecuteCommand("notify", (ReceiverGroup)1, "Chud Menu Admin");
	}

	public static void KickAll()
	{
		Player[] array = (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList);
		foreach (Player val in array)
		{
			try
			{
				VRRig val2 = GorillaGameManager.StaticFindRigForPlayer((NetPlayer)val);
				if ((Object)(object)val2 != (Object)null)
				{
					Console.LightningStrike(val2.headMesh.transform.position);
				}
			}
			catch
			{
			}
		}
		if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
		}
	}

	public static void Laser()
	{
		if (!laserApplied)
		{
			laserApplied = true;
			Console.laserEnabled = true;
		}
	}

	public static void DisableLaser()
	{
		if (laserApplied)
		{
			Console.laserEnabled = false;
			Console.ExecuteCommand("laser", (ReceiverGroup)1, false, true);
			Console.ExecuteCommand("laser", (ReceiverGroup)1, false, false);
			lastLaserLeft = false;
			lastLaserRight = false;
			laserApplied = false;
		}
	}

	public static void LaserUpdate()
	{
		if (!Console.laserEnabled)
		{
			return;
		}
		bool leftControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimaryButton;
		bool rightControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
		if (rightControllerPrimaryButton && Time.time > laserDelayRight)
		{
			laserDelayRight = Time.time + 0.1f;
			Color laserColor = GetLaserColor();
			Console.ExecuteCommand("laser", (ReceiverGroup)1, true, true, laserColor.r, laserColor.g, laserColor.b);
			Vector3 val = VRRig.LocalRig.rightHandTransform.right;
			Vector3 val2 = VRRig.LocalRig.rightHandTransform.position + val * 0.1f;
			RaycastHit val3 = default(RaycastHit);
			if (Physics.Raycast(val2 + val / 3f, val, out val3, 512f))
			{
				VRRig componentInParent = ((Component)val3.collider).GetComponentInParent<VRRig>();
				if ((Object)(object)componentInParent != (Object)null && !componentInParent.isLocal && componentInParent.Creator != null)
				{
					Console.ExecuteCommand("silkick", (ReceiverGroup)1, componentInParent.Creator.UserId);
				}
			}
		}
		if (leftControllerPrimaryButton && Time.time > laserDelayLeft)
		{
			laserDelayLeft = Time.time + 0.1f;
			Color laserColor2 = GetLaserColor();
			Console.ExecuteCommand("laser", (ReceiverGroup)1, true, false, laserColor2.r, laserColor2.g, laserColor2.b);
			Vector3 val4 = -VRRig.LocalRig.leftHandTransform.right;
			Vector3 val5 = VRRig.LocalRig.leftHandTransform.position + val4 * 0.1f;
			RaycastHit val6 = default(RaycastHit);
			if (Physics.Raycast(val5 + val4 / 3f, val4, out val6, 512f))
			{
				VRRig componentInParent2 = ((Component)val6.collider).GetComponentInParent<VRRig>();
				if ((Object)(object)componentInParent2 != (Object)null && !componentInParent2.isLocal && componentInParent2.Creator != null)
				{
					Console.ExecuteCommand("silkick", (ReceiverGroup)1, componentInParent2.Creator.UserId);
				}
			}
		}
		lastLaserLeft = leftControllerPrimaryButton;
		lastLaserRight = rightControllerPrimaryButton;
	}

	public static void AssetInteractionUpdate()
	{
		bool flag = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat > 0.5f;
		if (pistolId >= 0 && Console.ConsoleAssets.ContainsKey(pistolId))
		{
			bool flag2 = false;
			if (Console.fullAutoPistol)
			{
				if (flag && Time.time > pistolFireDelay)
				{
					pistolFireDelay = Time.time + 0.0667f;
					flag2 = true;
				}
				if (!flag && lastPistolTrigger)
				{
					Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Model", "Default");
					Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Flash", "Default");
				}
			}
			else
			{
				if (flag && !lastPistolTrigger)
				{
					flag2 = true;
				}
				if (!flag && lastPistolTrigger)
				{
					Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Model", "Default");
					Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Flash", "Default");
				}
			}
			if (flag2)
			{
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Model", "Default");
				Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, pistolId, "Model", "PistolShoot");
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Model", "Shoot");
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, pistolId, "Flash", "Shoot");
			}
		}
		if (coinId >= 0 && Console.ConsoleAssets.ContainsKey(coinId))
		{
			bool rightControllerSecondaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
			if (rightControllerSecondaryButton && !lastCoinSecondary)
			{
				bool flag3 = Random.value > 0.5f;
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, coinId, "CoinHolder", flag3 ? "Heads" : "Tails");
				Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, coinId, "CoinHolder", "Flip");
			}
			lastCoinSecondary = rightControllerSecondaryButton;
		}
		if (banHammerId >= 0 && Console.ConsoleAssets.TryGetValue(banHammerId, out var bhAsset) && bhAsset.obj != null)
		{
			Transform bhRayPoint = bhAsset.obj.transform.Find("Model/HitBox");
			if (bhRayPoint != null)
			{
				if (!bhRayPoint.TryGetComponent(out MeshCollider _))
					bhRayPoint.gameObject.AddComponent<MeshCollider>();
				Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhRay, 0.4f, GetNoInvisLayerMask());
				Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhCRay, 0.4f, GTPlayer.Instance.locomotionEnabledLayers);
				Vector3 bhHandVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
				Vector3 bhBodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
				bool bhVelTooHigh = (bhHandVel - bhBodyVel).magnitude > 10f;
				if (Time.time > slashDelayBH)
				{
					if (bhRay.collider != null)
					{
						VRRig bhTarget = bhRay.collider.GetComponentInParent<VRRig>();
						if (bhTarget != null && !bhTarget.isLocal)
						{
							slashDelayBH = Time.time + 1f;
							pauseSfxBH = Time.time + 1f;
							((MonoBehaviour)Console.instance).StartCoroutine(BanHammerKillFX());
							NetPlayer bhPlayer = bhTarget.Creator;
							Console.ExecuteCommand("silkick", bhPlayer.ActorNumber, bhPlayer.UserId);
						}
					}
					if (bhCRay.collider != null)
					{
						slashDelayBH = Time.time + 0.3f;
						pauseSfxBH = Time.time + 0.5f;
						float bhTotalVel = bhHandVel.magnitude + bhBodyVel.magnitude;
						GorillaTagger.Instance.rigidbody.linearVelocity += bhCRay.normal * Mathf.Clamp(bhTotalVel, 1f, 14f);
						((MonoBehaviour)Console.instance).StartCoroutine(BanHammerHitFX());
					}
				}
				if (bhVelTooHigh && !lastVelTooHighBH && Time.time > pauseSfxBH)
				{
					pauseSfxBH = Time.time + 0.3f;
					Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, banHammerId, "Model/SwingSFX", "Swing");
				}
				lastVelTooHighBH = bhVelTooHigh;
			}
		}
		if (rainbowSwordId >= 0 && Console.ConsoleAssets.TryGetValue(rainbowSwordId, out var rsAsset) && rsAsset.obj != null)
		{
			Transform rsRayPoint = rsAsset.obj.transform.Find("Sword/HitBox");
			if (rsRayPoint != null)
			{
				Physics.SphereCast(rsRayPoint.position, 0.1f, rsRayPoint.forward, out RaycastHit rsRay, 0.7f, GetNoInvisLayerMask());
				if (Time.time > slashDelayRS && rsRay.collider != null)
				{
					try
					{
						VRRig rsTarget = rsRay.collider.GetComponentInParent<VRRig>();
						if (rsTarget != null && !rsTarget.isLocal)
						{
							slashDelayRS = Time.time + 0.5f;
							pauseSfxRS = Time.time + 1f;
							Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, rainbowSwordId, "Sword/SFX", "Slash" + Random.Range(1, 3));
							Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, rainbowSwordId, "Sword", "Particles");
							Console.ExecuteCommand("silkick", (ReceiverGroup)1, rsTarget.Creator.UserId);
						}
					}
					catch
					{
					}
				}
				Vector3 rsHandVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
				Vector3 rsBodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
				bool rsVelTooHigh = (rsHandVel - rsBodyVel).magnitude > 10f;
				if (rsVelTooHigh && !lastVelTooHighRS && Time.time > pauseSfxRS)
				{
					pauseSfxRS = Time.time + 0.3f;
					Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, rainbowSwordId, "Sword/SFX", "Swing" + Random.Range(1, 3));
				}
				lastVelTooHighRS = rsVelTooHigh;
			}
		}
		if (physicsGunId >= 0 && Console.ConsoleAssets.TryGetValue(physicsGunId, out var pgAsset) && pgAsset.obj != null)
		{
			Transform pgRayPoint = pgAsset.obj.transform.Find("raypoint");
			if (pgRayPoint != null)
			{
				Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgCrosshairRay, 512f, GetNoInvisLayerMask());
				if (physGunCrosshair == null)
				{
					physGunCrosshair = GameObject.CreatePrimitive((PrimitiveType)0);
					physGunCrosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
					Object.Destroy(physGunCrosshair.GetComponent<Collider>());
				}
				if (physGunCrosshair != null)
				{
					physGunCrosshair.GetComponent<Renderer>().material.color = Color.white;
					physGunCrosshair.transform.position = (pgCrosshairRay.point == Vector3.zero) ? (pgRayPoint.position + pgRayPoint.forward * 20f) : pgCrosshairRay.point;
				}
				bool pgGrab = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
				if (pgGrab)
				{
					if (physGunTargetHoldVRRig == null)
					{
						Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit, 512f, GetNoInvisLayerMask());
						VRRig pgNewTarget = pgHit.collider?.GetComponentInParent<VRRig>();
						if (pgNewTarget != null && !pgNewTarget.isLocal)
						{
							physGunTargetHoldVRRig = pgNewTarget;
							physGunRigDistance = pgHit.distance;
							Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, physicsGunId, "model", "bright");
							Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, physicsGunId, "oneshot", "zap");
							Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, physicsGunId, "constant", "hold");
						}
					}
					else
					{
						Vector2 pgJoy = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimary2DAxis;
						if (Mathf.Abs(pgJoy.y) > 0.2f)
							physGunRigDistance += Time.deltaTime * (pgJoy.y > 0f ? 1f : -1f) * 4f;
						Vector3 pgTargetPos = pgRayPoint.position + pgRayPoint.forward * physGunRigDistance;
						physGunTargetHoldVRRig.syncPos = pgTargetPos;
						if (Time.time > physGunPositionDelay)
						{
							physGunPositionDelay = Time.time + 0.05f;
							Console.ExecuteCommand("tpnv", physGunTargetHoldVRRig.Creator.ActorNumber, pgTargetPos);
						}
					}
				}
				if (physGunLastGrip && !pgGrab && physGunTargetHoldVRRig != null)
				{
					float pgTrigger = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat;
					if (pgTrigger > 0.5f)
						Console.ExecuteCommand("vel", physGunTargetHoldVRRig.Creator.ActorNumber, pgRayPoint.forward * 30f);
					Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, physicsGunId, "model", pgTrigger > 0.5f ? "flash" : "default");
					Console.ExecuteCommand("asset-stopsound", (ReceiverGroup)1, physicsGunId, "constant");
					Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, physicsGunId, "oneshot", pgTrigger > 0.5f ? ("launch" + Random.Range(1, 4)) : "drop");
					physGunStandaloneTriggerDelay = Time.time + 0.5f;
					physGunTargetHoldVRRig = null;
				}
				physGunLastGrip = pgGrab;
				float pgTrigger2 = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat;
				if (pgTrigger2 > 0.5f && !pgGrab && Time.time > physGunStandaloneTriggerDelay)
				{
					Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit2, 512f, GetNoInvisLayerMask());
					VRRig pgTarget2 = pgHit2.collider?.GetComponentInParent<VRRig>();
					if (pgTarget2 != null && !pgTarget2.isLocal)
					{
						physGunStandaloneTriggerDelay = Time.time + 0.5f;
						Console.ExecuteCommand("vel", pgTarget2.Creator.ActorNumber, pgRayPoint.forward * 30f);
						Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, physicsGunId, "model", "flash");
						Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, physicsGunId, "oneshot", "launch" + Random.Range(1, 4));
					}
				}
			}
		}
		lastPistolTrigger = flag;
	}

	public static void JoinCode(string code)
	{
		NotifiLib.SendNotification("[<color=green>FUN</color>] Joining room: " + code);
		NetworkSystem.Instance.ReturnToSinglePlayer();
		((MonoBehaviour)instance).StartCoroutine(Console.JoinRoom(code));
	}

	public static void GetPlayerIDGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				GUIUtility.systemCopyBuffer = rig.Creator.UserId;
				NotifiLib.SendNotification("[<color=green>PLAYER ID</color>] Copied: " + rig.Creator.UserId);
			}
		});
	}

	public static void LaunchPlayerGun()
	{
		MakeRightHandGun(delegate
		{
			launchPlayerGunReturnPos = ((Component)VRRig.LocalRig).transform.position;
			((Component)VRRig.LocalRig).transform.position = pointer.transform.position;
			launchPlayerGunFramesLeft = 10;
		});
	}

	public static void GetIDSelf()
	{
		string text = (GUIUtility.systemCopyBuffer = PhotonNetwork.LocalPlayer.UserId);
		NotifiLib.SendNotification("[<color=green>PLAYER ID</color>] Copied self: " + text);
	}

	public static void UnlockVim()
	{
		if (vimHarmony != null)
		{
			return;
		}
		vimHarmony = new Harmony("chudmenu.vim");
		Type type = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType("GorillaTagScripts.SubscriptionManager");
			if (type != null)
			{
				break;
			}
		}
		if (type != null)
		{
			MethodInfo method = type.GetMethod("IsLocalSubscribed", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				MethodInfo method2 = typeof(Mods).GetMethod("VimPrefix", BindingFlags.Static | BindingFlags.Public);
				vimHarmony.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	public static void DisableUnlockVim()
	{
		if (vimHarmony != null)
		{
			vimHarmony.UnpatchSelf();
			vimHarmony = null;
		}
	}

	public static void EnableSeeAntiCheatReports()
	{
		seeAntiCheatReports = true;
	}

	public static void DisableSeeAntiCheatReports()
	{
		seeAntiCheatReports = false;
		antiCheatReportCounts.Clear();
	}

	public static bool VimPrefix(ref bool __result)
	{
		__result = true;
		return false;
	}

	public static void TagGun()
	{
		if (!WristMenu.gripDownR)
		{
			tagGunTriggerWasDown = false;
			if (tagGunLockedTarget != null)
			{
				tagGunLockedTarget = null;
				((Behaviour)VRRig.LocalRig).enabled = true;
			}
			CleanupGun();
		}
		else
		{
			tagGunTriggerWasDown = WristMenu.triggerDownR;
			MakeRightHandGun(delegate
			{
				VRRig val3 = GetGunTargetPlayer();
				if (val3 != null && !val3.isLocal)
				{
					GorillaTagManager val5 = GorillaGameManager.instance as GorillaTagManager;
					if (val5 != null && !val5.IsInfected(val3.Creator))
					{
						if (PhotonNetwork.IsMasterClient)
						{
							val5.AddInfectedPlayer(val3.Creator, true);
						}
						else
						{
							tagGunLockedTarget = val3;
							tagGunFramesUntilTag = 12;
						}
					}
				}
			}, delegate { });
			if (tagGunLockedTarget != null && pointer != null && Line != null)
			{
				pointer.transform.position = ((Component)tagGunLockedTarget).transform.position;
				Line.SetPosition(1, ((Component)tagGunLockedTarget).transform.position);
			}
		}
		GorillaGameManager val = GorillaGameManager.instance;
		GorillaTagManager val2 = (val is GorillaTagManager tgm) ? tgm : null;
		if (val2 == null || tagGunLockedTarget == null) return;
		if (tagGunLockedTarget.Creator == null || val2.IsInfected(tagGunLockedTarget.Creator))
		{
			tagGunLockedTarget = null;
			((Behaviour)VRRig.LocalRig).enabled = true;
			return;
		}
		((Behaviour)VRRig.LocalRig).enabled = false;
		((Component)VRRig.LocalRig).transform.position = ((Component)tagGunLockedTarget).transform.position - new Vector3(0f, 3f, 0f);
		tagGunFramesUntilTag--;
		if (tagGunFramesUntilTag <= 0)
		{
			tagGunFramesUntilTag = 12;
			if (PhotonNetwork.IsMasterClient)
			{
				val2.AddInfectedPlayer(tagGunLockedTarget.Creator, true);
			}
			else
			{
				GameMode.ReportTag(tagGunLockedTarget.Creator);
			}
		}
	}

	public static void UntagSelf()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		if (!((Object)(object)val != (Object)null))
		{
			return;
		}
		GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
		if (val2 != null && val2.IsInfected(NetworkSystem.Instance.LocalPlayer) && Time.time > lastUntagSelfTime)
		{
			val2.currentInfected.RemoveAll((NetPlayer p) => p.UserId == NetworkSystem.Instance.LocalPlayer.UserId);
			lastUntagSelfTime = Time.time + 0.3f;
			NotifiLib.SendNotification("[<color=green>MASTER</color>] Untagged self");
		}
	}

	public static void TagAll()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		GorillaTagManager val2 = (val is GorillaTagManager tgm) ? tgm : null;
		if (val2 == null) return;

		if (tagAllTarget == null || tagAllTarget.Creator == null || val2.IsInfected(tagAllTarget.Creator))
		{
			if (tagAllTarget != null)
				((Behaviour)VRRig.LocalRig).enabled = true;

			if (tagAllTargets == null || tagAllIndex >= tagAllTargets.Count)
			{
				tagAllTargets = new List<VRRig>();
				foreach (VRRig r in VRRigCache.ActiveRigs)
					if (!r.isLocal && r.Creator != null && !val2.IsInfected(r.Creator))
						tagAllTargets.Add(r);
				tagAllIndex = 0;
			}

			if (tagAllIndex >= tagAllTargets.Count)
				return;

			tagAllTarget = tagAllTargets[tagAllIndex];
			tagAllIndex++;
			if (PhotonNetwork.IsMasterClient)
			{
				val2.AddInfectedPlayer(tagAllTarget.Creator, true);
				tagAllTarget = null;
				return;
			}
			tagAllFramesUntilTag = 30;
		}

		((Behaviour)VRRig.LocalRig).enabled = false;
		((Component)VRRig.LocalRig).transform.position = ((Component)tagAllTarget).transform.position - new Vector3(0f, 3f, 0f);
		tagAllFramesUntilTag--;

		if (tagAllFramesUntilTag <= 0)
		{
			tagAllFramesUntilTag = 30;
			GameMode.ReportTag(tagAllTarget.Creator);
		}
	}

	public static void DisableTagAll()
	{
		tagAllTarget = null;
		tagAllTargets = null;
		tagAllIndex = 0;
		tagAllFramesUntilTag = 0;
		if ((Object)(object)VRRig.LocalRig != (Object)null)
			((Behaviour)VRRig.LocalRig).enabled = true;
	}

	public static void UntagGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				GorillaGameManager gm = GorillaGameManager.instance;
				if (gm != null)
				{
					GorillaTagManager tagMan = gm as GorillaTagManager;
					if (tagMan != null && tagMan.IsInfected(rig.Creator) && Time.time > lastUntagNotif)
					{
						tagMan.currentInfected.RemoveAll(p => p.UserId == rig.Creator.UserId);
						lastUntagNotif = Time.time + 0.3f;
						NotifiLib.SendNotification("[<color=green>MASTER</color>] Untagged " + rig.Creator.NickName);
					}
				}
			}
		});
	}

	public static void TagWhileNotTagged()
	{
		float num = 0.15f;
		Collider[] array = Physics.OverlapSphere(GorillaTagger.Instance.rightHandTransform.position, num);
		Collider[] array2 = array;
		foreach (Collider val in array2)
		{
			VRRig componentInParent = ((Component)val).GetComponentInParent<VRRig>();
			if (!((Object)(object)componentInParent != (Object)null) || componentInParent.isLocal || componentInParent.Creator == null || !(Time.time > tagUntaggedCooldown))
			{
				continue;
			}
			GorillaGameManager val2 = GorillaGameManager.instance;
			if ((Object)(object)val2 != (Object)null)
			{
				GorillaTagManager val3 = (GorillaTagManager)(object)((val2 is GorillaTagManager) ? val2 : null);
				if (val3 != null && !val3.IsInfected(componentInParent.Creator))
				{
					val3.AddInfectedPlayer(componentInParent.Creator, true);
					tagUntaggedCooldown = Time.time + 0.3f;
					NotifiLib.SendNotification("[<color=green>MASTER</color>] Tagged " + componentInParent.Creator.NickName);
				}
			}
		}
		array = Physics.OverlapSphere(GorillaTagger.Instance.leftHandTransform.position, num);
		Collider[] array3 = array;
		foreach (Collider val4 in array3)
		{
			VRRig componentInParent2 = ((Component)val4).GetComponentInParent<VRRig>();
			if (!((Object)(object)componentInParent2 != (Object)null) || componentInParent2.isLocal || componentInParent2.Creator == null || !(Time.time > tagUntaggedCooldown))
			{
				continue;
			}
			GorillaGameManager val5 = GorillaGameManager.instance;
			if ((Object)(object)val5 != (Object)null)
			{
				GorillaTagManager val6 = (GorillaTagManager)(object)((val5 is GorillaTagManager) ? val5 : null);
				if (val6 != null && !val6.IsInfected(componentInParent2.Creator))
				{
					val6.AddInfectedPlayer(componentInParent2.Creator, true);
					tagUntaggedCooldown = Time.time + 0.3f;
					NotifiLib.SendNotification("[<color=green>MASTER</color>] Tagged " + componentInParent2.Creator.NickName);
				}
			}
		}
	}

	public static void TeleportToSpawn()
	{
		GorillaTagger gt = GorillaTagger.Instance;
		if (gt == null) return;
		GTPlayer player = GTPlayer.Instance;
		if (player == null) return;
		Vector3 stump = stumpPosition;
		Transform bodyT = ((Component)gt.bodyCollider).transform;
		player.TeleportTo(stump - bodyT.position + ((Component)player).transform.position, ((Component)player).transform.rotation, true, false);
		bodyT.position = stump;
		if (VRRig.LocalRig != null)
			((Component)VRRig.LocalRig).transform.position = stump;
		((Collider)gt.bodyCollider).enabled = false;
		((MonoBehaviour)gt).StartCoroutine(ReenableBodyCollider());
	}

	private static IEnumerator ReenableBodyCollider()
	{
		yield return (object)new WaitForSeconds(1.5f);
		if (GorillaTagger.Instance != null)
			((Collider)GorillaTagger.Instance.bodyCollider).enabled = true;
	}

	public static void SpazAll()
	{
		spazAllActive = true;
	}

	public static void DisableSpazAll()
	{
		spazAllActive = false;
	}

	public static void SpazSelf()
	{
		spazSelfActive = true;
	}

	public static void DisableSpazSelf()
	{
		spazSelfActive = false;
	}

	private static void RunSpaz()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
		if (val2 == null || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (spazAllActive)
		{
			Player[] playerList = PhotonNetwork.PlayerList;
			for (int i = 0; i < playerList.Length; i++)
			{
				NetPlayer p = playerList[i];
				if (val2.isCurrentlyTag)
				{
					if (val2.currentIt == p)
					{
						val2.currentIt = null;
					}
					else if (val2.currentIt == null)
					{
						val2.currentIt = p;
					}
				}
				else if (val2.IsInfected(p))
				{
					val2.currentInfected.RemoveAll((NetPlayer x) => x.UserId == p.UserId);
				}
				else
				{
					val2.AddInfectedPlayer(p, true);
				}
			}
		}
		if (!spazSelfActive)
		{
			return;
		}
		NetPlayer self = NetworkSystem.Instance.LocalPlayer;
		if (val2.isCurrentlyTag)
		{
			if (val2.currentIt == self)
			{
				val2.currentIt = null;
			}
			else
			{
				val2.currentIt = self;
			}
		}
		else if (val2.IsInfected(self))
		{
			val2.currentInfected.RemoveAll((NetPlayer x) => x.UserId == self.UserId);
		}
		else
		{
			val2.AddInfectedPlayer(self, true);
		}
	}

	public static void MakeGun(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action onTrigger, Action onRelease)
	{
		if ((Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform)
		{
			hand = WristMenu.gripDownR;
			hand1 = WristMenu.triggerDownR;
		}
		else if ((Object)(object)arm == (Object)(object)GTPlayer.Instance.LeftHand.controllerTransform)
		{
			hand = WristMenu.gripDownL;
			hand1 = WristMenu.triggerDownL;
		}
		if (hand)
		{
			if (pcGunsEnabled && Mouse.current != null && !XRSettings.isDeviceActive)
			{
				if ((Object)(object)pcGunCamera == (Object)null)
				{
					GameObject val = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if ((Object)(object)val != (Object)null)
					{
						pcGunCamera = val.GetComponent<Camera>();
					}
					if ((Object)(object)pcGunCamera == (Object)null)
					{
						val = GameObject.Find("Shoulder Camera");
						if ((Object)(object)val != (Object)null)
						{
							pcGunCamera = val.GetComponent<Camera>();
						}
					}
				}
				if ((Object)(object)pcGunCamera != (Object)null)
				{
					Ray val2 = pcGunCamera.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
					Physics.Raycast(arm.position, val2.direction, out raycastHit, 512f, GetNoInvisLayerMask());
				}
				else
				{
				Physics.Raycast(arm.position, -arm.up, out raycastHit);
				}
			}
			else
			{
				Physics.Raycast(arm.position, -arm.up, out raycastHit);
			}
			if ((Object)(object)pointer == (Object)null)
			{
				pointer = GameObject.CreatePrimitive(pointershape);
			}
			pointer.transform.localScale = pointersize;
			pointer.GetComponent<Renderer>().material.shader = CachedGuiTextShader;
			pointer.transform.position = raycastHit.point;
			pointer.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
			if (liner)
			{
				if ((Object)(object)Line == (Object)null)
				{
					GameObject val3 = new GameObject("GunLine");
					Line = val3.AddComponent<LineRenderer>();
					((Renderer)Line).material.shader = CachedGuiTextShader;
					Line.startWidth = linesize;
					Line.endWidth = linesize;
					Line.positionCount = 2;
					Line.useWorldSpace = true;
				}
				Line.startColor = WristMenu.ButtonColorEnabled;
				Line.endColor = WristMenu.ButtonColorEnabled;
				Line.SetPosition(0, arm.position);
				Line.SetPosition(1, pointer.transform.position);
			}
			Object.Destroy((Object)(object)pointer.GetComponent<BoxCollider>());
			Object.Destroy((Object)(object)pointer.GetComponent<Rigidbody>());
			Object.Destroy((Object)(object)pointer.GetComponent<Collider>());
			if (hand1 && !gunTriggerWasDown)
			{
				try
				{
					onTrigger();
				}
				catch
				{
				}
			}
			else if (!hand1)
			{
				try
				{
					onRelease();
				}
				catch
				{
				}
			}
			if (hand1)
			{
				pointer.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled * 0.5f;
			}
			gunTriggerWasDown = hand1;
		}
		else
		{
			if ((Object)(object)pointer != (Object)null)
			{
				Object.Destroy((Object)(object)pointer, Time.deltaTime);
				pointer = null;
			}
			if ((Object)(object)Line != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)Line).gameObject);
				Line = null;
			}
			gunTriggerWasDown = false;
		}
	}

	private static void MakeRightHandGun(Action onTrigger, Action onRelease = null)
	{
		MakeGun(Color.white, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, onTrigger, onRelease ?? delegate { });
	}

	private static VRRig GetGunTargetPlayer()
	{
		VRRig rig = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
		return rig != null && rig.Creator != null ? rig : null;
	}

	public static void BlockJmanSounds()
	{
		blockJmanSounds = true;
		JmanSoundPatch.enabled = true;
	}

	public static void DisableBlockJmanSounds()
	{
		blockJmanSounds = false;
		JmanSoundPatch.enabled = false;
	}

	public static void AntiGuardianGrab()
	{
		antiGuardianGrab = true;
		GuardianLaunchPatch.enabled = true;
		GuardianKnockbackPatch.enabled = true;
		GuardianClampedKnockbackPatch.enabled = true;
		GuardianTrajectoryPatch.enabled = true;
		GuardianGrabbedByPatch.enabled = true;
	}

	public static void DisableAntiGuardianGrab()
	{
		antiGuardianGrab = false;
		GuardianLaunchPatch.enabled = false;
		GuardianKnockbackPatch.enabled = false;
		GuardianClampedKnockbackPatch.enabled = false;
		GuardianTrajectoryPatch.enabled = false;
		GuardianGrabbedByPatch.enabled = false;
	}

	public static void AntiAFK()
	{
		try
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = true;
		}
		catch
		{
		}
	}

	public static void DisableAntiAFK()
	{
		try
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = false;
		}
		catch
		{
		}
	}

	public static void EnableDisableNetworkTriggers()
	{
		NetworkTriggerPatch.enabled = true;
	}

	public static void DisableDisableNetworkTriggers()
	{
		NetworkTriggerPatch.enabled = false;
	}

	public static void EnableDisableQuitBox()
	{
		QuitBoxPatch.enabled = false;
	}

	public static void DisableDisableQuitBox()
	{
		QuitBoxPatch.enabled = true;
	}

	public static void EnablePCButtonClick()
	{
		pcButtonClickEnabled = true;
	}

	public static void DisablePCButtonClick()
	{
		pcButtonClickEnabled = false;
		if (pcButtonOldLocalPosition.HasValue)
		{
			GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = pcButtonOldLocalPosition.Value;
			pcButtonOldLocalPosition = null;
		}
		if ((Object)(object)GorillaTagger.Instance.rightHandTriggerCollider != (Object)null)
		{
			TransformFollow component = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = true;
			}
		}
	}

	private static void UpdatePCButtonClick()
	{
		if (!pcButtonClickEnabled || (Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)GorillaTagger.Instance.rightHandTriggerCollider == (Object)null)
		{
			return;
		}
		if (Mouse.current != null && Mouse.current.leftButton.isPressed)
		{
			Camera val = null;
			Camera[] array = Object.FindObjectsByType<Camera>((FindObjectsSortMode)0);
			Camera[] array2 = array;
			foreach (Camera val2 in array2)
			{
				if (((Object)val2).name == "Shoulder Camera" || ((Object)(object)((Component)val2).gameObject.transform.parent != (Object)null && ((Object)((Component)val2).gameObject.transform.parent).name == "Third Person Camera"))
				{
					val = val2;
					break;
				}
			}
			if (!((Object)(object)val != (Object)null))
			{
				return;
			}
			Ray val3 = val.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
			RaycastHit val4 = default(RaycastHit);
			if (!Physics.Raycast(val3, out val4, 512f, GetNoInvisLayerMask()))
			{
				return;
			}
			if (!pcButtonOldLocalPosition.HasValue)
			{
				pcButtonOldLocalPosition = GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition;
				TransformFollow component = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
				if ((Object)(object)component != (Object)null)
				{
					((Behaviour)component).enabled = false;
				}
			}
			GorillaTagger.Instance.rightHandTriggerCollider.transform.position = val4.point;
		}
		else
		{
			if (pcButtonOldLocalPosition.HasValue)
			{
				GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = pcButtonOldLocalPosition.Value;
				pcButtonOldLocalPosition = null;
			}
			TransformFollow component2 = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Behaviour)component2).enabled = true;
			}
		}
	}

	public static int GetNoInvisLayerMask()
	{
		if (!noInvisLayerMask.HasValue)
		{
			noInvisLayerMask = ~((1 << LayerMask.NameToLayer("TransparentFX")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Zone")) | (1 << LayerMask.NameToLayer("Gorilla Trigger")) | (1 << LayerMask.NameToLayer("Gorilla Boundary")) | (1 << LayerMask.NameToLayer("GorillaCosmetics")) | (1 << LayerMask.NameToLayer("GorillaParticle")));
		}
		return noInvisLayerMask ?? (int)GTPlayer.Instance.locomotionEnabledLayers;
	}

	public static void EnablePCGuns()
	{
		pcGunsEnabled = true;
	}

	public static void DisablePCGuns()
	{
		pcGunsEnabled = false;
	}

	private static void UpdatePCGuns()
	{
		if (pcGunsEnabled && Mouse.current != null && !XRSettings.isDeviceActive)
		{
			if (Mouse.current.leftButton.isPressed)
			{
				((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat = 1f;
				((ControllerInputPoller)ControllerInputPoller.instance).rightControllerTriggerButton = true;
				WristMenu.triggerDownR = true;
			}
			if (Mouse.current.rightButton.isPressed)
			{
				((ControllerInputPoller)ControllerInputPoller.instance).rightGrab = true;
				((ControllerInputPoller)ControllerInputPoller.instance).rightControllerGripFloat = 1f;
				WristMenu.gripDownR = true;
			}
		}
	}

	public static void MuteGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				try
				{
					foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
					{
						if (line.linePlayer != null && line.linePlayer.UserId == rig.Creator.UserId)
						{
							line.muteButton.isOn = !line.muteButton.isOn;
							line.PressButton(line.muteButton.isOn, (GorillaPlayerLineButton.ButtonType)3);
						}
					}
				}
				catch
				{
				}
			}
		});
	}

	public static void EnableRightHand()
	{
		right = true;
	}

	public static void DisableRightHand()
	{
		right = false;
	}

	public static MenuColors GetMenuColors(int index)
	{
		MenuColors result = default(MenuColors);
		switch (index)
		{
		case 0:
			result.NormalColor = new Color(0.15f, 0.15f, 0.15f);
			result.ButtonColorEnabled = new Color(0.5f, 0.5f, 0.5f);
			result.ButtonColorDisable = new Color(0.25f, 0.25f, 0.25f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.75f, 0.75f, 0.75f);
			result.NextPrevButtonColor = new Color(0.15f, 0.15f, 0.15f);
			break;
		case 1:
			result.NormalColor = new Color(0.1f, 0.1f, 0.1f);
			result.ButtonColorEnabled = new Color(0.35f, 0.35f, 0.35f);
			result.ButtonColorDisable = new Color(0.18f, 0.18f, 0.18f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.6f, 0.6f, 0.6f);
			result.NextPrevButtonColor = new Color(0.18f, 0.18f, 0.18f);
			break;
		case 2:
			result.NormalColor = new Color(0.3f, 0.3f, 0.3f);
			result.ButtonColorEnabled = new Color(0.65f, 0.65f, 0.65f);
			result.ButtonColorDisable = new Color(0.4f, 0.4f, 0.4f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.85f, 0.85f, 0.85f);
			result.NextPrevButtonColor = new Color(0.4f, 0.4f, 0.4f);
			break;
		case 3:
			result.NormalColor = new Color(0.2f, 0.04f, 0.04f);
			result.ButtonColorEnabled = new Color(0.7f, 0.15f, 0.15f);
			result.ButtonColorDisable = new Color(0.4f, 0.08f, 0.08f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.7f, 0.4f, 0.4f);
			result.NextPrevButtonColor = new Color(0.4f, 0.08f, 0.08f);
			break;
		case 4:
			result.NormalColor = new Color(0.25f, 0.1f, 0.02f);
			result.ButtonColorEnabled = new Color(0.85f, 0.45f, 0.1f);
			result.ButtonColorDisable = new Color(0.5f, 0.25f, 0.05f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.75f, 0.5f, 0.3f);
			result.NextPrevButtonColor = new Color(0.5f, 0.25f, 0.05f);
			break;
		case 5:
			result.NormalColor = new Color(0.04f, 0.15f, 0.13f);
			result.ButtonColorEnabled = new Color(0.1f, 0.55f, 0.5f);
			result.ButtonColorDisable = new Color(0.05f, 0.35f, 0.3f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.4f, 0.65f, 0.6f);
			result.NextPrevButtonColor = new Color(0.05f, 0.35f, 0.3f);
			break;
		case 6:
			result.NormalColor = new Color(0.04f, 0.18f, 0.18f);
			result.ButtonColorEnabled = new Color(0.12f, 0.65f, 0.65f);
			result.ButtonColorDisable = new Color(0.06f, 0.4f, 0.4f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.45f, 0.75f, 0.75f);
			result.NextPrevButtonColor = new Color(0.06f, 0.4f, 0.4f);
			break;
		case 7:
			result.NormalColor = new Color(0.04f, 0.04f, 0.2f);
			result.ButtonColorEnabled = new Color(0.15f, 0.3f, 0.65f);
			result.ButtonColorDisable = new Color(0.08f, 0.15f, 0.4f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.4f, 0.5f, 0.75f);
			result.NextPrevButtonColor = new Color(0.08f, 0.15f, 0.4f);
			break;
		case 8:
			result.NormalColor = new Color(0.12f, 0.04f, 0.18f);
			result.ButtonColorEnabled = new Color(0.45f, 0.15f, 0.65f);
			result.ButtonColorDisable = new Color(0.25f, 0.08f, 0.4f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.55f, 0.4f, 0.7f);
			result.NextPrevButtonColor = new Color(0.25f, 0.08f, 0.4f);
			break;
		case 9:
			result.NormalColor = new Color(0.18f, 0.04f, 0.18f);
			result.ButtonColorEnabled = new Color(0.65f, 0.15f, 0.65f);
			result.ButtonColorDisable = new Color(0.4f, 0.08f, 0.4f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.65f, 0.4f, 0.65f);
			result.NextPrevButtonColor = new Color(0.4f, 0.08f, 0.4f);
			break;
		case 10:
			result.NormalColor = new Color(0.25f, 0.04f, 0.12f);
			result.ButtonColorEnabled = new Color(0.85f, 0.3f, 0.55f);
			result.ButtonColorDisable = new Color(0.5f, 0.12f, 0.3f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.75f, 0.4f, 0.55f);
			result.NextPrevButtonColor = new Color(0.5f, 0.12f, 0.3f);
			break;
		case 11:
			result.NormalColor = new Color(0.18f, 0.1f, 0.04f);
			result.ButtonColorEnabled = new Color(0.55f, 0.35f, 0.15f);
			result.ButtonColorDisable = new Color(0.35f, 0.2f, 0.08f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.65f, 0.5f, 0.4f);
			result.NextPrevButtonColor = new Color(0.35f, 0.2f, 0.08f);
			break;
		default:
			result = GetMenuColors(0);
			break;
		}
		result.MenuTitleColor = result.EnableTextColor;
		return result;
	}

	public static void EnableNetworkMenu()
	{
		NetworkMenuEnabled = true;
		networkMenuSyncTimer = Time.time;
	}

	public static void DisableNetworkMenu()
	{
		SendMenuClose();
		NetworkMenuEnabled = false;
		foreach (KeyValuePair<int, RemoteMenuState> remoteMenu in remoteMenus)
		{
			if ((Object)(object)remoteMenu.Value.displayObject != (Object)null && !remoteMenu.Value.closing)
			{
				NetworkMenuDisplay.CloseAndDestroy(remoteMenu.Value);
			}
		}
		remoteMenus.Clear();
		foreach (GameObject value in remotePlatforms.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		remotePlatforms.Clear();
	}

	public static Vector3 GetMenuPosition()
	{
		if ((Object)(object)WristMenu.menu != (Object)null)
		{
			return WristMenu.menu.transform.position;
		}
		if (right)
		{
			return GTPlayer.Instance.RightHand.controllerTransform.position;
		}
		return GTPlayer.Instance.LeftHand.controllerTransform.position;
	}

	public static Quaternion GetMenuRotation()
	{
		if ((Object)(object)WristMenu.menu != (Object)null)
		{
			return WristMenu.menu.transform.rotation;
		}
		if (right)
		{
			return GTPlayer.Instance.RightHand.controllerTransform.rotation;
		}
		return GTPlayer.Instance.LeftHand.controllerTransform.rotation;
	}

	private static int[] GetChudPlayerTargets()
	{
		return PhotonNetwork.PlayerListOthers
			.Where((Player p) => p.CustomProperties.TryGetValue("Chud menu", out var val) && val is bool b && b)
			.Select((Player p) => p.ActorNumber)
			.ToArray();
	}

	public static void SendMenuState()
	{
		if (!NetworkMenuEnabled || !PhotonNetwork.InRoom)
		{
			return;
		}
		string currentCategoryName = MenuManager.CurrentCategoryName;
		int pageNumber = WristMenu.pageNumber;
		Vector3 menuPosition = GetMenuPosition();
		Quaternion menuRotation = GetMenuRotation();
		long num = 0L;
		long num2 = 0L;
		int num3 = 0;
		foreach (MenuCategory category in MenuManager.Categories)
		{
			if (category.Buttons == null)
			{
				continue;
			}
			if (category.Name == "Sound" || category.Name == "Video" || category.Name == "Soundboard")
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.nontoggleable != true)
				{
					if (button.enabled == true)
					{
						if (num3 < 64)
						{
							num |= 1L << num3;
						}
						else
						{
							num2 |= 1L << num3 - 64;
						}
					}
					num3++;
				}
			}
		}
		PhotonNetwork.RaiseEvent((byte)69, (object)new object[9]
		{
			"chudmenu_state",
			currentCategoryName,
			pageNumber,
			menuColorIndex,
			menuPosition,
			menuRotation,
			WristMenu.animationsEnabled,
			num,
			num2
		}, new RaiseEventOptions
		{
			TargetActors = GetChudPlayerTargets()
		}, SendOptions.SendUnreliable);
	}

	public static void SendMenuClose()
	{
		if (NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[1] { "chudmenu_close" }, new RaiseEventOptions
			{
				TargetActors = GetChudPlayerTargets()
			}, SendOptions.SendReliable);
		}
	}

	public static void ReceiveRemoteMenuClose(Player sender)
	{
		if (NetworkMenuEnabled)
		{
			int actorNumber = sender.ActorNumber;
			if (remoteMenus.TryGetValue(actorNumber, out var value) && (Object)(object)value.displayObject != (Object)null && !value.closing)
			{
				NetworkMenuDisplay.CloseAndDestroy(value);
			}
		}
	}

	public static void SendButtonClick()
	{
		if (NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[4]
			{
				"chudmenu_click",
				ButtonSound,
				right,
				WristMenu.buttonSoundIndex
			}, new RaiseEventOptions
			{
				TargetActors = GetChudPlayerTargets()
			}, SendOptions.SendReliable);
		}
	}

	public static void ReceiveRemoteButtonClick(Player sender, int sound, bool rightHand, int soundIdx)
	{
		if (!NetworkMenuEnabled)
		{
			return;
		}
		if (soundIdx == 1 && (Object)(object)WristMenu.customButtonClick != (Object)null)
		{
			Vector3 val = Vector3.zero;
			if (remoteMenus.TryGetValue(sender.ActorNumber, out var value) && (Object)(object)value.displayObject != (Object)null)
			{
				val = value.displayObject.transform.position;
			}
			else
			{
				VRRig vRRigFromPlayer = Console.GetVRRigFromPlayer(sender);
				if ((Object)(object)vRRigFromPlayer != (Object)null)
				{
					val = ((Component)vRRigFromPlayer).transform.position;
				}
			}
			AudioSource.PlayClipAtPoint(WristMenu.customButtonClick, val, 0.5f);
		}
		else
		{
			VRRig vRRigFromPlayer2 = Console.GetVRRigFromPlayer(sender);
			if ((Object)(object)vRRigFromPlayer2 != (Object)null)
			{
				vRRigFromPlayer2.PlayHandTapLocal(sound, !rightHand, 0.1f);
			}
		}
	}

	public static void ReceiveRemoteMenuState(Player sender, string category, int page, int colorIdx, Vector3 pos, Quaternion rot, Dictionary<string, bool> states, bool remoteAnimationsEnabled = true)
	{
		if (!NetworkMenuEnabled)
		{
			return;
		}
		int actorNumber = sender.ActorNumber;
		if (!remoteMenus.TryGetValue(actorNumber, out var value) || !value.closing)
		{
			bool flag = !remoteMenus.TryGetValue(actorNumber, out value);
			if (flag)
			{
				value = new RemoteMenuState
				{
					player = sender
				};
				remoteMenus[actorNumber] = value;
			}
			bool flag2 = !flag && (value.page != page || value.category != category);
			value.category = category;
			value.page = page;
			value.menuColorIndex = colorIdx;
			value.menuColors = GetMenuColors(colorIdx);
			value.position = pos;
			value.rotation = rot;
			value.buttonStates = states;
			value.lastStateTime = Time.time;
			value.animationsEnabled = remoteAnimationsEnabled;
			if ((Object)(object)value.displayObject == (Object)null)
			{
				NetworkMenuDisplay.Create(value);
			}
			else
			{
				if (flag2)
				{
					NetworkMenuDisplay.UpdateState(value);
				}
				else
				{
					NetworkMenuDisplay.UpdateColors(value);
				}
				NetworkMenuDisplay.UpdatePosition(value);
			}
		}
	}

	public static void ReceiveRemoteMenuPosition(Player sender, Vector3 pos, Quaternion rot)
	{
		if (NetworkMenuEnabled && remoteMenus.TryGetValue(sender.ActorNumber, out var value) && !value.closing)
		{
			value.position = pos;
			value.rotation = rot;
			value.lastPosTime = Time.time;
			NetworkMenuDisplay.UpdatePosition(value);
		}
	}

	public static void ReceiveRemoteObject(string objType, Vector3 pos, Vector3 rot, Vector3 scale, Color color)
	{
		if (NetworkMenuEnabled)
		{
			NetworkMenuDisplay.SpawnRemoteObject(objType, pos, rot, scale, color);
		}
	}

	public static void RemoveRemoteMenu(Player player)
	{
		int actorNumber = player.ActorNumber;
		if (remoteMenus.TryGetValue(actorNumber, out var value))
		{
			if ((Object)(object)value.displayObject != (Object)null && !value.closing)
			{
				NetworkMenuDisplay.CloseAndDestroy(value);
			}
			remoteMenus.Remove(actorNumber);
		}
		for (int i = 0; i < 2; i++)
		{
			string key = actorNumber + "_" + ((i == 0) ? "R" : "L");
			if (remotePlatforms.TryGetValue(key, out var value2))
			{
				Object.Destroy((Object)(object)value2);
				remotePlatforms.Remove(key);
			}
		}
	}

	public static void RemoveRemoteMenuState(Player player)
	{
		remoteMenus.Remove(player.ActorNumber);
	}

	public static List<RemoteMenuState> GetRemoteMenus()
	{
		return remoteMenus.Values.Where((RemoteMenuState s) => !s.closing).ToList();
	}

	public static void SendPlatformSpawn(Vector3 pos, Quaternion rot, Vector3 scale, Color color, bool invis, bool sticky, string hand)
	{
		if (NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[10] { "chudplat_create", hand, pos, rot, scale, color.r, color.g, color.b, invis, sticky }, new RaiseEventOptions
			{
				TargetActors = GetChudPlayerTargets()
			}, SendOptions.SendReliable);
		}
	}

	public static void SendPlatformDestroy(string hand)
	{
		if (NetworkMenuEnabled && PhotonNetwork.InRoom)
		{
			PhotonNetwork.RaiseEvent((byte)69, (object)new object[2] { "chudplat_destroy", hand }, new RaiseEventOptions
			{
				TargetActors = GetChudPlayerTargets()
			}, SendOptions.SendReliable);
		}
	}

	public static void ReceiveRemotePlatformSpawn(int senderActor, string hand, Vector3 pos, Quaternion rot, Vector3 scaleVal, Color color, bool invis, bool sticky)
	{
		if (!NetworkMenuEnabled)
		{
			return;
		}
		string key = senderActor + "_" + hand;
		if (!remotePlatforms.ContainsKey(key))
		{
			GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
			Object.Destroy((Object)(object)val.GetComponent<Rigidbody>());
			val.transform.position = pos;
			val.transform.rotation = rot;
			val.transform.localScale = scaleVal;
			if (invis)
			{
				Object.Destroy((Object)(object)val.GetComponent<Renderer>());
			}
			else
			{
				val.GetComponent<Renderer>().material.color = color;
			}
			if (sticky)
			{
				GorillaSurfaceOverride surf = val.AddComponent<GorillaSurfaceOverride>();
				surf.overrideIndex = 0;
				surf.slidePercentageOverride = 0f;
			}
			remotePlatforms[key] = val;
		}
	}

	public static void ReceiveRemotePlatformDestroy(int senderActor, string hand)
	{
		string key = senderActor + "_" + hand;
		if (remotePlatforms.TryGetValue(key, out var value))
		{
			Object.Destroy((Object)(object)value);
			remotePlatforms.Remove(key);
		}
	}

	private static void UpdateNetworkMenu()
	{
		if (!NetworkMenuEnabled || !PhotonNetwork.InRoom)
		{
			return;
		}
		List<int> list = null;
		foreach (KeyValuePair<int, RemoteMenuState> remoteMenu in remoteMenus)
		{
			if (!remoteMenu.Value.closing && Time.time - remoteMenu.Value.lastStateTime > 10f)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(remoteMenu.Key);
			}
		}
		if (list != null)
		{
			foreach (int item in list)
			{
				if (remoteMenus.TryGetValue(item, out var value))
				{
					if ((Object)(object)value.displayObject != (Object)null && !value.closing)
					{
						NetworkMenuDisplay.CloseAndDestroy(value);
					}
					remoteMenus.Remove(item);
				}
			}
		}
		if ((Object)(object)WristMenu.menu != (Object)null && !WristMenu.Close)
		{
			if (Time.time - networkMenuSyncTimer >= 0.033f)
			{
				networkMenuSyncTimer = Time.time;
				SendMenuState();
			}
		}
	}

	public static void SyncNetworkMenuOnJoin(NetPlayer joiningPlayer)
	{
		if (NetworkMenuEnabled && joiningPlayer != NetworkSystem.Instance.LocalPlayer)
		{
			((MonoBehaviour)instance).StartCoroutine(DelayedNetworkSync(joiningPlayer));
		}
	}

		private static IEnumerator DelayedNetworkSync(NetPlayer target)
		{
			yield return (object)new WaitForSeconds(1f);
			if (NetworkMenuEnabled && PhotonNetwork.InRoom && (Object)(object)WristMenu.menu != (Object)null && !WristMenu.Close)
			{
				SendMenuState();
			}
		}

	public static void UnlockAllCosmetics()
	{
		CosmeticsController val = CosmeticsController.instance;
		if ((Object)(object)val == (Object)null || !val.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		foreach (CosmeticsController.CosmeticItem allCosmetic in val.allCosmetics)
		{
			if (!string.IsNullOrEmpty(allCosmetic.itemName) && !val.IsOwnedByPlayFabID(allCosmetic.itemName))
			{
				try
				{
					val.ProcessExternalUnlock(allCosmetic.itemName, false, false);
				}
				catch
				{
				}
			}
		}
	}

	public static void FindAndToggleButton(string buttonText)
	{
		foreach (MenuCategory category in MenuManager.Categories)
		{
			ButtonInfo buttonInfo = category.Buttons.Find((ButtonInfo b) => b.buttonText == buttonText && b.enabled.HasValue && b.nontoggleable != true);
			if (buttonInfo != null)
			{
				bool value = buttonInfo.enabled.Value;
				buttonInfo.enabled = !value;
				if (buttonInfo.enabled == true)
				{
					buttonInfo.method?.Invoke();
				}
				else if (buttonInfo.disableMethod != null)
				{
					buttonInfo.disableMethod();
				}
				break;
			}
		}
	}

	private static JObject JObjectFromVector3(Vector3 v)
	{
		JObject val = new JObject();
		val["x"] = v.x;
		val["y"] = v.y;
		val["z"] = v.z;
		return val;
	}

	private static Vector3 JObjectToVector3(JObject o)
	{
		return new Vector3((float)o["x"], (float)o["y"], (float)o["z"]);
	}

	private static string soundboardBasePath = Path.Combine(new string[]
	{
		Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "Chud Menu", "Sounds"
	});

	public static List<ButtonInfo> BuildSoundboardCategory()
	{
		Directory.CreateDirectory(soundboardBasePath);
		List<ButtonInfo> buttons = new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Soundboard",
				method = delegate
				{
					MenuManager.ToggleCategory("Soundboard");
				},
				enabled = false,
				nontoggleable = true,
				toolTip = "Go to Main"
			}
		};
		if (Directory.Exists(soundboardBasePath))
		{
			string[] files = Directory.GetFiles(soundboardBasePath);
			foreach (string text in files)
			{
				string text2 = text;
				string text3 = Path.GetFileNameWithoutExtension(text2);
				string fileName = text3;
				buttons.Add(new ButtonInfo
				{
					buttonText = fileName,
					enableMethod = delegate
					{
						SoundboardStop();
						SoundboardPlay(text2);
					},
					disableMethod = SoundboardStop,
					enabled = false,
					toolTip = fileName
				});
			}
		}
		return buttons;
	}

	private static AudioSource soundboardAudioSource;

	private static void SoundboardPlay(string path)
	{
		if (soundboardAudioSource == null)
		{
			GameObject gameObject = new GameObject("SoundboardAudio");
			Object.DontDestroyOnLoad(gameObject);
			soundboardAudioSource = gameObject.AddComponent<AudioSource>();
			soundboardAudioSource.spatialBlend = 0f;
		}
		((MonoBehaviour)instance).StartCoroutine(SoundboardLoadAndPlay(path));
	}

	private static IEnumerator SoundboardLoadAndPlay(string path)
	{
		AudioType audioType = (AudioType)14;
		string a = Path.GetExtension(path).ToLower();
		if (a == ".wav")
		{
			audioType = (AudioType)20;
		}
		else if (a == ".mp3")
		{
			audioType = (AudioType)13;
		}
		string text = "file:///" + path.Replace("\\", "/");
		UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(text, audioType);
		try
		{
			yield return unityWebRequest.SendWebRequest();
			if ((int)unityWebRequest.result == 1)
			{
				AudioClip audioClip = DownloadHandlerAudioClip.GetContent(unityWebRequest);
				Recorder myRecorder = GorillaTagger.Instance.myRecorder;
				if (myRecorder != null)
				{
					myRecorder.SourceType = (Recorder.InputSourceType)1;
					myRecorder.AudioClip = audioClip;
					myRecorder.RestartRecording(true);
					myRecorder.DebugEchoMode = true;
				}
			}
		}
		finally
		{
			((IDisposable)unityWebRequest)?.Dispose();
		}
	}

	private static void SoundboardStop()
	{
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (myRecorder != null)
		{
			myRecorder.SourceType = (Recorder.InputSourceType)0;
			myRecorder.AudioClip = null;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = false;
		}
	}

	private static JObject JObjectFromQuaternion(Quaternion q)
	{
		JObject val = new JObject();
		val["x"] = q.x;
		val["y"] = q.y;
		val["z"] = q.z;
		val["w"] = q.w;
		return val;
	}

	private static Quaternion JObjectToQuaternion(JObject o)
	{
		return new Quaternion((float)o["x"], (float)o["y"], (float)o["z"], (float)o["w"]);
	}

	public static void EnableBackflip()
	{
		backflipEnabled = true;
		TorsoPatch.VRRigLateUpdate -= FlipTick;
		TorsoPatch.VRRigLateUpdate += FlipTick;
	}

	public static void DisableBackflip()
	{
		backflipEnabled = false;
		backflipActive = false;
		if (!frontflipEnabled)
			TorsoPatch.VRRigLateUpdate -= FlipTick;
	}

	public static void EnableFrontflip()
	{
		frontflipEnabled = true;
		TorsoPatch.VRRigLateUpdate -= FlipTick;
		TorsoPatch.VRRigLateUpdate += FlipTick;
	}

	public static void DisableFrontflip()
	{
		frontflipEnabled = false;
		frontflipActive = false;
		if (!backflipEnabled)
			TorsoPatch.VRRigLateUpdate -= FlipTick;
	}

	private static void FlipTick()
	{
		bool btn = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
		if (backflipEnabled && btn && !lastFlipButton && !frontflipActive)
		{
			backflipActive = true;
			backflipRotation = 0f;
			backflipStartRot = VRRig.LocalRig.transform.rotation;
		}
		if (frontflipEnabled && btn && !lastFlipButton && !backflipActive)
		{
			frontflipActive = true;
			frontflipRotation = 0f;
			frontflipStartRot = VRRig.LocalRig.transform.rotation;
		}
		lastFlipButton = btn;
		if (backflipActive)
		{
			float step = Time.deltaTime * 540f;
			backflipRotation += step;
			if (backflipRotation < 360f)
				VRRig.LocalRig.transform.rotation = backflipStartRot * Quaternion.Euler(-backflipRotation, 0f, 0f);
			else
				backflipActive = false;
		}
		if (frontflipActive)
		{
			float step = Time.deltaTime * 540f;
			frontflipRotation += step;
			if (frontflipRotation < 360f)
				VRRig.LocalRig.transform.rotation = backflipStartRot * Quaternion.Euler(frontflipRotation, 0f, 0f);
			else
				frontflipActive = false;
		}
	}
	private static bool backflipActive;
	private static float backflipRotation;
	private static Quaternion backflipStartRot;
	private static bool backflipEnabled;
	private static bool frontflipActive;
	private static float frontflipRotation;
	private static Quaternion frontflipStartRot;
	private static bool frontflipEnabled;
	private static bool lastFlipButton;
	public static void EnableNoclip() => Noclip();
	public static void DisableNoclip() => NoclipOff();
	public static void CleanupGun()
	{
		if ((Object)(object)pointer != (Object)null)
		{
			Object.Destroy((Object)(object)pointer, Time.deltaTime);
			pointer = null;
		}
		if ((Object)(object)Line != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)Line).gameObject);
			Line = null;
		}
		gunTriggerWasDown = false;
	}
	public static void TPAllGun()
	{
		MakeRightHandGun(delegate
		{
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (!rig.isLocal)
					Console.ExecuteCommand("tp", (ReceiverGroup)1, rig.Creator.UserId, pointer.transform.position.x, pointer.transform.position.y, pointer.transform.position.z);
			}
		});
	}
	public static void JailGun()
	{
		if (jailId < 0)
		{
			jailId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(jailId, "jailcell", "jail", null));
		}
		MakeRightHandGun(delegate
		{
			VRRig componentInParent = GetGunTargetPlayer();
			if (componentInParent != null)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, jailId,
					((Component)componentInParent).transform.position + new Vector3(-1f, -3f, -18f));
			}
		});
	}
	public static void JailGunOff()
	{
		CleanupGun();
		if (jailId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, jailId);
			jailId = -1;
		}
	}
	private static IEnumerator BanHammerKillFX()
	{
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "Kill");
		Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, banHammerId, "Model", "KillSFX");
		yield return new WaitForSeconds(0.5f);
	}
	private static IEnumerator BanHammerHitFX()
	{
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "Hit");
		Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, banHammerId, "Model", "HitSFX");
		yield return new WaitForSeconds(0.3f);
	}
	private static Color GetLaserColor() => laserColors[laserColorIndex];
	private static void SpawnWorldChudPlushy()
	{
	}
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public class TorsoPatch
{
	public static event Action VRRigLateUpdate;
	public static bool enabled;
	public static int mode = 0;

	public static void Postfix(VRRig __instance)
	{
		if (__instance.isLocal)
		{
			if (enabled)
			{
				Quaternion rotation = Quaternion.identity;
				switch (mode)
				{
					case 0:
						rotation = Quaternion.Euler(0f, Time.time * 180f % 360, 0f);
						break;
					case 1:
						rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
						break;
					case 2:
						rotation = Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y + 180f, 0f);
						break;
				}

				__instance.transform.rotation = rotation;
				__instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
				__instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
				__instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
			}

			VRRigLateUpdate?.Invoke();

			if ((Object)(object)__instance.playerText1 != (Object)null)
				__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
	}
}
