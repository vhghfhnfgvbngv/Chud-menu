using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Photon.Voice.Unity;
using Valve.Newtonsoft.Json.Linq;
using TMPro;

namespace MalachiTemp.Backend
{
    public class ConsoleIntegration : MonoBehaviour
    {
        #region Configuration

        public static ConsoleIntegration instance;

        public const string ConsoleVersion = "3.0.8";
        public const string MenuName = "Chud Menu";
        public const byte ConsoleByte = 68;
        public const string BlockedKey = "ConsoleBlocked";

        public static readonly string ConsoleResourceLocation = "Console";



        public static void TeleportPlayer(Vector3 position)
        {
            GTPlayer.Instance.TeleportTo(World2Player(position), GTPlayer.Instance.transform.rotation, true);
            VRRig.LocalRig.transform.position = position;
        }

        public static void EnableMod(string mod, bool enable) { }
        public static void ToggleMod(string mod) { }
        public static void ConfirmUsing(string id, string version, string menuName)
        {
            NotifiLib.SendNotification("[<color=purple>CONSOLE</color>] " + id + " uses " + menuName + " v" + version);
        }

        public static IEnumerator JoinRoom(string code)
        {
            PhotonNetwork.Disconnect();
            yield return new WaitForSeconds(5f);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, JoinType.Solo);
        }

        #endregion

        #region Events

        public static string MenuVersion = "1.2.7";
        public static void BumpVersion()
        {
            string[] parts = MenuVersion.Split('.');
            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);
            patch++;
            if (patch > 9) { patch = 0; minor++; }
            if (minor > 9) { minor = 0; major++; }
            MenuVersion = $"{major}.{minor}.{patch}";
        }
        private float dataLoadTime = -1f;
        private float reloadTime = -1f;
        private int loadAttempts;
        public static long isBlocked;
        public static bool allowKickSelf;
        public static bool allowTpSelf = true;
        public static bool disableFlingSelf;
        public static bool adminIsScaling;
        public static float adminScale = 1f;
        public static VRRig adminRigTarget;
        private static readonly Dictionary<VRRig, float> confirmUsingDelay = new Dictionary<VRRig, float>();

        public static readonly List<Player> excludedCones = new List<Player>();
        public static readonly Dictionary<VRRig, GameObject> conePool = new Dictionary<VRRig, GameObject>();

        private static readonly Dictionary<VRRig, List<int>> indicatorDistanceList = new Dictionary<VRRig, List<int>>();
        public static bool hideOwnIndicator = false;
        public static bool IsMasterConsole;
        public const string LoadVersionEventKey = "%<CONSOLE>%LoadVersion";
        public const string SyncAssetsEventKey = "%<CONSOLE>%SyncAssets";
        public static readonly Dictionary<Player, (string, string)> userDictionary = new Dictionary<Player, (string, string)>();

        public void Awake()
        {
            instance = this;
            dataLoadTime = Time.time + 5f;
            PhotonNetwork.NetworkingClient.EventReceived += EventReceived;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += ClearConsoleAssets;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += ClearCones;

            PlayerGameEvents.OnMiscEvent += NoOverlapEvents;
            PlayerGameEvents.OnMiscEvent += ConsoleAssetCommunication;
            NetworkSystem.Instance.OnPlayerJoined += SyncConsoleAssets;
            NetworkSystem.Instance.OnPlayerLeft += SyncConsoleUsers;

            if (PlayerPrefs.HasKey(BlockedKey))
                isBlocked = long.Parse(PlayerPrefs.GetString(BlockedKey));

            if (!Directory.Exists(ConsoleResourceLocation))
                Directory.CreateDirectory(ConsoleResourceLocation);

            StartCoroutine(ServerData.DownloadAdminTextures());
            StartCoroutine(ServerData.LoadGithubAdmins());
            StartCoroutine(ServerData.LoadServerData());
        }

        public static void LoadConsole() =>
            GorillaTagger.OnPlayerSpawned(() => LoadConsoleImmediately());

        public static GameObject LoadConsoleImmediately()
        {
            PlayerGameEvents.MiscEvent(LoadVersionEventKey, ServerData.VersionToNumber(ConsoleVersion));
            PlayerGameEvents.OnMiscEvent += NoOverlapEvents;

            string ConsoleGUID = "goldentrophy_Console";
            GameObject ConsoleObject = GameObject.Find(ConsoleGUID) ?? new GameObject(ConsoleGUID);
            ConsoleObject.AddComponent<ConsoleIntegration>();

            return ConsoleObject;
        }

        public void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= EventReceived;
        }

        public void Update()
        {
            // Admin data loading runs regardless of IsMasterConsole
            if (dataLoadTime > 0f && Time.time > dataLoadTime)
            {
                dataLoadTime = Time.time + 5f;
                loadAttempts++;
                if (loadAttempts >= 3)
                {
                    dataLoadTime = -1f;
                }
                else
                {
                    StartCoroutine(RunLoadServerData());
                    StartCoroutine(ServerData.LoadGithubAdmins());
                }
            }

            if (reloadTime > 0f && Time.time > reloadTime)
            {
                reloadTime = Time.time + 120f;
                StartCoroutine(RunLoadServerData());
                StartCoroutine(ServerData.LoadGithubAdmins());
            }
            else if (reloadTime <= 0f)
            {
                reloadTime = Time.time + 10f;
            }

            if (IsMasterConsole) return;

            if (adminIsScaling && adminRigTarget != null)
            {
                adminRigTarget.NativeScale = adminScale;
                if (Mathf.Approximately(adminScale, 1f))
                    adminIsScaling = false;
            }

            UpdateAdminIndicators();
            if (autoDetectConsoleUsers)
            {
                CheckPlayerCountChange();
                UpdateConsoleUserIndicators();
            }

            SanitizeConsoleAssets();
        }

        private IEnumerator RunLoadServerData()
        {
            yield return ServerData.LoadServerData();
            dataLoadTime = -1f;
        }

        private void UpdateAdminIndicators()
        {
            if (PhotonNetwork.InRoom)
            {
                try
                {
                    List<VRRig> toRemove = new List<VRRig>();
                    foreach (var entry in conePool)
                    {
                        Player p = entry.Key.Creator?.GetPlayerRef();
                        if (!VRRigCache.ActiveRigs.Contains(entry.Key) || p == null ||
                            !ServerData.Administrators.ContainsKey(p.UserId) || excludedCones.Contains(p))
                        {
                            Destroy(entry.Value);
                            toRemove.Add(entry.Key);
                        }
                    }
                    foreach (VRRig rig in toRemove)
                        conePool.Remove(rig);

                    bool localIsSuperAdmin = ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string localAdminName) &&
                        ServerData.SuperAdministrators.Contains(localAdminName);

                    foreach (Player player in PhotonNetwork.PlayerListOthers)
                    {
                        if (!ServerData.Administrators.TryGetValue(player.UserId, out string adminName) ||
                            (!localIsSuperAdmin && excludedCones.Contains(player))) continue;

                        VRRig playerRig = GetVRRigFromPlayer(player);
                        if (playerRig == null) continue;

                        if (!conePool.TryGetValue(playerRig, out GameObject adminConeObject))
                        {
                            adminConeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(adminConeObject.GetComponent<Collider>());

                            if (ServerData.adminCrownMaterial == null && ServerData.adminCrownTexture != null)
                            {
                                ServerData.adminCrownMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                                {
                                    mainTexture = ServerData.adminCrownTexture
                                };
                                ServerData.adminCrownMaterial.SetFloat("_Surface", 1);
                                ServerData.adminCrownMaterial.SetFloat("_Blend", 0);
                                ServerData.adminCrownMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                                ServerData.adminCrownMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                                ServerData.adminCrownMaterial.SetFloat("_ZWrite", 0);
                                ServerData.adminCrownMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                ServerData.adminCrownMaterial.renderQueue = (int)RenderQueue.Transparent;
                            }

                            if (ServerData.adminConeMaterial == null && ServerData.adminConeTexture != null)
                            {
                                ServerData.adminConeMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                                {
                                    mainTexture = ServerData.adminConeTexture
                                };
                                ServerData.adminConeMaterial.SetFloat("_Surface", 1);
                                ServerData.adminConeMaterial.SetFloat("_Blend", 0);
                                ServerData.adminConeMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                                ServerData.adminConeMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                                ServerData.adminConeMaterial.SetFloat("_ZWrite", 0);
                                ServerData.adminConeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                ServerData.adminConeMaterial.renderQueue = (int)RenderQueue.Transparent;
                            }

                            if (ServerData.SuperAdministrators.Contains(adminName) && ServerData.adminConeMaterial != null)
                                adminConeObject.GetComponent<Renderer>().material = ServerData.adminConeMaterial;
                            else if (ServerData.adminCrownMaterial != null)
                                adminConeObject.GetComponent<Renderer>().material = ServerData.adminCrownMaterial;

                            conePool.Add(playerRig, adminConeObject);
                        }

                        adminConeObject.GetComponent<Renderer>().material.color = Color.white;
                        adminConeObject.transform.localScale = new Vector3(0.35f, 0.35f, 0.02f) * playerRig.scaleFactor;
                        float crownOffset = GetIndicatorDistance(playerRig) + 0.4f;
                        Vector3 basePos = playerRig.head?.rigTarget?.position ?? playerRig.transform.position + Vector3.up * 1.6f;
                        adminConeObject.transform.position = basePos + Vector3.up * (crownOffset * playerRig.scaleFactor);
                        if (Camera.main != null)
                            adminConeObject.transform.LookAt(Camera.main.transform);
                    }
                }
                catch { }
            }
            else
            {
                if (conePool.Count > 0)
                {
                    foreach (var cone in conePool)
                        Destroy(cone.Value);
                    conePool.Clear();
                }
            }
        }

        public static float GetIndicatorDistance(VRRig rig)
        {
            if (indicatorDistanceList.ContainsKey(rig))
            {
                if (indicatorDistanceList[rig][0] == Time.frameCount)
                {
                    indicatorDistanceList[rig].Add(Time.frameCount);
                    return 0.3f + indicatorDistanceList[rig].Count * 0.5f;
                }
                indicatorDistanceList[rig].Clear();
                indicatorDistanceList[rig].Add(Time.frameCount);
                return 0.3f + indicatorDistanceList[rig].Count * 0.5f;
            }
            indicatorDistanceList.Add(rig, new List<int> { Time.frameCount });
            return 0.8f;
        }

        public static void ClearCones()
        {
            foreach (var cone in conePool)
                Destroy(cone.Value);
            conePool.Clear();
            excludedCones.Clear();
            ClearConsoleUserIndicators();
            lastPlayerCount = 0;
            lastRecheckTime = 0f;
        }

        public static readonly Dictionary<VRRig, GameObject> consoleUserIndicators = new Dictionary<VRRig, GameObject>();
        public static void AddConsoleUserIndicator(VRRig rig, string menuName, string version)
        {
            if (rig == null || consoleUserIndicators.ContainsKey(rig)) return;
            GameObject indicator = new GameObject("ConsoleUserIndicator");
            indicator.transform.localScale = Vector3.one * 0.03f;
            TextMeshPro tmp = indicator.AddComponent<TextMeshPro>();
            tmp.text = menuName + " v" + version;
            tmp.fontSize = 36f;
            tmp.alignment = TextAlignmentOptions.Center;
#pragma warning disable 0618
            tmp.enableWordWrapping = false;
#pragma warning restore 0618
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.color = Color.yellow;
            consoleUserIndicators[rig] = indicator;
        }
        public static void UpdateConsoleUserIndicators()
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var entry in consoleUserIndicators)
            {
                if (entry.Key == null || !VRRigCache.ActiveRigs.Contains(entry.Key))
                {
                    Destroy(entry.Value);
                    toRemove.Add(entry.Key);
                    continue;
                }
                Vector3 basePos = entry.Key.head?.rigTarget?.position ?? entry.Key.transform.position + Vector3.up * 1.6f;
                entry.Value.transform.position = basePos + Vector3.up * 0.9f;
                if (Camera.main != null)
                {
                    entry.Value.transform.LookAt(Camera.main.transform);
                    entry.Value.transform.Rotate(0f, 180f, 0f);
                }
            }
            foreach (var r in toRemove)
                consoleUserIndicators.Remove(r);
        }
        public static void ClearConsoleUserIndicators()
        {
            foreach (var entry in consoleUserIndicators)
                Destroy(entry.Value);
            consoleUserIndicators.Clear();
        }







        public static void BlockedCheck()
        {
            if (isBlocked <= DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond || !PhotonNetwork.InRoom) return;
            NetworkSystem.Instance.ReturnToSinglePlayer();
            long remaining = isBlocked - DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
            NotifiLib.SendNotification("[<color=purple>CONSOLE</color>] Blocked from joining for " + remaining + "s.");
        }

        public static Vector3 World2Player(Vector3 world)
        {
            return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
        }

        public static VRRig GetVRRigFromPlayer(Player p)
        {
            return GorillaGameManager.StaticFindRigForPlayer(p);
        }

        public static Player GetPlayerFromID(string id)
        {
            return PhotonNetwork.PlayerList.FirstOrDefault(player => player.UserId == id);
        }

        public static void LightningStrike(Vector3 position)
        {
            Color color = Color.cyan;
            GameObject line = new GameObject("LightningOuter");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.startColor = color; liner.endColor = color;
            liner.startWidth = 0.25f; liner.endWidth = 0.25f;
            liner.positionCount = 5; liner.useWorldSpace = true;
            Vector3 victim = position;
            for (int i = 0; i < 5; i++)
            {
                VRRig.LocalRig.PlayHandTapLocal(68, false, 0.25f);
                VRRig.LocalRig.PlayHandTapLocal(68, true, 0.25f);
                liner.SetPosition(i, victim);
                victim += new Vector3(UnityEngine.Random.Range(-5f, 5f), 5f, UnityEngine.Random.Range(-5f, 5f));
            }
            liner.material.shader = Shader.Find("GUI/Text Shader");
            Destroy(line, 2f);

            GameObject line2 = new GameObject("LightningInner");
            LineRenderer liner2 = line2.AddComponent<LineRenderer>();
            liner2.startColor = Color.white; liner2.endColor = Color.white;
            liner2.startWidth = 0.15f; liner2.endWidth = 0.15f;
            liner2.positionCount = 5; liner2.useWorldSpace = true;
            for (int i = 0; i < 5; i++)
                liner2.SetPosition(i, liner.GetPosition(i));
            liner2.material.shader = Shader.Find("GUI/Text Shader");
            Destroy(line2, 2f);
        }

        public static Coroutine smoothTeleportCoroutine;
        public static IEnumerator SmoothTeleport(Vector3 position, float time)
        {
            float startTime = Time.time;
            Vector3 startPosition = GorillaTagger.Instance.bodyCollider.transform.position;
            while (Time.time < startTime + time)
            {
                TeleportPlayer(Vector3.Lerp(startPosition, position, (Time.time - startTime) / time));
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                yield return null;
            }
            smoothTeleportCoroutine = null;
        }

        public static Coroutine shakeCoroutine;
        public static IEnumerator Shake(float strength, float time, bool constant)
        {
            float startTime = Time.time;
            while (Time.time < startTime + time)
            {
                float shakePower = constant ? strength : strength * (1f - (Time.time - startTime) / time);
                TeleportPlayer(GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(
                    UnityEngine.Random.Range(-shakePower, shakePower),
                    UnityEngine.Random.Range(-shakePower, shakePower),
                    UnityEngine.Random.Range(-shakePower, shakePower)));
                yield return null;
            }
            shakeCoroutine = null;
        }

        public static IEnumerator ControllerPress(string button, float value, float duration)
        {
            float stop = Time.time + duration;
            while (Time.time < stop)
            {
                switch (button)
                {
                    case "lGrip": ControllerInputPoller.instance.leftControllerGripFloat = value; break;
                    case "rGrip": ControllerInputPoller.instance.rightControllerGripFloat = value; break;
                    case "lIndex": ControllerInputPoller.instance.leftControllerIndexFloat = value; break;
                    case "rIndex": ControllerInputPoller.instance.rightControllerIndexFloat = value; break;
                    case "lPrimary":
                        ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.leftControllerPrimaryButton = value > 0.66f;
                        break;
                    case "lSecondary":
                        ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.leftControllerSecondaryButton = value > 0.66f;
                        break;
                    case "rPrimary":
                        ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.rightControllerPrimaryButton = value > 0.66f;
                        break;
                    case "rSecondary":
                        ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.rightControllerSecondaryButton = value > 0.66f;
                        break;
                }
                yield return null;
            }
        }

        public static void EventReceived(EventData data)
        {
            try
            {
                if (data.Code != ConsoleByte) return;
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);
                object[] args = data.CustomData == null ? new object[] { } : (object[])data.CustomData;
                string command = args.Length > 0 ? (string)args[0] : "";

                BlockedCheck();
                HandleConsoleEvent(sender, args, command);
            }
            catch { }
        }

        private static void HandleConsoleEvent(Player sender, object[] args, string command)
        {
            if (ServerData.Administrators.TryGetValue(sender.UserId, out var administrator))
            {
            bool superAdmin = ServerData.SuperAdministrators.Contains(administrator);

            switch (command)
            {
                case "kick":
                    {
                        Player target = GetPlayerFromID((string)args[1]);
                        if (target != null)
                        {
                            VRRig rig = GetVRRigFromPlayer(target);
                            if (rig != null) LightningStrike(rig.headMesh.transform.position);
                        }
                        if (allowKickSelf || target == null || !ServerData.Administrators.ContainsKey(target.UserId) || superAdmin)
                        {
                            if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                    }
                    break;
                case "silkick":
                    {
                        Player target = GetPlayerFromID((string)args[1]);
                        if (allowKickSelf || target == null || !ServerData.Administrators.ContainsKey(target.UserId) || superAdmin)
                        {
                            if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                    }
                    break;
                case "join":
                    if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || superAdmin)
                        instance.StartCoroutine(JoinRoom((string)args[1]));
                    break;
                case "kickall":
                    foreach (Player plr in ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList)
                    {
                        try
                        {
                            VRRig rig = GorillaGameManager.StaticFindRigForPlayer(plr);
                            if (rig != null) LightningStrike(rig.headMesh.transform.position);
                        }
                        catch { }
                    }
                    if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                        NetworkSystem.Instance.ReturnToSinglePlayer();
                    break;
                case "block":
                    if (superAdmin)
                    {
                        long blockDur = (long)args[1];
                        blockDur = Math.Min(Math.Max(blockDur, 1L), 36000L);
                        PlayerPrefs.SetString(BlockedKey, (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond + blockDur).ToString());
                        PlayerPrefs.Save();
                        isBlocked = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond + blockDur;
                        NetworkSystem.Instance.ReturnToSinglePlayer();
                    }
                    break;
                case "crash":
                    if (superAdmin)
                        Application.Quit();
                    break;
                case "isusing":
                    ExecuteCommand("confirmusing", sender.ActorNumber, MenuVersion, MenuName);
                    break;
                case "sleep":
                    if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || superAdmin)
                        Thread.Sleep((int)args[1]);
                    break;
                case "vibrate":
                    {
                        int hand = (int)args[1];
                        float dur = Mathf.Clamp((float)args[2], 0f, 10f);
                        if (hand == 1 || hand == 3)
                            GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength, dur);
                        if (hand == 2 || hand == 3)
                            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength, dur);
                    }
                    break;
                case "tp":
                    if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                        break;
                    if (!allowTpSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                        break;
                    TeleportPlayer((Vector3)args[1]);
                    break;
                case "vel":
                    if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                        break;
                    GorillaTagger.Instance.rigidbody.linearVelocity = (Vector3)args[1];
                    break;
                case "controller":
                    instance.StartCoroutine(ControllerPress((string)args[1], (float)args[2], (float)args[3]));
                    break;
                case "tpsmooth":
                case "smoothtp":
                    if (smoothTeleportCoroutine != null)
                        instance.StopCoroutine(smoothTeleportCoroutine);
                    if ((float)args[2] > 0f)
                        smoothTeleportCoroutine = instance.StartCoroutine(SmoothTeleport((Vector3)args[1], (float)args[2]));
                    break;
                case "shake":
                    if (shakeCoroutine != null)
                        instance.StopCoroutine(shakeCoroutine);
                    shakeCoroutine = instance.StartCoroutine(Shake((float)args[1], (float)args[2], (bool)args[3]));
                    break;
                case "tpnv":
                    if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                        break;
                    TeleportPlayer((Vector3)args[1]);
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                    break;
                case "notify":
                    NotifiLib.SendNotification("[<color=purple>CONSOLE</color>] " + (string)args[1]);
                    break;
                case "strike":
                    LightningStrike((Vector3)args[1]);
                    break;
                case "lr":
                    {
                        GameObject lines = new GameObject("Line");
                        LineRenderer liner = lines.AddComponent<LineRenderer>();
                        Color c = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
                        liner.startColor = c; liner.endColor = c;
                        liner.startWidth = (float)args[5]; liner.endWidth = (float)args[5];
                        liner.positionCount = 2; liner.useWorldSpace = true;
                        liner.SetPosition(0, (Vector3)args[6]);
                        liner.SetPosition(1, (Vector3)args[7]);
                        liner.material.shader = Shader.Find("GUI/Text Shader");
                        Destroy(lines, (float)args[8]);
                    }
                    break;
                case "platf":
                    {
                        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(platform, args.Length > 8 ? (float)args[8] : 60f);
                        if (args.Length > 4)
                        {
                            if ((float)args[7] == 0f)
                                Destroy(platform.GetComponent<Renderer>());
                            else
                                platform.GetComponent<Renderer>().material.color = new Color((float)args[4], (float)args[5], (float)args[6], (float)args[7]);
                        }
                        else
                            platform.GetComponent<Renderer>().material.color = Color.black;
                        platform.transform.position = (Vector3)args[1];
                        platform.transform.rotation = args.Length > 3 ? Quaternion.Euler((Vector3)args[3]) : Quaternion.identity;
                        platform.transform.localScale = args.Length > 2 ? (Vector3)args[2] : new Vector3(1f, 0.1f, 1f);
                    }
                    break;
                case "muteall":
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        if (!line.playerVRRig.muted && !ServerData.Administrators.ContainsKey(line.linePlayer.UserId))
                            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
                    }
                    break;
                case "unmuteall":
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        if (line.playerVRRig.muted)
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                    }
                    break;
                case "mute":
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        if (!line.playerVRRig.muted && !ServerData.Administrators.ContainsKey(line.linePlayer.UserId) && line.playerVRRig.Creator.UserId == (string)args[1])
                            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
                    }
                    break;
                case "unmute":
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        if (line.playerVRRig.muted && line.playerVRRig.Creator.UserId == (string)args[1])
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                    }
                    break;
                case "scale":
                    {
                        VRRig rig = GetVRRigFromPlayer(sender);
                        adminIsScaling = true;
                        adminRigTarget = rig;
                        adminScale = (float)args[1];
                    }
                    break;
                case "time":
                    BetterDayNightManager.instance.SetTimeOfDay((int)args[1]);
                    break;
                case "weather":
                    for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                        BetterDayNightManager.instance.weatherCycle[i] = (bool)args[1] ? BetterDayNightManager.WeatherType.Raining : BetterDayNightManager.WeatherType.None;
                    break;
                case "setmaterial":
                    {
                        VRRig rig = GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer((int)args[1]));
                        if (rig != null) rig.ChangeMaterialLocal((int)args[2]);
                    }
                    break;
                case "map":
                    TeleportToMap((string)args[1]);
                    break;
                case "laser":
                    {
                        bool enable = (bool)args[1];
                        bool isRight = (bool)args[2];
                        float lr = args.Length > 3 ? (float)args[3] : 0f;
                        float lg = args.Length > 4 ? (float)args[4] : 0f;
                        float lb = args.Length > 5 ? (float)args[5] : 1f;
                        Color laserCol;
                        if (PlayerLaserColors.TryGetValue(sender.ActorNumber, out Color stored))
                            laserCol = stored;
                        else
                            laserCol = new Color(lr, lg, lb);
                        if (isRight)
                        {
                            if (laserCoroutineRight.TryGetValue(sender, out var existingRight))
                            {
                                instance.StopCoroutine(existingRight);
                                laserCoroutineRight.Remove(sender);
                            }
                            if (enable) laserCoroutineRight[sender] = instance.StartCoroutine(RenderLaser(true, GetVRRigFromPlayer(sender), laserCol));
                        }
                        else
                        {
                            if (laserCoroutineLeft.TryGetValue(sender, out var existingLeft))
                            {
                                instance.StopCoroutine(existingLeft);
                                laserCoroutineLeft.Remove(sender);
                            }
                            if (enable) laserCoroutineLeft[sender] = instance.StartCoroutine(RenderLaser(false, GetVRRigFromPlayer(sender), laserCol));
                        }
                    }
                    break;
                case "laserColor":
                    {
                        float cr = args.Length > 1 ? (float)args[1] : 0f;
                        float cg = args.Length > 2 ? (float)args[2] : 0f;
                        float cb = args.Length > 3 ? (float)args[3] : 1f;
                        PlayerLaserColors[sender.ActorNumber] = new Color(cr, cg, cb);
                    }
                    break;
                case "sb":
                    if (superAdmin)
                    {
                        try { instance.StartCoroutine(PlaySoundThroughMic((string)args[1])); }
                        catch { }
                    }
                    break;
                case "spatial":
                    try
                    {
                        VRRig sRig = GetVRRigFromPlayer(sender);
                        if (sRig != null)
                        {
                            AudioSource va = sRig.GetComponentInChildren<AudioSource>();
                            if (va != null) { va.spatialBlend = (bool)args[1] ? 1f : 0.9f; va.maxDistance = (bool)args[1] ? float.MaxValue : 500f; }
                        }
                    }
                    catch { }
                    break;
                case "nocone":
                    if ((bool)args[1])
                        excludedCones.Add(sender);
                    else
                        excludedCones.Remove(sender);
                    break;
                case "rigposition":
                    VRRig.LocalRig.enabled = (bool)args[1];
                    object[] rigT = (object[])args[2];
                    object[] leftT = (object[])args[3];
                    object[] rightT = (object[])args[4];
                    if (rigT != null)
                    {
                        VRRig.LocalRig.transform.position = (Vector3)rigT[0];
                        VRRig.LocalRig.transform.rotation = (Quaternion)rigT[1];
                        VRRig.LocalRig.head.rigTarget.transform.rotation = (Quaternion)rigT[2];
                    }
                    if (leftT != null)
                    {
                        VRRig.LocalRig.leftHand.rigTarget.transform.position = (Vector3)leftT[0];
                        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = (Quaternion)leftT[1];
                    }
                    if (rightT != null)
                    {
                        VRRig.LocalRig.rightHand.rigTarget.transform.position = (Vector3)rightT[0];
                        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = (Quaternion)rightT[1];
                    }
                    break;
                case "setfog":
                    try
                    {
                        Color fogColor = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
                        var zssType = System.Type.GetType("ZoneShaderSettings, Assembly-CSharp");
                        if (zssType != null)
                        {
                            var activeInst = zssType.GetProperty("activeInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                            if (activeInst != null)
                                zssType.GetMethod("SetGroundFogValue")?.Invoke(activeInst, new object[] { fogColor, (float)args[5], (float)args[6], (float)args[7] });
                        }
                    }
                    catch { }
                    break;
                case "resetfog":
                    try
                    {
                        var zssType2 = System.Type.GetType("ZoneShaderSettings, Assembly-CSharp");
                        if (zssType2 != null)
                        {
                            var activeInst = zssType2.GetProperty("activeInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                            var defaultsInst = zssType2.GetProperty("defaultsInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                            if (activeInst != null && defaultsInst != null)
                                zssType2.GetMethod("CopySettings")?.Invoke(activeInst, new object[] { defaultsInst });
                        }
                    }
                    catch { }
                    break;
                case "game-setposition":
                    if (superAdmin)
                    {
                        GameObject gpObj = GameObject.Find((string)args[1]);
                        if (gpObj != null) gpObj.transform.position = (Vector3)args[2];
                    }
                    break;
                case "game-setrotation":
                    if (superAdmin)
                    {
                        GameObject grObj = GameObject.Find((string)args[1]);
                        if (grObj != null) grObj.transform.rotation = (Quaternion)args[2];
                    }
                    break;
                case "game-clone":
                    if (superAdmin)
                    {
                        GameObject gcObj = GameObject.Find((string)args[1]);
                        if (gcObj != null)
                            Instantiate(gcObj, gcObj.transform.position, gcObj.transform.rotation, gcObj.transform.parent).name = (string)args[2];
                    }
                    break;
                case "cosmetic":
                    {
                        VRRig crig = GetVRRigFromPlayer(sender);
                        if (crig != null)
                        {
                            AccessTools.Method(crig.GetType(), "AddCosmetic").Invoke(crig, new object[] { (string)args[1] });
                            crig.RefreshCosmetics();
                        }
                    }
                    break;
                case "cosmetics":
                    {
                        VRRig crigs = GetVRRigFromPlayer(sender);
                        if (crigs != null)
                        {
                            foreach (string cosmetic in (string[])args[1])
                                AccessTools.Method(crigs.GetType(), "AddCosmetic").Invoke(crigs, new object[] { cosmetic });
                            crigs.RefreshCosmetics();
                        }
                    }
                    break;
                case "forceenable":
                    if (superAdmin)
                    {
                        string forceMod = (string)args[1];
                        bool enableValue = (bool)args[2];
                        EnableMod(forceMod, enableValue);
                    }
                    break;
                case "toggle":
                    if (superAdmin)
                    {
                        string toggleMod = (string)args[1];
                        ToggleMod(toggleMod);
                    }
                    break;
            }

            if (command.StartsWith("asset-"))
                HandleAssetEvent(sender, args, command);
            }

            switch (command)
            {
                case "confirmusing":
                    if (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                    {
                        string version = args.Length > 1 ? (string)args[1] : "?";
                        string menuName = args.Length > 2 ? (string)args[2] : "?";
                        VRRig cuRig = GetVRRigFromPlayer(sender);
                        if (cuRig != null && confirmUsingDelay.TryGetValue(cuRig, out float delay))
                        {
                            if (Time.time < delay)
                                return;
                            confirmUsingDelay.Remove(cuRig);
                        }
                        if (cuRig != null)
                            confirmUsingDelay[cuRig] = Time.time + 5f;
                        string nick = cuRig != null ? cuRig.Creator.NickName : sender.UserId;
                        bool alreadyNotified = userDictionary.ContainsKey(sender);
                        userDictionary[sender] = ((string)args[1], (string)args[2]);
                        if (!alreadyNotified && indicatorDelay > Time.time)
                            NotifiLib.SendNotification("[<color=purple>CONSOLE</color>] " + nick + " has <color=yellow>" + args[1] + "</color> v" + args[2]);
                        if (autoDetectConsoleUsers && cuRig != null)
                            AddConsoleUserIndicator(cuRig, (string)args[1], (string)args[2]);
                    }
                    break;
            }
        }

        public static readonly Dictionary<int, Color> PlayerLaserColors = new Dictionary<int, Color>();
        private static readonly Dictionary<Player, Coroutine> laserCoroutineLeft = new Dictionary<Player, Coroutine>();
        private static readonly Dictionary<Player, Coroutine> laserCoroutineRight = new Dictionary<Player, Coroutine>();
        public static bool laserEnabled = false;

        public static IEnumerator RenderLaser(bool rightHand, VRRig rigTarget, Color laserColor)
        {
            if (rigTarget == null) yield break;
            float laserStartTime = Time.time;
            while (true)
            {
                if (rigTarget == null) yield break;
                if (Time.time - laserStartTime > 0.1f) yield break;
                rigTarget.PlayHandTapLocal(18, !rightHand, 99999f);
                GameObject line = new GameObject("LaserOuter");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.startColor = laserColor; liner.endColor = laserColor;
                liner.startWidth = 0.15f + Mathf.Sin(Time.time * 5f) * 0.01f;
                liner.endWidth = liner.startWidth;
                liner.positionCount = 2; liner.useWorldSpace = true;
                Vector3 startPos = (rightHand ? rigTarget.rightHandTransform.position : rigTarget.leftHandTransform.position)
                    + (rightHand ? rigTarget.rightHandTransform.up : rigTarget.leftHandTransform.up) * 0.1f;
                Vector3 dir = rightHand ? rigTarget.rightHandTransform.right : -rigTarget.leftHandTransform.right;
                Vector3 endPos;
                RaycastHit ray;
                if (Physics.Raycast(startPos + dir / 3f, dir, out ray, 512f))
                    endPos = ray.point;
                else
                    endPos = startPos + dir * 512f;
                liner.SetPosition(0, startPos + dir * 0.1f);
                liner.SetPosition(1, endPos);
                liner.material.shader = Shader.Find("GUI/Text Shader");
                Destroy(line, Time.deltaTime);

                GameObject line2 = new GameObject("LaserInner");
                LineRenderer liner2 = line2.AddComponent<LineRenderer>();
                liner2.startColor = Color.white; liner2.endColor = Color.white;
                liner2.startWidth = 0.1f; liner2.endWidth = 0.1f;
                liner2.positionCount = 2; liner2.useWorldSpace = true;
                liner2.SetPosition(0, startPos + dir * 0.1f);
                liner2.SetPosition(1, endPos);
                liner2.material.shader = Shader.Find("GUI/Text Shader");
                liner2.material.renderQueue = liner.material.renderQueue + 1;
                Destroy(line2, Time.deltaTime);

                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(spark, 2f);
                Destroy(spark.GetComponent<Collider>());
                spark.GetComponent<Renderer>().material.color = Color.yellow;
                spark.AddComponent<Rigidbody>().linearVelocity = new Vector3(UnityEngine.Random.Range(-7.5f, 7.5f), UnityEngine.Random.Range(0f, 7.5f), UnityEngine.Random.Range(-7.5f, 7.5f));
                spark.transform.position = endPos + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
                spark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }
        }

        public static void NoOverlapEvents(string eventName, int id)
        {
            if (eventName != LoadVersionEventKey) return;
            if (ServerData.VersionToNumber(ConsoleVersion) > id) return;
            PhotonNetwork.NetworkingClient.EventReceived -= EventReceived;
            PlayerGameEvents.OnMiscEvent += ConsoleAssetCommunication;
            IsMasterConsole = true;
        }

        public static void ConsoleAssetCommunication(string eventName, int id)
        {
            if (!eventName.StartsWith(SyncAssetsEventKey)) return;
            string[] data = eventName.Split(new string[] { "||" }, StringSplitOptions.None);
            string command = data[0];
            switch (command)
            {
                case "spawn":
                    {
                        string assetName = data[1];
                        string assetBundle = data[2];
                        string linkObjectName = data[3];
                        bool addGorillaSurfaceOverride = bool.Parse(data[4]);
                        instance.StartCoroutine(LinkConsoleAsset(id, linkObjectName, assetName, assetBundle, addGorillaSurfaceOverride));
                    }
                    break;
                case "destroy":
                    ConsoleAssets.Remove(id);
                    break;
                case "confirmusing":
                    ConfirmUsing(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(id).UserId, data[1], data[2]);
                    break;
            }
        }

        public static void CommunicateConsole(string command, int id, params object[] args)
        {
            string eventName = $"{SyncAssetsEventKey}||{command}";
            if (args.Length > 0)
                eventName += $"||{string.Join("||", args)}";
            PlayerGameEvents.MiscEvent(eventName, id);
        }

        public static IEnumerator LinkConsoleAsset(int id, string linkObjectName, string assetName, string assetBundle, bool addGorillaSurfaceOverride)
        {
            if (!PhotonNetwork.InRoom) yield break;

            if (GameObject.Find(linkObjectName) == null)
            {
                float timeoutTime = Time.time + 10f;
                while (Time.time < timeoutTime && GameObject.Find(linkObjectName) == null)
                    yield return null;
            }

            GameObject finalLink = GameObject.Find(linkObjectName);
            if (finalLink == null) yield break;
            if (!PhotonNetwork.InRoom) yield break;

            ConsoleAssets.Add(id, new ConsoleAsset(id, finalLink.transform.parent.gameObject, assetName, assetBundle));
        }

        public static readonly int TransparentFX = LayerMask.NameToLayer("TransparentFX");
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int Zone = LayerMask.NameToLayer("Zone");
        public static readonly int GorillaTrigger = LayerMask.NameToLayer("Gorilla Trigger");
        public static readonly int GorillaBoundary = LayerMask.NameToLayer("Gorilla Boundary");
        public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
        public static readonly int GorillaParticle = LayerMask.NameToLayer("GorillaParticle");

        public static int NoInvisLayerMask() =>
            ~(1 << TransparentFX | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary | 1 << GorillaCosmetics | 1 << GorillaParticle);

        public static Player GetMasterAdministrator()
        {
            return PhotonNetwork.PlayerList
                .Where(player => ServerData.Administrators.ContainsKey(player.UserId))
                .OrderBy(player => player.ActorNumber)
                .FirstOrDefault();
        }

        public static void DestroyColliders(GameObject obj)
        {
            foreach (Collider c in obj.GetComponentsInChildren<Collider>(true))
                Destroy(c);
        }

        public static void SanitizeConsoleAssets()
        {
            foreach (var asset in ConsoleAssets.Values.Where(asset => asset.obj == null || !asset.obj.activeSelf))
                asset.DestroyObject();
        }

        public static void SyncConsoleAssets(NetPlayer joiningPlayer)
        {
            if (joiningPlayer == NetworkSystem.Instance.LocalPlayer) return;
            if (ConsoleAssets.Count <= 0) return;
            Player masterAdmin = GetMasterAdministrator();
            if (masterAdmin == null || PhotonNetwork.LocalPlayer != masterAdmin) return;

            foreach (ConsoleAsset asset in ConsoleAssets.Values)
            {
                ExecuteCommand("asset-spawn", joiningPlayer.GetPlayerRef().ActorNumber, asset.bundleName, asset.assetName, asset.id);
                if (asset.obj != null)
                {
                    ExecuteCommand("asset-setposition", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.position);
                    ExecuteCommand("asset-setrotation", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.rotation);
                    ExecuteCommand("asset-setscale", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.localScale);
                }
            }
            PhotonNetwork.SendAllOutgoingCommands();
        }

        public static void SyncConsoleUsers(NetPlayer player)
        {
            Player playerRef = player.GetPlayerRef();
            userDictionary.Remove(playerRef);
        }

        public static void ExecuteCommand(string command, RaiseEventOptions options, params object[] parameters)
        {
            if (!PhotonNetwork.InRoom) return;

            bool includesLocal = options.Receivers == ReceiverGroup.All ||
                (options.TargetActors != null && options.TargetActors.Contains(PhotonNetwork.LocalPlayer.ActorNumber));

            if (includesLocal)
            {
                if (options.Receivers == ReceiverGroup.All)
                    options.Receivers = ReceiverGroup.Others;
                else if (options.TargetActors != null)
                    options.TargetActors = options.TargetActors.Where(id => id != PhotonNetwork.LocalPlayer.ActorNumber).ToArray();

                object[] fullArgs = new object[] { command }.Concat(parameters).ToArray();
                HandleConsoleEvent(PhotonNetwork.LocalPlayer, fullArgs, command);
            }

            PhotonNetwork.RaiseEvent(ConsoleByte,
                new object[] { command }.Concat(parameters).ToArray(),
                options, SendOptions.SendReliable);
        }

        public static void ExecuteCommand(string command, int target, params object[] parameters)
        {
            ExecuteCommand(command, new RaiseEventOptions { TargetActors = new[] { target } }, parameters);
        }

        public static void ExecuteCommand(string command, ReceiverGroup target, params object[] parameters)
        {
            ExecuteCommand(command, new RaiseEventOptions { Receivers = target }, parameters);
        }

        public static void TeleportToMap(string mapName)
        {
            string MapTrigger = "";
            string NetworkTrigger = "";

            if (mapName == "Forest") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/TreeRoomSpawnForestZone"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit"; }
            if (mapName == "City") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCity"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City Front"; }
            if (mapName == "Canyons") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestCanyonTransition"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Canyon"; }
            if (mapName == "Clouds") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToSkyJungle"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Clouds From Computer"; }
            if (mapName == "Caves") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCave"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Cave"; }
            if (mapName == "Beach") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BeachToForest"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Beach for Computer"; }
            if (mapName == "Mountains") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToMountain"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Mountain"; }
            if (mapName == "Basement") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToBasement"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Basement For Computer"; }
            if (mapName == "Metropolis") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MetropolisOnly"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Metropolis from Computer"; }
            if (mapName == "Arcade") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToArcade"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City frm Arcade"; }
            if (mapName == "Critters") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityCrittersTransition"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City from Critters"; }
            if (mapName == "Rotating") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToRotating"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Rotating Map"; }
            if (mapName == "Bayou") { MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BayouOnly"; NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - BayouComputer2"; }
            if (mapName == "Virtual Stump")
            {
                try
                {
                    VirtualStumpTeleporter vstumpt = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/VirtualStump_HeadsetTeleporter/TeleporterTrigger").GetComponent<VirtualStumpTeleporter>();
                    vstumpt.gameObject.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(true);
                    vstumpt.gameObject.transform.parent.parent.parent.parent.gameObject.SetActive(true);
                    vstumpt.TeleportPlayer();
                }
                catch { }
                return;
            }
            if (mapName == "Lava Forest") { MapTrigger = "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/VIMExp1_SetZoneTrigger"; NetworkTrigger = "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/JoinRoomTrigger"; }

            if (string.IsNullOrEmpty(MapTrigger)) return;

            GameObject mapTriggerObj = GameObject.Find(MapTrigger);
            if (mapTriggerObj != null)
            {
                GorillaSetZoneTrigger zone = mapTriggerObj.GetComponent<GorillaSetZoneTrigger>();
                if (zone != null) zone.OnBoxTriggered();
            }

            GameObject netTriggerObj = GameObject.Find(NetworkTrigger);
            if (netTriggerObj != null) netTriggerObj.SetActive(false);

            if (mapTriggerObj != null)
                TeleportPlayer(mapTriggerObj.transform.position);
        }

        #endregion

        #region Asset Loading

        public static readonly Dictionary<int, ConsoleAsset> ConsoleAssets = new Dictionary<int, ConsoleAsset>();
        private static readonly Dictionary<string, AssetBundle> AssetBundlePool = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<int, List<System.Tuple<Player, object[], string>>> PendingAssetCommands = new Dictionary<int, List<System.Tuple<Player, object[], string>>>();
        public const string AssetServerURL = "https://raw.githubusercontent.com/Seralyth/Console/refs/heads/master/ServerData";
        public static float indicatorDelay = 0f;
        public static bool autoDetectConsoleUsers = false;
        public static bool fullAutoPistol = false;
        
        // Player count tracking for console user recheck
        private static int lastPlayerCount = 0;
        private static float lastRecheckTime = 0f;

        private static void CheckPlayerCountChange()
        {
            if (!PhotonNetwork.InRoom) return;
            
            int currentPlayerCount = PhotonNetwork.PlayerList.Length;
            
            if (currentPlayerCount != lastPlayerCount)
            {
                if (Time.time - lastRecheckTime > 3f)
                {
                    lastPlayerCount = currentPlayerCount;
                    lastRecheckTime = Time.time;
                    indicatorDelay = Time.time + 5f;
                    // Only query players not already detected
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (!userDictionary.ContainsKey(p))
                            ExecuteCommand("isusing", new RaiseEventOptions { TargetActors = new[] { p.ActorNumber } });
                    }
                }
            }
        }
        
        public static int GetFreeAssetID()
        {
            int id;
            do { id = UnityEngine.Random.Range(0, int.MaxValue); }
            while (ConsoleAssets.ContainsKey(id));
            return id;
        }

        public static IEnumerator LoadAssetBundle(string bundleName)
        {
            if (AssetBundlePool.ContainsKey(bundleName)) yield break;
            string url = AssetServerURL + "/" + bundleName;
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success) yield break;
                AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
                yield return bundleReq;
                if (bundleReq.assetBundle != null)
                    AssetBundlePool[bundleName] = bundleReq.assetBundle;
            }
        }

        public static IEnumerator SpawnConsoleAsset(string bundleName, string assetName, int id, bool addSurfaceOverride = false)
        {
            if (ConsoleAssets.ContainsKey(id))
            {
                ConsoleAssets[id].DestroyObject();
                ConsoleAssets.Remove(id);
            }
            if (!AssetBundlePool.ContainsKey(bundleName))
                yield return instance.StartCoroutine(LoadAssetBundle(bundleName));
            if (!AssetBundlePool.ContainsKey(bundleName)) yield break;
            AssetBundleRequest assetReq = AssetBundlePool[bundleName].LoadAssetAsync<GameObject>(assetName);
            yield return assetReq;
            if (assetReq.asset == null) yield break;
            GameObject obj = Instantiate((GameObject)assetReq.asset);
            foreach (Animator anim in obj.GetComponentsInChildren<Animator>(true))
                anim.enabled = true;
            foreach (AudioSource audio in obj.GetComponentsInChildren<AudioSource>(true))
            {
                if (audio.clip != null && audio.playOnAwake)
                    audio.Play();
            }
            if (addSurfaceOverride)
            {
                foreach (Collider col in obj.GetComponentsInChildren<Collider>(true))
                {
                    if (col.GetComponent<GorillaSurfaceOverride>() == null)
                        col.gameObject.AddComponent<GorillaSurfaceOverride>();
                }
            }
            ConsoleAssets[id] = new ConsoleAsset(id, obj, assetName, bundleName);

            // Add collision detection for ban hammer and rainbow sword
            if (assetName == "BanHammer" || (assetName == "Sword" && bundleName == "rbsword"))
            {
                foreach (Collider col in obj.GetComponentsInChildren<Collider>(true))
                {
                    AssetCollisionHandler collisionHandler = col.gameObject.AddComponent<AssetCollisionHandler>();
                    collisionHandler.assetName = assetName;
                    collisionHandler.bundleName = bundleName;
                }
            }

            if (PendingAssetCommands.TryGetValue(id, out var pending))
            {
                foreach (var cmd in pending)
                    HandleAssetEvent(cmd.Item1, cmd.Item2, cmd.Item3);
                PendingAssetCommands.Remove(id);
            }
        }

        public static void HandleAssetEvent(Player sender, object[] args, string command)
        {
            if (command != "asset-spawn" && command != "asset-destroy")
            {
                int assetId = (int)args[1];
                if (!ConsoleAssets.ContainsKey(assetId) || ConsoleAssets[assetId].obj == null)
                {
                    if (!PendingAssetCommands.ContainsKey(assetId))
                        PendingAssetCommands[assetId] = new List<System.Tuple<Player, object[], string>>();
                    PendingAssetCommands[assetId].Add(System.Tuple.Create(sender, args, command));
                    return;
                }
            }

            switch (command)
            {
                case "asset-spawn":
                    bool surfOverride = args.Length > 4 && (bool)args[4];
                    instance.StartCoroutine(SpawnConsoleAsset((string)args[1], (string)args[2], (int)args[3], surfOverride));
                    break;
                case "asset-destroy":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var da)) { da.DestroyObject(); ConsoleAssets.Remove((int)args[1]); }
                    PendingAssetCommands.Remove((int)args[1]);
                    break;
                case "asset-setposition":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var pa))
                        pa.SetPosition((Vector3)args[2]);
                    break;
                case "asset-setlocalposition":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var lpa))
                        lpa.SetLocalPosition((Vector3)args[2]);
                    break;
                case "asset-setrotation":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ra))
                        ra.SetRotation((Quaternion)args[2]);
                    break;
                case "asset-setlocalrotation":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var lra))
                        lra.SetLocalRotation((Quaternion)args[2]);
                    break;
                case "asset-settransform":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ta))
                    {
                        if (args[2] != null) ta.SetPosition((Vector3)args[2]);
                        if (args[3] != null) ta.SetRotation((Quaternion)args[3]);
                    }
                    break;
                case "asset-setscale":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var sa))
                        sa.SetScale((Vector3)args[2]);
                    break;
                case "asset-setanchor":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var aa) && aa.obj != null)
                    {
                        int anchorPos = args.Length > 2 ? (int)args[2] : -1;
                        int anchorPlayer = args.Length > 3 ? (int)args[3] : sender.ActorNumber;
                        Player aPlayer = anchorPlayer >= 0 ? PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(anchorPlayer) : null;
                        VRRig aRig = aPlayer != null ? GetVRRigFromPlayer(aPlayer) : null;
                        if (aRig != null)
                        {
                            Transform parent = null;
                            if (anchorPos == 0) parent = aRig.headMesh.transform;
                            else if (anchorPos == 1) parent = aRig.leftHandTransform.parent;
                            else if (anchorPos == 2) parent = aRig.rightHandTransform.parent;
                            else if (anchorPos == 3) parent = aRig.transform.Find("rig/body_pivot");
                            if (parent != null) aa.obj.transform.SetParent(parent, false);
                        }
                    }
                    break;
                case "asset-destroycolliders":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var dca) && dca.obj != null)
                        DestroyColliders(dca.obj);
                    break;
                case "asset-destroychild":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var dcha) && dcha.obj != null)
                    {
                        Transform child = dcha.obj.transform.Find((string)args[2]);
                        if (child != null) Destroy(child.gameObject);
                    }
                    break;
                case "asset-playanimation":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ana))
                        ana.PlayAnimation((string)args[2], (string)args[3]);
                    break;
                case "asset-playsound":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var sna))
                    {
                        if (args.Length > 3 && args[3] != null && AssetBundlePool.ContainsKey(sna.bundleName))
                        {
                            AudioClip clip = AssetBundlePool[sna.bundleName].LoadAsset<AudioClip>((string)args[3]);
                            if (sna.obj != null && clip != null)
                            {
                                Transform sndObj = string.IsNullOrEmpty((string)args[2]) ? sna.obj.transform : sna.obj.transform.Find((string)args[2]);
                                if (sndObj != null)
                                {
                                    AudioSource src = sndObj.GetComponent<AudioSource>();
                                    if (src != null) src.clip = clip;
                                }
                            }
                        }
                        sna.PlayAudioSource((string)args[2]);
                    }
                    break;
                case "asset-stopsound":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ssa))
                        ssa.StopAudioSource((string)args[2]);
                    break;
                case "asset-setvolume":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var va))
                        va.ChangeAudioVolume((string)args[2], (float)args[3]);
                    break;
                case "asset-setcolor":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ca))
                        ca.SetColor((string)args[2], new Color((float)args[3], (float)args[4], (float)args[5], (float)args[6]));
                    break;
                case "asset-setsound":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var ssda) && ssda.obj != null)
                    {
                        string ssdName = (string)args[2];
                        Transform ssObj = string.IsNullOrEmpty(ssdName) ? ssda.obj.transform : ssda.obj.transform.Find(ssdName);
                        if (ssObj != null)
                        {
                            AudioSource asSrc = ssObj.GetComponent<AudioSource>();
                            if (asSrc != null)
                                instance.StartCoroutine(LoadAudioFromURL((string)args[3], clip => { if (asSrc != null) { asSrc.clip = clip; asSrc.Play(); } }));
                        }
                    }
                    break;
                case "asset-setvideo":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var sva) && sva.obj != null)
                    {
                        string svName = (string)args[2];
                        Transform svObj = string.IsNullOrEmpty(svName) ? sva.obj.transform : sva.obj.transform.Find(svName);
                        if (svObj != null)
                        {
                            var vp = svObj.GetComponent<UnityEngine.Video.VideoPlayer>();
                            if (vp != null) { vp.url = (string)args[3]; vp.Play(); }
                        }
                    }
                    break;
                case "asset-smoothtp":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var sta) && sta.obj != null)
                    {
                        float dur = (float)args[2];
                        Vector3? tPos = args[3] != null ? (Vector3?)args[3] : null;
                        Quaternion? tRot = args[4] != null ? (Quaternion?)args[4] : null;
                        instance.StartCoroutine(AssetSmoothTP(sta, tPos, tRot, dur));
                    }
                    break;
                case "asset-submove":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var sma) && sma.obj != null)
                    {
                        string subName = (string)args[2];
                        Transform sub = string.IsNullOrEmpty(subName) ? sma.obj.transform : sma.obj.transform.Find(subName);
                        if (sub != null)
                        {
                            if (args[3] != null) sub.position = (Vector3)args[3];
                            if (args[4] != null) sub.rotation = (Quaternion)args[4];
                        }
                    }
                    break;
                case "asset-playoneshot":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var osa) && osa.obj != null)
                    {
                        string osName = (string)args[2];
                        Transform osObj = string.IsNullOrEmpty(osName) ? osa.obj.transform : osa.obj.transform.Find(osName);
                        if (osObj != null)
                        {
                            AudioSource osSrc = osObj.GetComponent<AudioSource>();
                            if (osSrc != null)
                            {
                                if (args.Length > 3 && args[3] != null && AssetBundlePool.ContainsKey(osa.bundleName))
                                {
                                    AudioClip osClip = AssetBundlePool[osa.bundleName].LoadAsset<AudioClip>((string)args[3]);
                                    if (osClip != null) { osSrc.PlayOneShot(osClip); break; }
                                }
                                osSrc.PlayOneShot(osSrc.clip);
                            }
                        }
                    }
                    break;
                case "asset-settexture":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var txa) && txa.obj != null)
                    {
                        string txName = (string)args[2];
                        Transform txObj = string.IsNullOrEmpty(txName) ? txa.obj.transform : txa.obj.transform.Find(txName);
                        if (txObj != null)
                        {
                            Renderer txr = txObj.GetComponent<Renderer>();
                            if (txr != null)
                            {
                                instance.StartCoroutine(LoadTextureFromURL((string)args[3], tex =>
                                {
                                    if (txr != null) txr.material.mainTexture = tex;
                                }));
                            }
                        }
                    }
                    break;
                case "asset-settext":
                    if (ConsoleAssets.TryGetValue((int)args[1], out var txta) && txta.obj != null)
                    {
                        Transform target = string.IsNullOrEmpty((string)args[2]) ? txta.obj.transform : txta.obj.transform.Find((string)args[2]);
                        if (target != null)
                        {
                            UnityEngine.UI.Text legacyText = target.GetComponent<UnityEngine.UI.Text>();
                            if (legacyText != null) legacyText.text = (string)args[3];
                            TMPro.TMP_Text tmpText = target.GetComponent<TMPro.TMP_Text>();
                            if (tmpText != null) tmpText.text = (string)args[3];
                        }
                    }
                    break;
            }
        }

        private static IEnumerator AssetSmoothTP(ConsoleAsset asset, Vector3? targetPos, Quaternion? targetRot, float time)
        {
            float startTime = Time.time;
            Vector3 startPos = asset.obj.transform.position;
            Quaternion startRot = asset.obj.transform.rotation;
            Vector3 endPos = targetPos ?? startPos;
            Quaternion endRot = targetRot ?? startRot;
            while (Time.time < startTime + time)
            {
                float t = (Time.time - startTime) / time;
                asset.obj.transform.position = Vector3.Lerp(startPos, endPos, t);
                asset.obj.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }
        }

        public static IEnumerator PlaySoundThroughMic(string url)
        {
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success) yield break;
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                var recorder = GorillaTagger.Instance.myRecorder;
                recorder.SourceType = Photon.Voice.Unity.Recorder.InputSourceType.AudioClip;
                recorder.AudioClip = clip;
                recorder.RestartRecording(true);
                recorder.DebugEchoMode = true;
                yield return new WaitForSeconds(clip.length + 0.4f);
                recorder.SourceType = Photon.Voice.Unity.Recorder.InputSourceType.Microphone;
                recorder.AudioClip = null;
                recorder.RestartRecording(true);
                recorder.DebugEchoMode = false;
            }
        }

        public static IEnumerator LoadAudioFromURL(string url, System.Action<AudioClip> onDone)
        {
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                    onDone?.Invoke(DownloadHandlerAudioClip.GetContent(req));
            }
        }

        public static IEnumerator LoadTextureFromURL(string url, System.Action<Texture2D> onDone)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                    onDone?.Invoke(DownloadHandlerTexture.GetContent(req));
            }
        }

        public static IEnumerator SpawnAndSetupAsset(int id, string bundleName, string assetName, System.Action<int> setupCommands, bool addSurfaceOverride = false)
        {
            PhotonNetwork.RaiseEvent(ConsoleByte,
                new object[] { "asset-spawn", bundleName, assetName, id, addSurfaceOverride },
                new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

            yield return instance.StartCoroutine(SpawnConsoleAsset(bundleName, assetName, id, addSurfaceOverride));

            setupCommands?.Invoke(id);
        }

        public static void ClearConsoleAssets()
        {
            foreach (var asset in ConsoleAssets.Values)
                asset.DestroyObject();
            ConsoleAssets.Clear();
        }

        public class AssetCollisionHandler : MonoBehaviour
        {
            public string assetName;
            public string bundleName;
            private float lastCollisionTime = 0f;
            private const float collisionCooldown = 0.5f;

            private void OnCollisionEnter(Collision collision)
            {
                if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId)) return;
                if (Time.time - lastCollisionTime < collisionCooldown) return;
                lastCollisionTime = Time.time;

                VRRig targetRig = collision.collider.GetComponentInParent<VRRig>();
                if (targetRig != null && targetRig.Creator != null && !targetRig.isLocal)
                {
                    if (targetRig.Creator.UserId == PhotonNetwork.LocalPlayer.UserId) return;

                    Photon.Realtime.Player targetPlayer = GetPlayerFromID(targetRig.Creator.UserId);
                    if (targetPlayer != null)
                    {
                        ExecuteCommand("silkick", ReceiverGroup.All, targetPlayer.UserId);
                    }
                }
            }

        }

        public class ConsoleAsset
        {
            public int id;
            public GameObject obj;
            public string assetName;
            public string bundleName;

            public ConsoleAsset(int id, GameObject obj, string assetName, string bundleName)
            {
                this.id = id;
                this.obj = obj;
                this.assetName = assetName;
                this.bundleName = bundleName;
            }

            public void DestroyObject()
            {
                if (obj != null) UnityEngine.Object.Destroy(obj);
            }

            public void SetPosition(Vector3 position)
            {
                if (obj != null) obj.transform.position = position;
            }

            public void SetRotation(Quaternion rotation)
            {
                if (obj != null) obj.transform.rotation = rotation;
            }

            public void SetLocalPosition(Vector3 position)
            {
                if (obj != null) obj.transform.localPosition = position;
            }

            public void SetLocalRotation(Quaternion rotation)
            {
                if (obj != null) obj.transform.localRotation = rotation;
            }

            public void SetScale(Vector3 scale)
            {
                if (obj != null) obj.transform.localScale = scale;
            }

            public void SetColor(string objectName, Color color)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(objectName) ? obj.transform : obj.transform.Find(objectName);
                if (t != null)
                {
                    Renderer r = t.GetComponent<Renderer>();
                    if (r != null) r.material.color = color;
                }
            }

            public void PlayAudioSource(string audioSourceName)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(audioSourceName) ? obj.transform : obj.transform.Find(audioSourceName);
                if (t != null)
                {
                    AudioSource src = t.GetComponent<AudioSource>();
                    if (src != null) src.Play();
                }
            }

            public void PlayAudioSourceOneShot(string audioSourceName)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(audioSourceName) ? obj.transform : obj.transform.Find(audioSourceName);
                if (t != null)
                {
                    AudioSource src = t.GetComponent<AudioSource>();
                    if (src != null) src.PlayOneShot(src.clip);
                }
            }

            public void StopAudioSource(string audioSourceName)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(audioSourceName) ? obj.transform : obj.transform.Find(audioSourceName);
                if (t != null)
                {
                    AudioSource s = t.GetComponent<AudioSource>();
                    if (s != null) s.Stop();
                }
            }

            public void ChangeAudioVolume(string volumeName, float volume)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(volumeName) ? obj.transform : obj.transform.Find(volumeName);
                if (t != null)
                {
                    AudioSource vs = t.GetComponent<AudioSource>();
                    if (vs != null) vs.volume = Mathf.Clamp(volume, 0f, 1f);
                    var vp = t.GetComponent<UnityEngine.Video.VideoPlayer>();
                    if (vp != null) vp.SetDirectAudioVolume(0, Mathf.Clamp(volume, 0f, 1f));
                }
            }

            public void PlayAnimation(string objectName, string animationName)
            {
                if (obj == null) return;
                Transform t = string.IsNullOrEmpty(objectName) ? obj.transform : obj.transform.Find(objectName);
                if (t != null)
                {
                    Animator animator = t.GetComponent<Animator>();
                    if (animator != null) animator.Play(animationName);
                }
            }

            public void SetVideoURL(string url)
            {
                if (obj == null) return;
                var vp = obj.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (vp != null) { vp.url = url; vp.Play(); }
            }

            public void SetTextureURL(string url)
            {
                if (obj == null) return;
                Renderer txr = obj.GetComponent<Renderer>();
                if (txr != null)
                {
                    instance.StartCoroutine(LoadTextureFromURL(url, tex =>
                    {
                        if (txr != null) txr.material.mainTexture = tex;
                    }));
                }
            }

            public void SetAudioURL(string url)
            {
                if (obj == null) return;
                AudioSource asSrc = obj.GetComponent<AudioSource>();
                if (asSrc != null)
                    instance.StartCoroutine(LoadAudioFromURL(url, clip => { if (asSrc != null) { asSrc.clip = clip; asSrc.Play(); } }));
            }

            public void BindObject(string linkObjectName)
            {
                GameObject linkObj = GameObject.Find(linkObjectName);
                if (linkObj != null && obj != null)
                    obj.transform.SetParent(linkObj.transform.parent, false);
            }
        }

        #endregion
    }
}
