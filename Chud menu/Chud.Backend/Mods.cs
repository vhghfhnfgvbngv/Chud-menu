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

	private const string MinosSoundDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Chud menu\\";

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
		{ "LMAYT.", "LAVA MONKE DOUGHBOI" }
	};

	private static readonly Dictionary<VRRig, GameObject> cosmeticNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static FieldInfo _ownedCosmeticsField;

	private static bool arsNameTagsActive = false;

	private static string arsLastCheckedRoom = "";

	private static bool cosmeticNotifierActive = false;

	private static HashSet<string> cosmeticNotifierNotified = new HashSet<string>();

	private static bool notificationsEnabled = true;

	public static int menuColorIndex = 0;

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


	private static bool assetPositionerEnabled = false;
	private static int positioningAssetId = -1;
	private static Vector3 grabOffsetPos;
	private static Quaternion grabOffsetRot;
	private static float lastScaleTime = 0f;


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
	private static int noliStarId = -1;
	private static int noliMusicId = -1;
	private static float noliUpdateDelay;
	private static float noliRespawnTime;
	private static bool noliHoldingTrigger;
	private static Vector3 noliThrowDirection;
	private static Vector3 noliNetworkedPos;
	private static Quaternion noliNetworkedRot;
	private static int noliStarState; // 0=Default, 1=Throwing, 2=Respawning
	private static bool noliStarEnabled;

	private static bool detectApplied = false;

	private static bool fullAutoApplied = false;

	private static bool muteRainbowSwordApplied = false;

	private static int karambitId = -1;

	private static int knifeId = -1;

	private static int rblxCarpetId = -1;

	private static int mcSwordId = -1;

	private static int banHammerId = -1;

	private static int pistolId = -1;

	private static int boomboxId = -1;

	private static int robloxSwordId = -1;

	private static int rainbowSwordId = -1;

	private static int samsungId = -1;

	private static int videoPlayerId = -1;

	private static int physicsGunId = -1;

	private static int shreksophoneId = -1;

	private static int cartiId = -1;

	private static int travisId = -1;

	private static int travisBeachId = -1;

	private static int travisCrittersId = -1;

	private static int travisCityId = -1;

	private static int kormakurId = -1;

	private static int bagId = -1;

	private static int coinId = -1;

	private static GameObject worldChudPlushy;

	private static bool worldChudPlushyLoading;

	private static int minosId;

	private static int jailId = -1;

	private static VRRig grabbedPlayer = null;

	private static bool grabUsingRight = true;

	private static bool adminGrabActive = false;


	private static bool noAdminApplied = false;

	private static bool allowKickApplied = false;

	private static bool allowTpApplied = true;

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

	private static readonly string[] laserColorNames = new string[6] { "Blue", "Red", "Purple", "Pink", "Yellow", "Gray" };

	private static int tvId = -1;

	private static Vector3 launchPlayerGunReturnPos;

	private static int launchPlayerGunFramesLeft = 0;

	private static int tagGunFramesUntilTag;

	private static Harmony vimHarmony;

	private static bool tagGunTriggerWasDown = false;

	private static float lastUntagNotif = 0f;

	private static VRRig tagGunLockedTarget = null;

	private static float lastUntagSelfTime;

	private static float tagUntaggedCooldown = 0f;

	private static Vector3 stumpPosition = new Vector3(-66.871f, 12.086f, -82.637f);

	private static bool spazAllActive = false;

	private static bool spazSelfActive = false;

	private static int spazFrameCounter = 0;

	private static bool gunTriggerWasDown = false;

	private static Camera pcGunCamera;

	public static bool blockJmanSounds = false;

	public static bool antiGuardianGrab = false;

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

	public static bool RPlat;

	public static bool LPlat;


	private static string ConfigPath => WristMenu.FolderName + "\\Config.json";

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
		AutoSave();
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
		AutoSave();
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
			Cursor.lockState = (CursorLockMode)1;
			Cursor.visible = false;
			Vector2 val4 = ((InputControl<Vector2>)(object)((Pointer)current2).delta).ReadValue() * wasdFlyMouseSense * 0.15f;
			transform2.Rotate(Vector3.up, val4.x, (Space)0);
			wasdPitch = Mathf.Clamp(wasdPitch - val4.y, -90f, 90f);
			transform.localRotation = Quaternion.Euler(wasdPitch, 0f, 0f);
		}
		else if ((int)Cursor.lockState == 1)
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
		AutoSave();
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
		AutoSave();
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
		{
			UpdateWASDFly();
		}
		if (flyActive)
		{
			UpdateFly();
		}
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
		if (!Directory.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Chud menu\\"))
		{
			Directory.CreateDirectory("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Chud menu\\");
		}
		string crushPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Chud menu\\CRUSH !.mp3";
		string slamPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Chud menu\\slam sound.mp3";
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
			spazFrameCounter++;
			if (spazFrameCounter >= 10)
			{
				spazFrameCounter = 0;
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
		if (!arsDownloaded)
		{
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

	public static void AutoSave()
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

	public static void AutoLoad()
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
			if (menuColorIndex >= 12)
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

			foreach (string enabledButton in modConfig.EnabledButtons)
			{
				foreach (MenuCategory category in MenuManager.Categories)
				{
					foreach (ButtonInfo button in category.Buttons)
					{
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
		SpawnWorldChudPlushy();
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
		AutoSave();
	}

	private static void ApplyMenuColor(int index)
	{
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
		menuColorIndex = (menuColorIndex + 1) % 12;
		ApplyMenuColor(menuColorIndex);
		WristMenu.DestroyMenu();
		WristMenu.instance.Draw();
		AutoSave();
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
					jump_right_local = GameObject.CreatePrimitive((PrimitiveType)0);
				}
				else
				{
					jump_right_local = GameObject.CreatePrimitive((PrimitiveType)3);
				}
				if (invis)
				{
					Object.Destroy((Object)(object)jump_right_local.GetComponent<Renderer>());
				}
				jump_right_local.transform.localScale = scale;
				jump_right_local.transform.position = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.RightHand.controllerTransform.position;
				jump_right_local.transform.rotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
				jump_right_local.AddComponent<GorillaSurfaceOverride>().overrideIndex = jump_right_local.GetComponent<GorillaSurfaceOverride>().overrideIndex;
				once_right = true;
				once_right_false = false;
				jump_right_local.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
				if (NetworkMenuEnabled)
				{
					SendPlatformSpawn(jump_right_local.transform.position, jump_right_local.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "R");
				}
			}
		}
		else if (!once_right_false && (Object)(object)jump_right_local != (Object)null)
		{
			if (NetworkMenuEnabled)
			{
				SendPlatformDestroy("R");
			}
			Object.Destroy((Object)(object)jump_right_local);
			jump_right_local = null;
			once_right = false;
			once_right_false = true;
		}
		if (LPlat)
		{
			if (!once_left && (Object)(object)jump_left_local == (Object)null)
			{
				if (sticky)
				{
					jump_left_local = GameObject.CreatePrimitive((PrimitiveType)0);
				}
				else
				{
					jump_left_local = GameObject.CreatePrimitive((PrimitiveType)3);
				}
				if (invis)
				{
					Object.Destroy((Object)(object)jump_left_local.GetComponent<Renderer>());
				}
				jump_left_local.transform.localScale = scale;
				jump_left_local.transform.position = new Vector3(0f, -0.01f, 0f) + GTPlayer.Instance.LeftHand.controllerTransform.position;
				jump_left_local.transform.rotation = GTPlayer.Instance.LeftHand.controllerTransform.rotation;
				jump_left_local.AddComponent<GorillaSurfaceOverride>().overrideIndex = jump_left_local.GetComponent<GorillaSurfaceOverride>().overrideIndex;
				once_left = true;
				once_left_false = false;
				jump_left_local.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
				if (NetworkMenuEnabled)
				{
					SendPlatformSpawn(jump_left_local.transform.position, jump_left_local.transform.rotation, scale, WristMenu.ButtonColorEnabled, invis, sticky, "L");
				}
			}
		}
		else if (!once_left_false && (Object)(object)jump_left_local != (Object)null)
		{
			if (NetworkMenuEnabled)
			{
				SendPlatformDestroy("L");
			}
			Object.Destroy((Object)(object)jump_left_local);
			jump_left_local = null;
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
		MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				Console.ExecuteCommand("kick", (ReceiverGroup)1, componentInParent.Creator.UserId);
			}
		}, delegate
		{
		});
	}

	public static void SilentKickGun()
	{
		MakeGun(new Color(0.5f, 0f, 0f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				Console.ExecuteCommand("silkick", (ReceiverGroup)1, componentInParent.Creator.UserId);
			}
		}, delegate
		{
		});
	}

	public static void TPGun()
	{
		MakeGun(Color.cyan, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			Console.TeleportPlayer(pointer.transform.position);
		}, delegate
		{
		});
	}

	public static void FlingGun()
	{
		MakeGun(Color.yellow, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				Player playerFromID = Console.GetPlayerFromID(componentInParent.Creator.UserId);
				if (playerFromID != null)
				{
					flingTargetActor = playerFromID.ActorNumber;
					if (flingGunCoroutine != null)
					{
						((MonoBehaviour)instance).StopCoroutine(flingGunCoroutine);
					}
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
		MakeGun(Color.cyan, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			Console.ExecuteCommand("strike", (ReceiverGroup)1, pointer.transform.position);
		}, delegate
		{
		});
	}

	public static void VibrateGun()
	{
		MakeGun(Color.magenta, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				Player playerFromID = Console.GetPlayerFromID(componentInParent.Creator.UserId);
				if (playerFromID != null)
				{
					Console.ExecuteCommand("vibrate", playerFromID.ActorNumber, 3, 5f);
				}
			}
		}, delegate
		{
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
		UpdateAssetPositioner();
	}

	public static void ToggleAssetPositioner()
	{
		assetPositionerEnabled = !assetPositionerEnabled;
		if (!assetPositionerEnabled)
		{
			positioningAssetId = -1;
		}
	}

	public static void DisableAssetPositioner()
	{
		assetPositionerEnabled = false;
		positioningAssetId = -1;
	}

	private static void UpdateAssetPositioner()
	{
		if (!assetPositionerEnabled)
		{
			return;
		}
		bool leftGrab = ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
		Transform leftHandTransform = LeftHandTransform;
		if (leftGrab && positioningAssetId < 0)
		{
			float num = float.MaxValue;
			int num2 = -1;
			foreach (KeyValuePair<int, Console.ConsoleAsset> consoleAsset in Console.ConsoleAssets)
			{
				if (!((Object)(object)consoleAsset.Value.obj == (Object)null))
				{
					float num3 = Vector3.Distance(leftHandTransform.position, consoleAsset.Value.obj.transform.position);
					if (num3 < num)
					{
						num = num3;
						num2 = consoleAsset.Key;
					}
				}
			}
			if (num2 >= 0)
			{
				positioningAssetId = num2;
				GameObject obj = Console.ConsoleAssets[num2].obj;
				grabOffsetPos = obj.transform.position - leftHandTransform.position;
				grabOffsetRot = Quaternion.Inverse(leftHandTransform.rotation) * obj.transform.rotation;
			}
		}
		else if (leftGrab && positioningAssetId >= 0)
		{
			if (Console.ConsoleAssets.TryGetValue(positioningAssetId, out var value) && (Object)(object)value.obj != (Object)null)
			{
				value.obj.transform.position = leftHandTransform.position + grabOffsetPos;
				value.obj.transform.rotation = leftHandTransform.rotation * grabOffsetRot;
				float leftControllerIndexFloat = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat;
				float rightControllerIndexFloat = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat;
				float num4 = rightControllerIndexFloat - leftControllerIndexFloat;
				if (Mathf.Abs(num4) > 0.1f && Time.time > lastScaleTime + 0.08f)
				{
					float num5 = 1f + num4 * 0.03f;
					Vector3 val = value.obj.transform.localScale * num5;
					value.obj.transform.localScale = val;
					Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, positioningAssetId, val);
					lastScaleTime = Time.time;
				}
			}
		}
		else
		{
			if (leftGrab || positioningAssetId < 0)
			{
				return;
			}
			if (Console.ConsoleAssets.TryGetValue(positioningAssetId, out var value2) && (Object)(object)value2.obj != (Object)null)
			{
				Vector3 localPosition = value2.obj.transform.localPosition;
				Quaternion localRotation = value2.obj.transform.localRotation;
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, positioningAssetId, localPosition);
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, positioningAssetId, localRotation);
				try
				{
					string folderName = WristMenu.FolderName;
					if (!Directory.Exists(folderName))
					{
						Directory.CreateDirectory(folderName);
					}
					Vector3 localScale = value2.obj.transform.localScale;
					string text = string.Join("_", value2.assetName.Split(Path.GetInvalidFileNameChars()));
					string path = folderName + "\\AssetPosition_" + text + ".txt";
					File.WriteAllText(path, "// " + value2.assetName + " position data\nlocalPosition: " + localPosition.x + " " + localPosition.y + " " + localPosition.z + "\nlocalRotation: " + localRotation.eulerAngles.x + " " + localRotation.eulerAngles.y + " " + localRotation.eulerAngles.z + "\nlocalScale: " + localScale.x + " " + localScale.y + " " + localScale.z + "\n// C#: new Vector3(" + localPosition.x + "f, " + localPosition.y + "f, " + localPosition.z + "f)\n// C#: Quaternion.Euler(" + localRotation.eulerAngles.x + "f, " + localRotation.eulerAngles.y + "f, " + localRotation.eulerAngles.z + "f)\n// C#: new Vector3(" + localScale.x + "f, " + localScale.y + "f, " + localScale.z + "f)");
				}
				catch
				{
				}
			}
			positioningAssetId = -1;
		}
	}

	private static IEnumerator BanHammerHitFX()
	{
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "Default");
		yield return null;
		yield return null;
		Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, banHammerId, "Model/SwingSFX", "HammerHit");
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "HitGround");
		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.transform.position) < 2f)
				Console.ExecuteCommand("vel", rig.Creator.ActorNumber, (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 5f);
		}
	}

	private static IEnumerator BanHammerKillFX()
	{
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "Default");
		yield return null;
		yield return null;
		Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, banHammerId, "Model/KillSFX", "HammerKill");
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, banHammerId, "Model", "HitPlayer");
	}

	public static void NoliStarToggle()
	{
		noliStarEnabled = !noliStarEnabled;
		if (noliStarEnabled)
		{
			noliStarId = -1;
			noliStarState = 0;
			noliHoldingTrigger = false;
		}
		else
		{
			DisableNoliStar();
		}
	}

	public static void NoliStarUpdate()
	{
		if (noliStarId < 0)
		{
			noliStarId = Console.GetFreeAssetID();
			Console.ExecuteCommand("asset-spawn", (ReceiverGroup)1, "console.main1", "Star", noliStarId);
			Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, noliStarId, "Model", "StarSpawn");
		}
		if (!Console.ConsoleAssets.TryGetValue(noliStarId, out var starAsset) || starAsset.obj == null)
			return;
		GameObject starObj = starAsset.obj;
		ControllerInputPoller poller = (ControllerInputPoller)ControllerInputPoller.instance;
		float noliTrigger = poller.rightControllerIndexFloat;
		if (noliTrigger > 0.5f && noliStarState == 0)
		{
			Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
			GameObject noliCrosshair = GameObject.CreatePrimitive((PrimitiveType)0);
			noliCrosshair.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			noliCrosshair.transform.position = (noliRay.point == Vector3.zero) ? (noliRay.transform.position + noliRay.transform.forward * 20f) : noliRay.point;
			noliCrosshair.GetComponent<Renderer>().material.color = Color.white;
			Object.Destroy(noliCrosshair, Time.deltaTime);
			Object.Destroy(noliCrosshair.GetComponent<Collider>());
		}
		if (noliTrigger < 0.5f && noliHoldingTrigger && noliStarState == 0)
		{
			noliStarState = 1;
			Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, noliStarId, "Model", "Throw");
			Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, noliStarId, "Model", "ThrowStar");
			Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliDirRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
			noliThrowDirection = (noliDirRay.point - starObj.transform.position).normalized;
		}
		noliHoldingTrigger = noliTrigger > 0.5f;
		switch (noliStarState)
		{
		case 0:
			starObj.transform.position = GorillaTagger.Instance.rightHandTransform.position + Vector3.up * 0.2f;
			starObj.transform.rotation = Quaternion.Euler(Time.time * 32f, Time.time * 10f, Time.time * 47f);
			break;
		case 1:
		{
			Physics.Raycast(starObj.transform.position, noliThrowDirection, out RaycastHit noliHitRay, 0.5f, GTPlayer.Instance.locomotionEnabledLayers);
			if (noliHitRay.point == Vector3.zero)
			{
				starObj.transform.position += noliThrowDirection * (Time.deltaTime * 15f);
				starObj.transform.rotation = Quaternion.Euler(Time.time * 239f, Time.time * 201f, Time.time * 170f);
			}
			else
			{
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, noliStarId, "Model", "Explode");
				bool noliKill = false;
				foreach (VRRig nRig in VRRigCache.ActiveRigs)
				{
					if (!nRig.isLocal && Vector3.Distance(starObj.transform.position, nRig.transform.position) < 2.32775f)
					{
						Console.ExecuteCommand("silkick", (ReceiverGroup)1, nRig.Creator.UserId);
						noliKill = true;
					}
				}
				Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, noliStarId, "Model", noliKill ? "KillStar" : "BreakStar");
				noliStarState = 2;
				noliRespawnTime = Time.time + 3f;
			}
			break;
		}
		case 2:
			if (Time.time > noliRespawnTime)
			{
				Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)1, noliStarId, "Model", "Default");
				Console.ExecuteCommand("asset-playsound", (ReceiverGroup)1, noliStarId, "Model", "StarSpawn");
				noliStarState = 0;
			}
			break;
		}
		if (Time.time > noliUpdateDelay && (noliNetworkedRot != starObj.transform.rotation || noliNetworkedPos != starObj.transform.position))
		{
			noliUpdateDelay = Time.time + 0.05f;
			noliNetworkedPos = starObj.transform.position;
			noliNetworkedRot = starObj.transform.rotation;
			Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, noliStarId, starObj.transform.position);
			Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, noliStarId, starObj.transform.rotation);
		}
	}

	public static void DisableNoliStar()
	{
		noliStarEnabled = false;
		if (noliStarId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, noliStarId);
			noliStarId = -1;
		}
		if (noliMusicId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, noliMusicId);
			noliMusicId = -1;
		}
		noliStarState = 0;
		noliHoldingTrigger = false;
		noliUpdateDelay = 0f;
		noliRespawnTime = 0f;
	}

	public static void DetectConsoleUsers()
	{
		if (!detectApplied)
		{
			detectApplied = true;
			Console.autoDetectConsoleUsers = true;
			if (PhotonNetwork.InRoom)
			{
				Console.indicatorDelay = Time.time + 5f;
				Console.ScheduleConsoleUserScan();
			}
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Console User Detection: ON");
		}
	}

	public static void DisableDetectConsoleUsers()
	{
		if (detectApplied)
		{
			Console.autoDetectConsoleUsers = false;
			Console.ClearConsoleUserIndicators();
			Console.userDictionary.Clear();
			detectApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Console User Detection: OFF");
		}
	}

	public static void ToggleFullAutoPistol()
	{
		if (!fullAutoApplied)
		{
			fullAutoApplied = true;
			Console.fullAutoPistol = true;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Full Auto Pistol: ON");
		}
	}

	public static void DisableFullAutoPistol()
	{
		if (fullAutoApplied)
		{
			Console.fullAutoPistol = false;
			fullAutoApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Full Auto Pistol: OFF");
		}
	}

	public static void ToggleMuteRainbowSword()
	{
		if (!muteRainbowSwordApplied)
		{
			muteRainbowSwordApplied = true;
			Console.muteRainbowSword = true;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Mute Rainbow Sword: ON");
		}
	}

	public static void DisableMuteRainbowSword()
	{
		if (muteRainbowSwordApplied)
		{
			Console.muteRainbowSword = false;
			muteRainbowSwordApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Mute Rainbow Sword: OFF");
		}
	}

	public static void SpawnKarambit()
	{
		if (karambitId < 0)
		{
			karambitId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(karambitId, "karambit", "karambit", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.045f, 0.065f, 0f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(270f, 60f, 0f));
			}));
		}
	}

	public static void SpawnKnife()
	{
		if (knifeId < 0)
		{
			knifeId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(knifeId, "knife", "knife", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.02866926f, 0.0961746f, 0.1409995f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(79.12813f, 337.5215f, 347.2383f));
			}));
		}
	}

	public static void SpawnRblxCarpet()
	{
		if (rblxCarpetId < 0)
		{
			rblxCarpetId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(rblxCarpetId, "rblxcarpet", "robloxrainbowcarpet", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.2574666f, -0.007336602f, 0.1125555f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(1.562481f, 359.7548f, 155.0262f));
			}));
		}
	}

	public static void SpawnMcSword()
	{
		if (mcSwordId >= 0)
		{
			return;
		}
		mcSwordId = Console.GetFreeAssetID();
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(mcSwordId, "mcsword", "Sword", delegate(int id)
		{
			Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.03233476f, 0.0433403f, -0.08071579f));
			Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(302.1735f, 351.6904f, 280.6184f));
			Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, (object)new Vector3(0.01450266f, 0.01450266f, 0.01450266f));
			if (Console.ConsoleAssets.TryGetValue(id, out var value) && (Object)(object)value.obj != (Object)null)
			{
				Transform val = value.obj.transform.Find("Music");
				if ((Object)(object)val != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)val).gameObject);
				}
			}
			Console.ExecuteCommand("asset-setsound", (ReceiverGroup)1, id, "Music", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
		}));
	}

	public static void SpawnBanHammer()
	{
		if (banHammerId < 0)
		{
			banHammerId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(banHammerId, "banhammer", "BanHammer", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}, addSurfaceOverride: true));
		}
	}

	public static void SpawnPistol()
	{
		if (pistolId < 0)
		{
			pistolId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(pistolId, "console.main1", "Pistol", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
		}
	}

	public static void SpawnBoombox()
	{
		if (boomboxId >= 0)
		{
			return;
		}
		boomboxId = Console.GetFreeAssetID();
		string clipboardUrl = GUIUtility.systemCopyBuffer;
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(boomboxId, "console.main1", "Boombox", delegate(int id)
		{
			Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
			Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0f, 0f, 0.15f));
			Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(0f, 90f, 90f));
			if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
			{
				Console.ExecuteCommand("asset-setsound", (ReceiverGroup)1, id, "Model", clipboardUrl);
			}
		}));
	}

	public static void SpawnRobloxSword()
	{
		if (robloxSwordId < 0)
		{
			robloxSwordId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(robloxSwordId, "console.main1", "Sword", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
		}
	}

	public static void SpawnRainbowSword()
	{
		if (rainbowSwordId >= 0)
		{
			return;
		}
		rainbowSwordId = Console.GetFreeAssetID();
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(rainbowSwordId, "rbsword", "Sword", delegate(int id)
		{
			Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			if (Console.muteRainbowSword)
			{
				Console.ExecuteCommand("asset-stopsound", (ReceiverGroup)1, id, "Sword");
				Console.ExecuteCommand("asset-setsound", (ReceiverGroup)1, id, "Sword", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
			}
		}, addSurfaceOverride: true));
	}

	public static void SpawnSamsung()
	{
		if (samsungId >= 0)
		{
			return;
		}
		samsungId = Console.GetFreeAssetID();
		string clipboardUrl = GUIUtility.systemCopyBuffer;
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(samsungId, "consolehamburburassets", "samsungphone", delegate(int id)
		{
			Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
			Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(-0.075f, 0.1f, 0f));
			Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(80f, 90f, 180f));
			Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one * 0.3f);
			Console.ExecuteCommand("asset-destroycolliders", (ReceiverGroup)1, id);
			if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
			{
				Console.ExecuteCommand("asset-setvideo", (ReceiverGroup)1, id, "VideoPlayer", clipboardUrl);
			}
		}));
	}

	public static void SpawnVideoPlayer()
	{
		if (videoPlayerId >= 0)
		{
			return;
		}
		videoPlayerId = Console.GetFreeAssetID();
		string clipboardUrl = GUIUtility.systemCopyBuffer;
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(videoPlayerId, "console.main1", "VideoPlayer", delegate(int id)
		{
			Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
			Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0f, 0.04f, 0.12f));
			Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one * 0.05f);
			if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
			{
				Console.ExecuteCommand("asset-setvideo", (ReceiverGroup)1, id, "VideoPlayer", clipboardUrl);
			}
		}));
	}

	public static void SpawnPhysicsGun()
	{
		if (physicsGunId < 0)
		{
			physicsGunId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(physicsGunId, "console.main1", "PhysicsGun", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
		}
	}

	public static void SpawnShreksophone()
	{
		if (shreksophoneId < 0)
		{
			shreksophoneId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(shreksophoneId, "consolehamburburassets", "shrek", delegate(int id)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, id, (object)new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, id, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one * 5f);
			}));
		}
	}

	public static void SpawnCarti()
	{
		if (cartiId < 0)
		{
			cartiId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(cartiId, "consolehamburburassets", "carti", delegate(int id)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, id, (object)new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, id, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one * 5f);
			}));
		}
	}

	public static void SpawnTravis()
	{
		if (travisId < 0)
		{
			travisId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(travisId, "travis", "travisscott", delegate(int id)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, id, (object)new Vector3(-70f, 2f, -52f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one * 0.38f);
			}));
		}
	}

	public static void SpawnTravisBeach()
	{
		if (travisBeachId < 0)
		{
			travisBeachId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(travisBeachId, "travis", "travisscott", delegate(int id)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(16.38702f, 12.29928f, 23.63119f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(352.4303f, 49.92272f, 0.8915782f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, (object)new Vector3(0.38f, 0.38f, 0.38f));
			}));
		}
	}

	public static void DisableSpawnTravisBeach()
	{
		DestroyAsset(ref travisBeachId);
	}

	public static void SpawnTravisCritters()
	{
		if (travisCrittersId < 0)
		{
			travisCrittersId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(travisCrittersId, "travis", "travisscott", delegate(int id)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(229.5867f, -98.26467f, 178.8833f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(4.141929f, 52.20211f, 2.67847f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, (object)new Vector3(1.784783f, 1.784783f, 1.784783f));
			}));
		}
	}

	public static void DisableSpawnTravisCritters()
	{
		DestroyAsset(ref travisCrittersId);
	}

	public static void SpawnTravisCity()
	{
		if (travisCityId < 0)
		{
			travisCityId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(travisCityId, "travis", "travisscott", delegate(int id)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(-52.68209f, 16.36728f, -118.7615f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(0.9019919f, 345.8464f, 1.200598f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, (object)new Vector3(0.02183428f, 0.02183428f, 0.02183428f));
			}));
		}
	}

	public static void DisableSpawnTravisCity()
	{
		DestroyAsset(ref travisCityId);
	}

	public static void SpawnKormakur()
	{
		if (kormakurId < 0)
		{
			kormakurId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(kormakurId, "consolehamburburassets", "KormakurSign", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.29f, -0.2f, -0.1272f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(355f, 275f, 265f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, Vector3.one);
			}));
		}
	}

	public static void SpawnBag()
	{
		if (bagId < 0)
		{
			bagId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(bagId, "consolehamburburassets", "bag", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, id, (object)new Vector3(0.1427352f, 0.08271359f, 0.06961101f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, id, Quaternion.Euler(355.0145f, 350.4344f, 162.7124f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, id, (object)new Vector3(9.717054f, 9.717054f, 9.717054f));
			}));
		}
	}

	public static void SpawnCoin()
	{
		if (coinId < 0)
		{
			coinId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(coinId, "console.main1", "Coin", delegate(int id)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
		}
	}

	public static void SpawnWorldChudPlushy()
	{
		if (!((Object)(object)worldChudPlushy != (Object)null) && !worldChudPlushyLoading)
		{
			worldChudPlushyLoading = true;
			((MonoBehaviour)Console.instance).StartCoroutine(DoSpawnWorldChudPlushy());
		}
	}

	private static IEnumerator DoSpawnWorldChudPlushy()
	{
		UnityWebRequest req = UnityWebRequest.Get("https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/refs/heads/main/chud");
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result != 1)
			{
				worldChudPlushyLoading = false;
				yield break;
			}
			AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
			yield return bundleReq;
			if ((Object)(object)bundleReq.assetBundle == (Object)null)
			{
				worldChudPlushyLoading = false;
				yield break;
			}
			AssetBundleRequest assetReq = bundleReq.assetBundle.LoadAssetAsync<GameObject>("assets/bundleparent (put objects in here dont move).prefab");
			yield return assetReq;
			if (assetReq.asset == (Object)null)
			{
				worldChudPlushyLoading = false;
				yield break;
			}
			worldChudPlushy = Object.Instantiate<GameObject>((GameObject)assetReq.asset);
			Transform chud = worldChudPlushy.transform.Find("chud");
			if ((Object)(object)chud != (Object)null)
			{
				chud.localPosition = Vector3.zero;
			}
			worldChudPlushy.transform.position = new Vector3(-65.41852f, 11.94344f, -79.92567f);
			worldChudPlushy.transform.rotation = Quaternion.Euler(350.3598f, 199.196f, 0.9686815f);
			worldChudPlushy.transform.localScale = Vector3.one * 0.6630982f;
			Object.DontDestroyOnLoad((Object)(object)worldChudPlushy);
			worldChudPlushyLoading = false;
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static void Spawnminosprime()
	{
		minosId = Console.GetFreeAssetID();
		Console.ExecuteCommand("asset-spawn", (ReceiverGroup)1, "minosprime", "minosprime", minosId);
		Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, minosId, 2);
		Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, minosId, (object)new Vector3(0.06263994f, 0.05301395f, -0.04137805f));
		Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, minosId, Quaternion.Euler(286.3085f, 201.7456f, 347.1011f));
		Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, minosId, Vector3.one * 0.3518889f);
	}

	public static void Delminosprime()
	{
		Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, minosId);
	}

	public static void JailGun()
	{
		if (jailId < 0)
		{
			jailId = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(jailId, "jailcell", "jail", null));
		}
		MakeGun(new Color(0.3f, 0.3f, 0.3f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, jailId, ((Component)componentInParent).transform.position + new Vector3(-1f, -3f, -18f));
			}
		}, delegate
		{
		});
	}

	public static void JailGunOff()
	{
		if (jailId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, jailId);
			jailId = -1;
		}
	}

	public static void AdminGrab()
	{
		if (!adminGrabActive)
		{
			adminGrabActive = true;
		}
	}

	public static void AdminGrabOff()
	{
		if (adminGrabActive)
		{
			adminGrabActive = false;
			grabbedPlayer = null;
		}
	}

	public static void UpdateAdminGrab()
	{
		if (!adminGrabActive)
		{
			return;
		}
		bool flag = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
		bool flag2 = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
		if (flag || flag2)
		{
			if ((Object)(object)grabbedPlayer == (Object)null)
			{
				Transform val = (flag ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform);
				grabUsingRight = flag;
				VRRig val2 = null;
				float num = 2f;
				foreach (VRRig activeRig in VRRigCache.ActiveRigs)
				{
					if (!((Object)(object)activeRig == (Object)null) && !activeRig.isLocal)
					{
						float num2 = Vector3.Distance(val.position, ((Component)activeRig).transform.position);
						if (num2 < num)
						{
							num = num2;
							val2 = activeRig;
						}
					}
				}
				grabbedPlayer = val2;
			}
			if ((Object)(object)grabbedPlayer != (Object)null && grabbedPlayer.Creator != null)
			{
				Transform val3 = (grabUsingRight ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform);
				Console.ExecuteCommand("tp", grabbedPlayer.Creator.ActorNumber, val3.position + new Vector3(0f, 0.5f, 0f));
			}
		}
		else
		{
			grabbedPlayer = null;
		}
	}

	public static void CleanupGun()
	{
		if ((Object)(object)pointer != (Object)null)
		{
			Object.Destroy((Object)(object)pointer);
			pointer = null;
		}
		if ((Object)(object)Line != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)Line).gameObject);
			Line = null;
		}
		gunTriggerWasDown = false;
	}

	public static void DisableSpawnKarambit()
	{
		DestroyAsset(ref karambitId);
	}

	public static void DisableSpawnKnife()
	{
		DestroyAsset(ref knifeId);
	}

	public static void DisableSpawnRblxCarpet()
	{
		DestroyAsset(ref rblxCarpetId);
	}

	public static void DisableSpawnMcSword()
	{
		DestroyAsset(ref mcSwordId);
	}

	public static void DisableSpawnBanHammer()
	{
		DestroyAsset(ref banHammerId);
	}

	public static void DisableSpawnRobloxSword()
	{
		DestroyAsset(ref robloxSwordId);
	}

	public static void DisableSpawnRainbowSword()
	{
		DestroyAsset(ref rainbowSwordId);
	}

	public static void DisableSpawnPistol()
	{
		DestroyAsset(ref pistolId);
	}

	public static void DisableSpawnPhysicsGun()
	{
		DestroyAsset(ref physicsGunId);
	}

	public static void DisableSpawnBag()
	{
		DestroyAsset(ref bagId);
	}

	public static void DisableSpawnKormakur()
	{
		DestroyAsset(ref kormakurId);
	}

	public static void DisableSpawnCoin()
	{
		DestroyAsset(ref coinId);
	}

	public static void DisableSpawnBoombox()
	{
		DestroyAsset(ref boomboxId);
	}

	public static void DisableSpawnSamsung()
	{
		DestroyAsset(ref samsungId);
	}

	public static void DisableSpawnVideoPlayer()
	{
		DestroyAsset(ref videoPlayerId);
	}

	public static void DisableSpawnTV()
	{
		DestroyAsset(ref tvId);
	}

	public static void DisableSpawnTravis()
	{
		DestroyAsset(ref travisId);
	}

	public static void DisableSpawnShreksophone()
	{
		DestroyAsset(ref shreksophoneId);
	}

	public static void DisableSpawnCarti()
	{
		DestroyAsset(ref cartiId);
	}

	private static void DestroyAsset(ref int id)
	{
		if (id >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, id);
			id = -1;
		}
	}

	public static void DestroyAllAssets()
	{
		HashSet<int> hashSet = new HashSet<int>
		{
			karambitId, banHammerId, pistolId, boomboxId, tvId, robloxSwordId, rainbowSwordId, samsungId, videoPlayerId, physicsGunId,
			shreksophoneId, cartiId, travisId, travisBeachId, travisCrittersId, travisCityId, kormakurId, bagId, coinId, jailId
		};
		hashSet.RemoveWhere((int id) => id < 0);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Console.ConsoleAsset> consoleAsset in Console.ConsoleAssets)
		{
			if (!hashSet.Contains(consoleAsset.Key))
			{
				list.Add(consoleAsset.Key);
			}
		}
		foreach (int item in list)
		{
			if (Console.ConsoleAssets.TryGetValue(item, out var value))
			{
				value.DestroyObject();
				Console.ConsoleAssets.Remove(item);
			}
		}
	}

	public static void TPAllGun()
	{
		MakeGun(new Color(0f, 1f, 1f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			Console.ExecuteCommand("tp", (ReceiverGroup)0, pointer.transform.position);
		}, delegate
		{
		});
	}

	public static void ToggleNoAdminIndicator()
	{
		if (!noAdminApplied)
		{
			noAdminApplied = true;
			Console.ExecuteCommand("nocone", (ReceiverGroup)1, true);
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Admin indicator: HIDDEN");
		}
	}

	public static void DisableNoAdminIndicator()
	{
		if (noAdminApplied)
		{
			Console.ExecuteCommand("nocone", (ReceiverGroup)1, false);
			noAdminApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Admin indicator: VISIBLE");
		}
	}

	public static void ToggleAllowKickSelf()
	{
		if (!allowKickApplied)
		{
			allowKickApplied = true;
			Console.allowKickSelf = true;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow admins to affect you: ON");
		}
	}

	public static void DisableAllowKickSelf()
	{
		if (allowKickApplied)
		{
			Console.allowKickSelf = false;
			allowKickApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow admins to affect you: OFF");
		}
	}

	public static void ToggleAllowTpSelf()
	{
		if (!allowTpApplied)
		{
			allowTpApplied = true;
			Console.allowTpSelf = true;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow teleport self: ON");
		}
	}

	public static void DisableAllowTpSelf()
	{
		if (allowTpApplied)
		{
			Console.allowTpSelf = false;
			allowTpApplied = false;
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow teleport self: OFF");
		}
	}

	public static void NotifyPresence()
	{
		string text = ((PhotonNetwork.LocalPlayer != null) ? PhotonNetwork.LocalPlayer.NickName : "Admin");
		Console.ExecuteCommand("notify", (ReceiverGroup)1, "Admin " + text + " is in the lobby!");
	}

	private static Color GetLaserColor()
	{
		return laserColors[laserColorIndex];
	}

	public static void CycleLaserColor()
	{
		laserColorIndex = (laserColorIndex + 1) % laserColors.Length;
		Color laserColor = GetLaserColor();
		Console.ExecuteCommand("laserColor", (ReceiverGroup)1, laserColor.r, laserColor.g, laserColor.b);
		NotifiLib.SendNotification("[<color=red>ADMIN</color>] Laser color: " + laserColorNames[laserColorIndex]);
		AutoSave();
	}

	public static void SpawnTV()
	{
		if (tvId >= 0)
		{
			return;
		}
		tvId = Console.GetFreeAssetID();
		string url = GUIUtility.systemCopyBuffer;
		((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(tvId, "consolehamburburassets", "TV", delegate(int id)
		{
			Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, id, (object)new Vector3(-57.1f, 5.6f, -37f));
			Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, id, Quaternion.Euler(270f, 0f, 0f));
			if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
			{
				Console.ExecuteCommand("asset-setvideo", (ReceiverGroup)1, id, "VideoPlayer", url);
			}
		}));
	}

	public static void JoinCode(string code)
	{
		NotifiLib.SendNotification("[<color=green>FUN</color>] Joining room: " + code);
		NetworkSystem.Instance.ReturnToSinglePlayer();
		((MonoBehaviour)instance).StartCoroutine(Console.JoinRoom(code));
	}

	public static void GetPlayerIDGun()
	{
		MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				string text = (GUIUtility.systemCopyBuffer = componentInParent.Creator.UserId);
				NotifiLib.SendNotification("[<color=green>PLAYER ID</color>] Copied: " + text);
			}
		}, delegate
		{
		});
	}

	public static void LaunchPlayerGun()
	{
		MakeGun(Color.green, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			launchPlayerGunReturnPos = ((Component)VRRig.LocalRig).transform.position;
			((Component)VRRig.LocalRig).transform.position = pointer.transform.position;
			launchPlayerGunFramesLeft = 10;
		}, delegate
		{
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
			if ((Object)(object)tagGunLockedTarget != (Object)null)
			{
				tagGunLockedTarget = null;
				((Behaviour)VRRig.LocalRig).enabled = true;
			}
			CleanupGun();
		}
		else
		{
			tagGunTriggerWasDown = WristMenu.triggerDownR;
			MakeGun(WristMenu.ButtonColorEnabled, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
			{
				Collider collider = raycastHit.collider;
				VRRig val3 = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
				if ((Object)(object)val3 != (Object)null && !val3.isLocal && val3.Creator != null)
				{
					GorillaGameManager val4 = GorillaGameManager.instance;
					GorillaTagManager val5 = (GorillaTagManager)(object)(((Object)(object)val4 != (Object)null) ? ((val4 is GorillaTagManager) ? val4 : null) : null);
					if ((Object)(object)val5 != (Object)null && !val5.IsInfected(val3.Creator))
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
			}, delegate
			{
			});
			if ((Object)(object)tagGunLockedTarget != (Object)null && (Object)(object)pointer != (Object)null && (Object)(object)Line != (Object)null)
			{
				pointer.transform.position = ((Component)tagGunLockedTarget).transform.position;
				Line.SetPosition(1, ((Component)tagGunLockedTarget).transform.position);
			}
		}
		GorillaGameManager val = GorillaGameManager.instance;
		GorillaTagManager val2 = (GorillaTagManager)(object)(((Object)(object)val != (Object)null) ? ((val is GorillaTagManager) ? val : null) : null);
		if ((Object)(object)val2 == (Object)null || !((Object)(object)tagGunLockedTarget != (Object)null))
		{
			return;
		}
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

	public static void UntagGun()
	{
		MakeGun(Color.green, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig rig = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)rig != (Object)null && rig.Creator != null)
			{
				GorillaGameManager val = GorillaGameManager.instance;
				if ((Object)(object)val != (Object)null)
				{
					GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
					if (val2 != null && val2.IsInfected(rig.Creator) && Time.time > lastUntagNotif)
					{
						val2.currentInfected.RemoveAll((NetPlayer p) => p.UserId == rig.Creator.UserId);
						lastUntagNotif = Time.time + 0.3f;
						NotifiLib.SendNotification("[<color=green>MASTER</color>] Untagged " + rig.Creator.NickName);
					}
				}
			}
		}, delegate
		{
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
		MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, (PrimitiveType)0, GTPlayer.Instance.RightHand.controllerTransform, liner: true, delegate
		{
			VRRig componentInParent = ((Component)raycastHit.collider).GetComponentInParent<VRRig>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.Creator != null)
			{
				try
				{
					foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
					{
						if (allScoreboardLine.linePlayer != null && allScoreboardLine.linePlayer.UserId == componentInParent.Creator.UserId)
						{
							allScoreboardLine.muteButton.isOn = !allScoreboardLine.muteButton.isOn;
							allScoreboardLine.PressButton(allScoreboardLine.muteButton.isOn, (GorillaPlayerLineButton.ButtonType)3);
						}
					}
				}
				catch
				{
				}
			}
		}, delegate
		{
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

	public static void ReceiveRemotePlatformSpawn(int senderActor, string hand, Vector3 pos, Quaternion rot, Vector3 scale, Color color, bool invis, bool sticky)
	{
		if (!NetworkMenuEnabled)
		{
			return;
		}
		string key = senderActor + "_" + hand;
		if (!remotePlatforms.ContainsKey(key))
		{
			GameObject val = GameObject.CreatePrimitive((PrimitiveType)((!sticky) ? 3 : 0));
			Object.Destroy((Object)(object)val.GetComponent<Rigidbody>());
			val.transform.position = pos;
			val.transform.rotation = rot;
			val.transform.localScale = scale;
			if (invis)
			{
				Object.Destroy((Object)(object)val.GetComponent<Renderer>());
			}
			else
			{
				val.GetComponent<Renderer>().material.color = color;
			}
			val.AddComponent<GorillaSurfaceOverride>();
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

}
