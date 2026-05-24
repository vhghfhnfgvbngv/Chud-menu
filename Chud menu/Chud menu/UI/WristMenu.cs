using GTAG_NotificationLib;
using MalachiTemp.Backend;
using MalachiTemp.Classes;
using MalachiTemp.UI;
using MalachiTemp.Utilities;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace MalachiTemp.UI
{
    internal class WristMenu : MonoBehaviour
    {
        public static string MenuTitle = "Chud Menu";
        public static Font MenuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        #region Main
        public static void InitCategories()
        {
            MenuManager.AddCategory("Main", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Settings", method =() => MenuManager.ToggleCategory("Settings"), enabled = false, nontoggleable = true, toolTip = "Go to Settings"},
                new ButtonInfo { buttonText = "Movement Mods", method =() => MenuManager.ToggleCategory("Movement Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Movement Mods"},
                new ButtonInfo { buttonText = "Visual Mods", method =() => MenuManager.ToggleCategory("Visual Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Visual Mods"},
                new ButtonInfo { buttonText = "Fun Mods", method =() => MenuManager.ToggleCategory("Fun Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Fun Mods"},
                new ButtonInfo { buttonText = "Useful Mods", method =() => MenuManager.ToggleCategory("Useful Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Useful Mods"},
                new ButtonInfo { buttonText = "Rig Mods", method =() => MenuManager.ToggleCategory("Rig Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Rig Mods"},
                new ButtonInfo { buttonText = "Infection Mods", method =() => MenuManager.ToggleCategory("Infection Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Infection Mods"},
                new ButtonInfo { buttonText = "Master Mods", method =() => MenuManager.ToggleCategory("Master Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Master Mods"},
                new ButtonInfo { buttonText = "Credits", method =() => MenuManager.ToggleCategory("Credits"), enabled = false, nontoggleable = true, toolTip = "Go to Credits"},
            });

            // Settings
            MenuManager.AddCategory("Settings", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Settings", method =() => MenuManager.ToggleCategory("Settings"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Toggle Notifications", method =() => Mods.ToggleNotifications(), disableMethod =() => Mods.DisableNotifications(), enabled = true, toolTip = "Toggle notifications on/off"},
                new ButtonInfo { buttonText = "Clear Notifications", method =() => Mods.ClearNotifications(), enabled = false, nontoggleable = true, toolTip = "Clear all notifications on screen"},
                new ButtonInfo { buttonText = "Toggle Anti Cheat Reports", method =() => Mods.ToggleAntiCheatReports(), disableMethod =() => Mods.DisableAntiCheatReports(), enabled = false, toolTip = "Toggle anti cheat report notifications"},
                new ButtonInfo { buttonText = "Toggle White Guns", method =() => Mods.ToggleWhiteGuns(), disableMethod =() => Mods.DisableWhiteGuns(), enabled = false, toolTip = "Toggle gun color between black and white"},
                new ButtonInfo { buttonText = "Change Speed Amount", method =() => Mods.ChangeSpeedBoostAmount(), enabled = false, nontoggleable = true, toolTip = "Change speed boost amount"},
                new ButtonInfo { buttonText = "Change Fly Speed", method =() => Mods.ChangeFlySpeed(), enabled = false, nontoggleable = true, toolTip = "Change fly speed up to 20"},
                new ButtonInfo { buttonText = "Change WASD Sense", method =() => Mods.ChangeWASDFlyMouseSense(), enabled = false, nontoggleable = true, toolTip = "Change WASD fly mouse sensitivity"},
                new ButtonInfo { buttonText = "Change Pull Power", method =() => Mods.ChangePullModPower(), enabled = false, nontoggleable = true, toolTip = "Change pull mod power"},
            });

            // Movement Mods
            MenuManager.AddCategory("Movement Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Movement Mods", method =() => MenuManager.ToggleCategory("Movement Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Speed Boost", method =() => Mods.SpeedBoost(), disableMethod =() => Mods.DisableSpeedBoost(), enabled = false, toolTip = "Increased movement speed"},
                new ButtonInfo { buttonText = "Joystick Fly", method =() => Mods.JoystickFly(Mods.flySpeed), disableMethod =() => Mods.DisableJoystickFly(), enabled = false, toolTip = "Fly with joystick."},
                new ButtonInfo { buttonText = "WASD Fly", method =() => Mods.EnableWASDFly(), disableMethod =() => Mods.DisableWASDFly(), enabled = false, toolTip = "Fly with WASD keys"},
                new ButtonInfo { buttonText = "No Gravity", method =() => Mods.NoGravity(), disableMethod =() => Mods.DisableNoGravity(), enabled = false, toolTip = "Disable gravity"},
                new ButtonInfo { buttonText = "Noclip", method =() => Mods.Noclip(), disableMethod =() => Mods.NoclipOff(), enabled = false, toolTip = "Hold B button to noclip"},
                new ButtonInfo { buttonText = "Platforms", method =() => Mods.Platforms(), enabled = false, toolTip = "Place platforms"},
                new ButtonInfo { buttonText = "TP Gun", method =() => Mods.TPGun(), enabled = false, toolTip = "Shoot to teleport yourself"},
                new ButtonInfo { buttonText = "Pull Mod", method =() => Mods.PullMod(), enabled = false, toolTip = "Pull yourself forward while gripping"},
                new ButtonInfo { buttonText = "Minos Prime", method =() => Mods.MinosPrime(), disableMethod =() => Mods.DisableMinosPrime(), enabled = false, toolTip = "Right B to jump, then Right A to slam"},
            });

            // Visual Mods
            MenuManager.AddCategory("Visual Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Visual Mods", method =() => MenuManager.ToggleCategory("Visual Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Name Tags", method =() => Mods.NameTags(), disableMethod =() => Mods.DisableNameTags(), enabled = false, toolTip = "Show player names above heads"},
                new ButtonInfo { buttonText = "Tracers", method =() => Mods.Tracers(), disableMethod =() => Mods.DisableTracers(), enabled = false, toolTip = "Lines towards everyone"},
                new ButtonInfo { buttonText = "2D Box ESP", method =() => Mods.BoxEspRender(), disableMethod =() => Mods.DisableBoxEsp(), enabled = false, toolTip = "Boxes around players through walls"},
                new ButtonInfo { buttonText = "FPS Name Tags", method =() => Mods.FPSTags(), disableMethod =() => Mods.DisableFPSTags(), enabled = false, toolTip = "Show player FPS above heads"},
                new ButtonInfo { buttonText = "ID Name Tags", method =() => Mods.IDTags(), disableMethod =() => Mods.DisableIDTags(), enabled = false, toolTip = "Show player IDs above heads"},
                new ButtonInfo { buttonText = "Cosmetic Name Tags", method =() => Mods.CosmeticNameTags(), disableMethod =() => Mods.DisableCosmeticNameTags(), enabled = false, toolTip = "Show owned cosmetics above heads in red"},
                new ButtonInfo { buttonText = "Cosmetic Notifier", method =() => Mods.CosmeticNotifier(), disableMethod =() => Mods.DisableCosmeticNotifier(), enabled = false, toolTip = "Notify when a player has tracked cosmetics"},
            });

            // Useful Mods
            MenuManager.AddCategory("Useful Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Useful Mods", method =() => MenuManager.ToggleCategory("Useful Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Anti Name Ban", method =() => Mods.AntiNameBan(), disableMethod =() => Mods.DisableAntiNameBan(), enabled = true, toolTip = "Prevents you from getting banned for setting your name to bad things"},
                new ButtonInfo { buttonText = "Anti AFK", method =() => Mods.AntiAFK(), disableMethod =() => Mods.DisableAntiAFK(), enabled = false, toolTip = "Prevents AFK kick"},
                new ButtonInfo { buttonText = "Anti Guardian Grab", method =() => Mods.AntiGuardianGrab(), disableMethod =() => Mods.DisableAntiGuardianGrab(), enabled = false, toolTip = "Blocks guardian knockback, grabs, and throws"},
                new ButtonInfo { buttonText = "Block jman sounds", method =() => Mods.BlockJmanSounds(), disableMethod =() => Mods.DisableBlockJmanSounds(), enabled = false, toolTip = "Block handtap sounds 336-338 from playing"},
                new ButtonInfo { buttonText = "Mute Gun", method =() => Mods.MuteGun(), enabled = false, toolTip = "Shoot a player to mute/unmute them"},
                new ButtonInfo { buttonText = "Get ID Self", method =() => Mods.GetIDSelf(), enabled = false, nontoggleable = true, toolTip = "Copy your own PlayerID to clipboard"},
                new ButtonInfo { buttonText = "Disable Network Triggers", method =() => Mods.EnableDisableNetworkTriggers(), disableMethod =() => Mods.DisableDisableNetworkTriggers(), enabled = false, toolTip = "Change maps without disconnecting"},
                new ButtonInfo { buttonText = "Disable Quit Box", method =() => Mods.EnableDisableQuitBox(), disableMethod =() => Mods.DisableDisableQuitBox(), enabled = false, toolTip = "Disables the quit box under the map"},
                new ButtonInfo { buttonText = "Join Code MODS", method =() => Mods.JoinCode("MODS"), enabled = false, nontoggleable = true, toolTip = "Join room with code MODS"},
                new ButtonInfo { buttonText = "Join Code MOD", method =() => Mods.JoinCode("MOD"), enabled = false, nontoggleable = true, toolTip = "Join room with code MOD"},
            });

            // Fun Mods
            MenuManager.AddCategory("Fun Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Fun Mods", method =() => MenuManager.ToggleCategory("Fun Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Unlock VIM/Subscription", method =() => Mods.UnlockVim(), disableMethod =() => Mods.DisableUnlockVim(), enabled = false, toolTip = "Unlock VIM subscription features"},
                new ButtonInfo { buttonText = "Bitcrunch Mic", method =() => Mods.BitcrunchMic(), disableMethod =() => Mods.DisableBitcrunchMic(), enabled = false, toolTip = "Makes your mic sound bitcrushed and bad"},
                new ButtonInfo { buttonText = "GetPlayerID Gun", method =() => Mods.GetPlayerIDGun(), enabled = false, toolTip = "Shoot a player to copy their PlayerID to clipboard"},
                new ButtonInfo { buttonText = "Launch Player Gun", method =() => Mods.LaunchPlayerGun(), enabled = false, toolTip = "Teleport yourself 10 frames wherever you shoot"},
            });

            // Rig Mods
            MenuManager.AddCategory("Rig Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Rig Mods", method =() => MenuManager.ToggleCategory("Rig Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Ghost Monke", method =() => Mods.GhostMonke(), disableMethod =() => Mods.DisableGhostMonke(), enabled = false, toolTip = "Press B to freeze your rig"},
                new ButtonInfo { buttonText = "Invis Monke", method =() => Mods.InvisMonke(), disableMethod =() => Mods.DisableInvisMonke(), enabled = false, toolTip = "Press B to be invisible"},
                new ButtonInfo { buttonText = "Flail Hands", method =() => Mods.Flail(), disableMethod =() => Mods.DisableFlail(), enabled = false, toolTip = "Hold Right A to flail your arms wildly you friggen weirdo"},
                new ButtonInfo { buttonText = "Spaz Body", method =() => Mods.SpazBody(), disableMethod =() => Mods.DisableSpazBody(), enabled = false, toolTip = "Spaz your entire body uncontrollably"},
            });

            // Infection Mods
            MenuManager.AddCategory("Infection Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Infection Mods", method =() => MenuManager.ToggleCategory("Infection Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Tag Gun", method =() => Mods.TagGun(), enabled = false, toolTip = "Shoot a player to tag them"},
            });

            // Master Mods
            MenuManager.AddCategory("Master Mods", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Master Mods", method =() => MenuManager.ToggleCategory("Master Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Not master client", method = null, enabled = false, nontoggleable = true, toolTip = "Your current master client status"},
                new ButtonInfo { buttonText = "Tag While Not Tagged", method =() => Mods.TagWhileNotTagged(), enabled = false, toolTip = "tag while not infected"},
                new ButtonInfo { buttonText = "Untag Self", method =() => Mods.UntagSelf(), enabled = false, nontoggleable = true, toolTip = "Remove yourself from infected list"},
                new ButtonInfo { buttonText = "Spaz All", method =() => Mods.SpazAll(), disableMethod =() => Mods.DisableSpazAll(), enabled = false, toolTip = "Toggle all players it/infected every 10 frames"},
                new ButtonInfo { buttonText = "Spaz Self", method =() => Mods.SpazSelf(), disableMethod =() => Mods.DisableSpazSelf(), enabled = false, toolTip = "Toggle self it/infected every 10 frames"},
                new ButtonInfo { buttonText = "Untag Gun", method =() => Mods.UntagGun(), enabled = false, toolTip = "Shoot infected players to untag them"},
            });

            // Admin Mods (added dynamically when admin detected)
            MenuManager.AddCategory("Admin Mods");

            // Console Settings
            MenuManager.AddCategory("Console Settings", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Console Settings", method =() => MenuManager.ToggleCategory("Console Settings"), enabled = false, nontoggleable = true, toolTip = "Back to Admin Mods"},
                new ButtonInfo { buttonText = "Allow Kick Self", method =() => Mods.ToggleAllowKickSelf(), disableMethod =() => Mods.DisableAllowKickSelf(), enabled = false, toolTip = "Allow other admins to kick/tp/fling you"},
                new ButtonInfo { buttonText = "Allow Teleport Self", method =() => Mods.ToggleAllowTpSelf(), disableMethod =() => Mods.DisableAllowTpSelf(), enabled = true, toolTip = "Allow other admins to teleport you"},
                new ButtonInfo { buttonText = "Detect Console Users", method =() => Mods.DetectConsoleUsers(), disableMethod =() => Mods.DisableDetectConsoleUsers(), enabled = false, toolTip = "Auto detect who has console"},
                new ButtonInfo { buttonText = "Full Auto Pistol", method =() => Mods.ToggleFullAutoPistol(), disableMethod =() => Mods.DisableFullAutoPistol(), enabled = false, toolTip = "Toggle full auto mode for pistol"},
                new ButtonInfo { buttonText = "No Admin Indicator", method =() => Mods.ToggleNoAdminIndicator(), disableMethod =() => Mods.DisableNoAdminIndicator(), enabled = false, toolTip = "Hide your admin crown"},
                new ButtonInfo { buttonText = "Change Laser Color", method =() => Mods.CycleLaserColor(), enabled = false, nontoggleable = true, toolTip = "Change laser color"},
                new ButtonInfo { buttonText = "Notify Presence", method =() => Mods.NotifyPresence(), enabled = false, nontoggleable = true, toolTip = "Announce you're in the lobby"},
                new ButtonInfo { buttonText = "Asset Positioner", method =() => Mods.ToggleAssetPositioner(), disableMethod =() => Mods.DisableAssetPositioner(), enabled = false, toolTip = "Left grip to grab nearest asset; left trigger shrink, right trigger grow" },
            });

            // Credits
            MenuManager.AddCategory("Credits", new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Credits", method =() => MenuManager.ToggleCategory("Credits"), enabled = false, nontoggleable = true, toolTip = "Go to Main"},
                new ButtonInfo { buttonText = "Jolyne", method = null, enabled = false, nontoggleable = true, toolTip = "Menu owner"},
                new ButtonInfo { buttonText = "Malachi", method = null, enabled = false, nontoggleable = true, toolTip = "Temp creator"},
                new ButtonInfo { buttonText = "Big Pickle AI", method = null, enabled = false, nontoggleable = true, toolTip = "Made most of the mods on the menu"},
                new ButtonInfo { buttonText = "Seralyth", method = null, enabled = false, nontoggleable = true, toolTip = "has skidded code from Seralyth"},
            });

        }
        #endregion
        #region Other Stuff
        public static string[] CustomBoardTexts = new string[]
        {
            "CHUD MENU USERS, LOOK HERE",
            "CHUD MENU",
            "Monkeys can climb. Crickets can leap. Horses can race. Owls can seek. Cheetahs can run. Eagles can fly. People can try. But that's about it.",
            "if u get banned with this, its on u, not me"
        };
        public static string FolderName = "Chud Menu";
        #endregion 
        #region Colors
        public static bool ChangingColors = false;
        public static Color FirstColor = Color.blue;
        public static Color SecondColor = Color.black;
        public static Color NormalColor = Color.black;
        public static Color ButtonColorDisable = Color.white;
        public static Color ButtonColorEnabled = Color.gray;
        public static Color EnableTextColor = Color.white;
        public static Color DIsableTextColor = Color.black;
        public static Color MenuTitleColor = Color.white;
        public static Color ToolTipColor = Color.white;
        public static Color DisconnectButtonColor = Color.red;
        public static Color DisconnectTextColor = Color.white;
        public static Color NextPrevButtonColor = Color.black;
        public static Color NextPrevTextColor = Color.white;
        #endregion
        #region Scales
        public static Vector3 MenuScale = new Vector3(0.1f, 1f, 1f) * 1f;
        public static Vector3 MenuPos = new Vector3(0.05f, 0f, 0f) * 1f;
        public static Vector3 PointerScale = new Vector3(0.01f, 0.01f, 0.01f);
        public static Vector3 PointerPos = new Vector3(0f, -0.1f, 0f);
        public static Vector3 ToolTipPos = new Vector3(0.06f, 0f, -0.18f) * 1f;
        public static Vector2 ToolTipScale = new Vector2(0.2f, 0.03f) * 1f;
        public static Vector3 MenuTitlePos = new Vector3(0.06f, 0f, 0.175f);
        public static Vector2 MenuTitleScale = new Vector2(0.28f, 0.05f);
        public static Vector3 ButtonScale = new Vector3(0.09f, 0.8f, 0.08f);
        public static Vector2 ButtonTextScale = new Vector2(0.2f, 0.03f) * 1f;
        #endregion
        #region Controller Inputs
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
        #endregion
        #region Menu Stuff
        public static int lastPressedButtonIndex = -1;
        public static GameObject menu = null;
        public static GameObject canvasObj = null;
        public static GameObject reference = null;
        public static int pageNumber = 0;
        public static WristMenu instance;
        public static GameObject menuObj;
        public static Text fpsText;
        private static DateTime sessionStartTime;
        private static string bottomBarStr = "FPS: 0 | 12:00 AM | 0:00";
        private static float fpsAccumulator;
        private static int fpsFrameCount;
        public static bool toggle = false;
        public static bool toggle1 = false;
        public static int pageSize = 4;
        public static int ClickCooldown = 10;
        public static bool custom = true;
        private static int _frameCounter;
        private static bool _adminInitialized;
        void Update()
        {
            try
            {
                gripDownL = ControllerInputPoller.instance.leftGrab;
                gripDownR = ControllerInputPoller.instance.rightGrab;
                triggerDownL = ControllerInputPoller.instance.leftControllerIndexFloat == 1f;
                triggerDownR = ControllerInputPoller.instance.rightControllerIndexFloat == 1f;
                abuttonDown = ControllerInputPoller.instance.rightControllerPrimaryButton;
                bbuttonDown = ControllerInputPoller.instance.rightControllerSecondaryButton;
                xbuttonDown = ControllerInputPoller.instance.leftControllerPrimaryButton;
                ybuttonDown = ControllerInputPoller.instance.leftControllerSecondaryButton;
                joy = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
                joyL = ControllerInputPoller.instance.leftControllerPrimary2DAxis;

                Mods.UpdateAdminGrab();
                Mods.UpdateActiveMods();
                Mods.LaserUpdate();
                Mods.AssetInteractionUpdate();

                if (Mods.change7 == 5 && menu != null && !menu.GetComponent<Rigidbody>())
                    HandleTriggerPageNav();

                HandleMenuFollow();

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
                    int avgFps = Mathf.RoundToInt(fpsFrameCount / fpsAccumulator);
                    TimeSpan elapsed = DateTime.Now - sessionStartTime;
                    bottomBarStr = "FPS: " + avgFps + " | " + DateTime.Now.ToString("h:mm tt") + " | " + (int)elapsed.TotalMinutes + ":" + elapsed.Seconds.ToString("D2");
                    fpsAccumulator = 0f;
                    fpsFrameCount = 0;
                }

                if (menu != null && fpsText != null)
                    fpsText.text = bottomBarStr;

                if (!Directory.Exists(FolderName))
                    Directory.CreateDirectory(FolderName);

                if (custom)
                    UpdateCustomBoardText();
            }
            catch { }
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
            else toggle = false;

            if (triggerDownR)
            {
                if (!toggle1)
                {
                    Toggle("NextPage");
                    VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f);
                    toggle1 = true;
                }
            }
            else toggle1 = false;
        }
        private void HandleMenuFollow()
        {
            if (ybuttonDown && !Mods.right)
            {
                if (menu == null) { instance.Draw(); return; }
                menu.transform.position = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform.position;
                menu.transform.rotation = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform.rotation;
                if (reference == null)
                {
                    reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    reference.name = "buttonPresser";
                }
                reference.transform.parent = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform;
                reference.transform.localPosition = PointerPos;
                reference.transform.localScale = PointerScale;
                reference.GetComponent<Renderer>().material.color = ChangingColors ? FirstColor : NormalColor;
                if (menu.GetComponent<Rigidbody>())
                    Destroy(menu.GetComponent<Rigidbody>());
            }
            else if (!ybuttonDown && !Mods.right && menu != null && !menu.GetComponent<Rigidbody>())
            {
                Destroy(reference); reference = null;
                var rb = menu.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = GorillaLocomotion.GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0f, false);
            }

            if (bbuttonDown && Mods.right)
            {
                if (menu == null) { instance.Draw(); return; }
                menu.transform.position = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                menu.transform.rotation = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.rotation;
                menu.transform.RotateAround(menu.transform.position, menu.transform.forward, 180f);
                if (reference == null)
                {
                    reference = GameObject.CreatePrimitive(0);
                    reference.name = "buttonPresser";
                }
                reference.transform.parent = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform;
                reference.transform.localPosition = PointerPos;
                reference.transform.localScale = PointerScale;
                reference.GetComponent<Renderer>().material.color = ChangingColors ? FirstColor : NormalColor;
                if (menu.GetComponent<Rigidbody>())
                    Destroy(menu.GetComponent<Rigidbody>());
            }
            else if (!abuttonDown && Mods.right && menu != null && !menu.GetComponent<Rigidbody>())
            {
                Destroy(reference); reference = null;
                var rb = menu.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f, false);
            }
        }
        private void UpdateMasterClientStatus()
        {
            var masterCat = MenuManager.Categories.Find(c => c.Name == "Master Mods");
            if (masterCat == null || masterCat.Buttons.Count <= 1) return;
            bool isMaster = PhotonNetwork.IsMasterClient;
            masterCat.Buttons[1].buttonText = isMaster ? "You are master client" : "Not master client";
            masterCat.Buttons[1].toolTip = isMaster ? "You are the master client" : "You are not the master client";
            if (!isMaster)
            {
                for (int i = 2; i < masterCat.Buttons.Count; i++)
                {
                    var btn = masterCat.Buttons[i];
                    if (btn.enabled.GetValueOrDefault())
                    {
                        if (btn.disableMethod != null) try { btn.disableMethod(); } catch { }
                        btn.enabled = false;
                    }
                }
            }
        }
        private void CheckAdminStatus()
        {
            bool isAdmin = PhotonNetwork.LocalPlayer != null && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
            bool adminCatExists = MenuManager.Categories.Any(c => c.Name == "Admin Mods");
            bool adminMainBtnExists = false;
            var mainCat = MenuManager.Categories.Find(c => c.Name == "Main");
            if (mainCat != null)
                adminMainBtnExists = mainCat.Buttons.Any(b => b.buttonText == "Admin Mods");

            if (isAdmin && adminCatExists && !adminMainBtnExists)
            {
                string adminName = ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId];
                if (!_adminInitialized)
                {
                    NotifiLib.SendNotification("[<color=green>CHUD</color>] Welcome " + adminName, 3);
                    _adminInitialized = true;
                }
                var adminCat = MenuManager.Categories.Find(c => c.Name == "Admin Mods");
                if (adminCat != null && adminCat.Buttons.Count == 0)
                {
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Exit Admin Mods", method =() => MenuManager.ToggleCategory("Admin Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Main" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Kick Gun", method =() => Mods.KickGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot a player to kick them" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Silent Kick Gun", method =() => Mods.SilentKickGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot a player to silently kick them" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Fling Gun", method =() => Mods.FlingGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot a player to fling them" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Vibrate Gun", method =() => Mods.VibrateGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot a player to vibrate their controllers" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "TP All Gun", method =() => Mods.TPAllGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot to TP everyone to that spot" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Lightning Gun", method =() => Mods.LightningGun(), disableMethod =() => Mods.CleanupGun(), enabled = false, toolTip = "Shoot to strike lightning" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Jail Gun", method =() => Mods.JailGun(), disableMethod =() => Mods.JailGunOff(), enabled = false, toolTip = "Trap players in a jail cell" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Admin Grab", method =() => Mods.AdminGrab(), disableMethod =() => Mods.AdminGrabOff(), enabled = false, toolTip = "Grab players with your hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Laser", method =() => Mods.Laser(), disableMethod =() => Mods.DisableLaser(), enabled = false, toolTip = "Toggle lasers from your hands" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Kick All", method =() => Mods.KickAll(), enabled = false, nontoggleable = true, toolTip = "Kick everyone from lobby" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Notify All", method =() => Mods.NotifyAll(), enabled = false, nontoggleable = true, toolTip = "Send a notification to everyone" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Karambit", method =() => Mods.SpawnKarambit(), disableMethod =() => Mods.DisableSpawnKarambit(), enabled = false, toolTip = "Spawn/despawn karambit in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Knife", method =() => Mods.SpawnKnife(), disableMethod =() => Mods.DisableSpawnKnife(), enabled = false, toolTip = "Spawn/despawn knife in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Rblx Carpet", method =() => Mods.SpawnRblxCarpet(), disableMethod =() => Mods.DisableSpawnRblxCarpet(), enabled = false, toolTip = "Spawn/despawn rblx carpet in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn MC Sword", method =() => Mods.SpawnMcSword(), disableMethod =() => Mods.DisableSpawnMcSword(), enabled = false, toolTip = "Spawn/despawn minecraft sword in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Ban Hammer", method =() => Mods.SpawnBanHammer(), disableMethod =() => Mods.DisableSpawnBanHammer(), enabled = false, toolTip = "Spawn/despawn ban hammer in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Roblox Sword", method =() => Mods.SpawnRobloxSword(), disableMethod =() => Mods.DisableSpawnRobloxSword(), enabled = false, toolTip = "Spawn/despawn roblox sword in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Rainbow Sword", method =() => Mods.SpawnRainbowSword(), disableMethod =() => Mods.DisableSpawnRainbowSword(), enabled = false, toolTip = "Spawn/despawn rainbow sword in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Pistol", method =() => Mods.SpawnPistol(), disableMethod =() => Mods.DisableSpawnPistol(), enabled = false, toolTip = "Spawn/despawn pistol in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Physics Gun", method =() => Mods.SpawnPhysicsGun(), disableMethod =() => Mods.DisableSpawnPhysicsGun(), enabled = false, toolTip = "Spawn/despawn physics gun in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Bag", method =() => Mods.SpawnBag(), disableMethod =() => Mods.DisableSpawnBag(), enabled = false, toolTip = "Spawn/despawn bag in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Kormakur", method =() => Mods.SpawnKormakur(), disableMethod =() => Mods.DisableSpawnKormakur(), enabled = false, toolTip = "Hold Kormakur sign in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Coin", method =() => Mods.SpawnCoin(), disableMethod =() => Mods.DisableSpawnCoin(), enabled = false, toolTip = "Spawn coin flip in right hand" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Boombox", method =() => Mods.SpawnBoombox(), disableMethod =() => Mods.DisableSpawnBoombox(), enabled = false, toolTip = "Spawn boombox (URL from clipboard)" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Samsung", method =() => Mods.SpawnSamsung(), disableMethod =() => Mods.DisableSpawnSamsung(), enabled = false, toolTip = "Samsung phone (video URL from clipboard)" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Video Player", method =() => Mods.SpawnVideoPlayer(), disableMethod =() => Mods.DisableSpawnVideoPlayer(), enabled = false, toolTip = "Hand video player (URL from clipboard)" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn TV", method =() => Mods.SpawnTV(), disableMethod =() => Mods.DisableSpawnTV(), enabled = false, toolTip = "Spawn TV (video URL from clipboard)" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Travis", method =() => Mods.SpawnTravis(), disableMethod =() => Mods.DisableSpawnTravis(), enabled = false, toolTip = "Spawn Travis Scott" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Shreksophone", method =() => Mods.SpawnShreksophone(), disableMethod =() => Mods.DisableSpawnShreksophone(), enabled = false, toolTip = "Spawn shreksophone in the map" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Carti", method =() => Mods.SpawnCarti(), disableMethod =() => Mods.DisableSpawnCarti(), enabled = false, toolTip = "Spawn twerking Carti in the map" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Spawn Nuke", method =() => Mods.SpawnNuke(), disableMethod =() => Mods.DisableSpawnNuke(), enabled = false, toolTip = "Deploy a nuke above you" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Destroy All Assets", method =() => Mods.DestroyAllAssets(), enabled = false, nontoggleable = true, toolTip = "Remove all spawned assets" });
                    adminCat.Buttons.Add(new ButtonInfo { buttonText = "Console Settings", method =() => MenuManager.ToggleCategory("Console Settings"), enabled = false, nontoggleable = true, toolTip = "Console settings (laser color, protection, etc)" });
                }
                if (mainCat != null)
                    mainCat.Buttons.Add(new ButtonInfo { buttonText = "Admin Mods", method =() => MenuManager.ToggleCategory("Admin Mods"), enabled = false, nontoggleable = true, toolTip = "Go to Admin Mods!" });
                if (MenuManager.CurrentCategoryName != "Main")
                    MenuManager.CurrentCategoryName = "Main";
                pageNumber = 0;
                DestroyMenu();
                instance.Draw();
            }
            else if (!isAdmin && adminMainBtnExists)
            {
                if (mainCat != null)
                    mainCat.Buttons.RemoveAll(b => b.buttonText == "Admin Mods");
                if (MenuManager.CurrentCategoryName == "Admin Mods" || MenuManager.CurrentCategoryName == "Console Settings")
                    MenuManager.CurrentCategoryName = "Main";
                pageNumber = 0;
                DestroyMenu();
                instance.Draw();
            }
        }
        private void UpdateCustomBoardText()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText").GetComponent<TextMeshPro>().text = CustomBoardTexts[0];
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText").GetComponent<TextMeshPro>().text = CustomBoardTexts[1];
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData").GetComponent<TextMeshPro>().text = CustomBoardTexts[2];
            if (PhotonNetwork.IsConnectedAndReady)
            {
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText").GetComponent<TextMeshPro>().text = CustomBoardTexts[3];
                custom = false;
            }
        }
        public void Draw()
        {
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(menu.GetComponent<Rigidbody>());
            Destroy(menu.GetComponent<BoxCollider>());
            Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * GorillaLocomotion.GTPlayer.Instance.scale;
            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(menuObj.GetComponent<Rigidbody>());
            Destroy(menuObj.GetComponent<BoxCollider>());
            menuObj.transform.parent = menu.transform;
            menuObj.transform.rotation = Quaternion.identity;
            menuObj.transform.localScale = MenuScale;
            if (ChangingColors)
            {
                GradientColorKey[] array = new GradientColorKey[4];
                array[0].color = FirstColor;
                array[0].time = 0f;
                array[1].color = FirstColor;
                array[1].time = 0.3f;
                array[2].color = SecondColor;
                array[2].time = 0.6f;
                array[3].color = FirstColor;
                array[3].time = 1f;
                ColorChanger colorChanger = menuObj.AddComponent<ColorChanger>();
                colorChanger.colors = new Gradient
                {
                    colorKeys = array
                };
                colorChanger.Start();
            }
            else
            {
                menuObj.GetComponent<Renderer>().material.color = NormalColor;
            }
            menuObj.transform.position = MenuPos;
            canvasObj = new GameObject();
            canvasObj.transform.parent = menu.transform;
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f;
            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObj.transform
                }
            }.AddComponent<Text>();
            text.gameObject.name = "name";
            titiel = text;
            text.font = MenuFont;
            int yau = pageNumber + 1;
            text.text = MenuTitle;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = MenuTitleColor;
            if (FirstColor == Color.white && SecondColor == Color.white)
            {
                text.color = Color.black;
            }
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = MenuTitleScale;
            component.position = MenuTitlePos;
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            AddPageButtons();
            var currentButtons = MenuManager.CurrentButtons;
            if (currentButtons != null)
            {
                // Check if settings category and use its buttons
                string[] buttonNames = currentButtons.Skip(pageNumber * pageSize).Take(pageSize).Select(b => b.buttonText).ToArray();
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    AddButton(i * 0.13f + 0.26f * 1f, buttonNames[i]);
                }
            }
            GameObject fpsObj = new GameObject();
            fpsObj.transform.SetParent(canvasObj.transform);
            fpsObj.transform.localPosition = new Vector3(0, 0, 1) * 1f;
            fpsText = fpsObj.GetComponent<Text>();
            if (fpsText == null)
                fpsText = fpsObj.AddComponent<Text>();
            fpsText.font = MenuFont;
            fpsText.text = bottomBarStr;
            fpsText.fontSize = 20;
            fpsText.alignment = TextAnchor.MiddleCenter;
            fpsText.resizeTextForBestFit = true;
            fpsText.resizeTextMinSize = 0;
            fpsText.color = ToolTipColor;
            if (FirstColor == Color.white && SecondColor == Color.white)
            {
                fpsText.color = Color.black;
            }
            RectTransform fpsRT = fpsObj.GetComponent<RectTransform>();
            fpsRT.localPosition = Vector3.zero;
            fpsRT.sizeDelta = ToolTipScale;
            fpsRT.position = new Vector3(0.06f, 0f, -0.18f) * 1f;
            fpsRT.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }
        public static Text titiel;
        private static GameObject MakeNavButton(string name, string relatedText, Vector3 pos, Vector3 scale, bool gradient)
        {
            GameObject go = GameObject.CreatePrimitive(gradient ? (PrimitiveType)3 : PrimitiveType.Cube);
            go.name = name;
            Destroy(go.GetComponent<Rigidbody>());
            go.GetComponent<BoxCollider>().isTrigger = true;
            go.transform.parent = menu.transform;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = scale;
            go.transform.localPosition = pos;
            go.AddComponent<BtnCollider>().relatedText = relatedText;
            if (gradient)
            {
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
                var keys = new GradientColorKey[3];
                keys[0] = new GradientColorKey(new Color32(50, 50, 50, 255), 0f);
                keys[1] = new GradientColorKey(new Color32(90, 90, 90, 255), 0.5f);
                keys[2] = new GradientColorKey(new Color32(50, 50, 50, 255), 1f);
                var cc = go.AddComponent<ColorChanger>();
                cc.colors = new Gradient { colorKeys = keys };
                cc.Start();
            }
            else
                go.GetComponent<Renderer>().material.color = NextPrevButtonColor;
            return go;
        }
        private static Text MakeNavText(string content, Vector3 pos, Color? color = null)
        {
            Text t = new GameObject { transform = { parent = canvasObj.transform } }.AddComponent<Text>();
            t.font = MenuFont;
            t.text = content;
            t.fontSize = 1;
            t.alignment = TextAnchor.MiddleCenter;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 0;
            if (color.HasValue) t.color = color.Value;
            var rt = t.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.sizeDelta = new Vector2(0.2f, 0.03f);
            rt.localPosition = pos;
            rt.rotation = Quaternion.Euler(180f, 90f, 90f);
            return t;
        }
        private static void AddPageButtons()
        {
            var currentButtons = MenuManager.CurrentButtons;
            int totalButtons = currentButtons?.Count ?? 0;
            int pages = Mathf.Max(1, (totalButtons + pageSize - 1) / pageSize);
            int nextPage = (pageNumber + 1) % pages;
            int prevPage = (pageNumber - 1 + pages) % pages;

            if (Mods.change7 == 1)
            {
                float z = 0f;
                MakeNavButton("prev", "PreviousPage", new Vector3(0.56f, 0f, 0.28f - z), new Vector3(0.09f, 0.8f, 0.08f), true);
                MakeNavText("[" + prevPage + "] << Prev", new Vector3(0.064f, 0f, 0.111f - z / 2.55f));
                z = 0.13f;
                MakeNavButton("next", "NextPage", new Vector3(0.56f, 0f, 0.28f - z), new Vector3(0.09f, 0.8f, 0.08f), true);
                MakeNavText("Next >> [" + nextPage + "]", new Vector3(0.064f, 0f, 0.111f - z / 2.55f));
                pageSize = 4;
            }
            else if (Mods.change7 == 2)
            {
                var s = new Vector3(0.045f, 0.25f, 0.064295f);
                MakeNavButton("prev", "PreviousPage", new Vector3(0.56f, 0.37f, 0.541f), s, false);
                MakeNavText("<", new Vector3(0.064f, 0.11f, 0.215f), NextPrevTextColor);
                MakeNavButton("next", "NextPage", new Vector3(0.56f, -0.37f, 0.541f), s, false);
                MakeNavText(">", new Vector3(0.064f, -0.11f, 0.215f), NextPrevTextColor);
                pageSize = 6;
            }
            else if (Mods.change7 == 3)
            {
                var s = new Vector3(0.045f, 0.25f, 0.8936298f);
                MakeNavButton("prev", "PreviousPage", new Vector3(0.56f, 0.657f, 0.0063f), s, false);
                MakeNavText("<", new Vector3(0.064f, 0.20f, 0.0063f), NextPrevTextColor);
                MakeNavButton("next", "NextPage", new Vector3(0.56f, -0.657f, 0.0063f), s, false);
                MakeNavText(">", new Vector3(0.064f, -0.20f, 0.0063f), NextPrevTextColor);
                pageSize = 6;
            }
            else if (Mods.change7 == 4)
            {
                var s = new Vector3(0.045f, 0.25f, 0.064295f);
                MakeNavButton("prev", "PreviousPage", new Vector3(0.56f, 0.37f, -0.541f), s, false);
                MakeNavText("<", new Vector3(0.064f, 0.11f, -0.215f), NextPrevTextColor);
                MakeNavButton("next", "NextPage", new Vector3(0.56f, -0.37f, -0.541f), s, false);
                MakeNavText(">", new Vector3(0.064f, -0.11f, -0.215f), NextPrevTextColor);
                pageSize = 6;
            }
            else if (Mods.change7 == 5)
                pageSize = 6;

            float dy = 0.26f;
            var disconnectBtn = MakeNavButton("disconnect", "DisconnectingButton", new Vector3(0.56f, -0.8f, 0.35f - dy), new Vector3(0.045f, 0.55f, 0.16f), false);
            disconnectBtn.GetComponent<Renderer>().material.color = DisconnectButtonColor;
            var dText = MakeNavText("Disconnect", new Vector3(0.06f, -0.24f, 0.14f - dy / 2.55f), DisconnectTextColor);
            dText.gameObject.name = "disconnect text";
            if (Mods.change7 == 3)
            {
                disconnectBtn.transform.localPosition = new Vector3(0.56f, -1.1f, 0.35f - dy);
                dText.GetComponent<RectTransform>().localPosition = new Vector3(0.06f, -0.33f, 0.14f - dy / 2.55f);
            }
        }
        public static void DestroyMenu()
        {
            Destroy(menu);
            Destroy(canvasObj);
            Destroy(reference);
            menu = null;
            menuObj = null;
            canvasObj = null;
            reference = null;
        }
        public static GameObject Button;
        public static Text text2;
        private static void AddButton(float offset, string text)
        {
            Button = GameObject.CreatePrimitive((PrimitiveType)3);
            Destroy(Button.GetComponent<Rigidbody>());
            Button.GetComponent<BoxCollider>().isTrigger = true;
            Button.transform.parent = menu.transform;
            Button.transform.rotation = Quaternion.identity;
            Button.transform.localScale = ButtonScale * GorillaLocomotion.GTPlayer.Instance.scale;
            if (Mods.change7 == 1)
            {
                Button.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            }
            if (Mods.change7 == 2 | Mods.change7 == 3 | Mods.change7 == 4 | Mods.change7 == 5)
            {
                Button.transform.localPosition = new Vector3(0.56f, 0f, 0.6f - offset);
            }
            Button.AddComponent<BtnCollider>().relatedText = text;
            int num = -1;
            var currentButtons = MenuManager.CurrentButtons;
            if (currentButtons != null)
            {
                for (int i = 0; i < currentButtons.Count; i++)
                {
                    if (text == currentButtons[i].buttonText) { num = i; break; }
                }
            }
            text2 = new GameObject
            {
                transform =
                {
                    parent = canvasObj.transform
                }
            }.AddComponent<Text>();
            text2.font = MenuFont;
            text2.text = text;
            text2.fontSize = 1;
            text2.alignment = TextAnchor.MiddleCenter;
            text2.resizeTextForBestFit = true;
            text2.resizeTextMinSize = 0;
            RectTransform component = text2.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = ButtonTextScale;
            if (Mods.change7 == 1)
            {
                component.localPosition = new Vector3(0.064f, 0f, 0.111f - offset / 2.55f);
            }
            if (Mods.change7 == 2 | Mods.change7 == 3 | Mods.change7 == 4 | Mods.change7 == 5)
            {
                component.localPosition = new Vector3(0.064f, 0f, 0.237f - offset / 2.55f);
            }
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            bool? isEnabled = null;
            if (currentButtons != null && num >= 0 && num < currentButtons.Count)
                isEnabled = currentButtons[num].enabled;
            if (isEnabled == true)
            {
                Button.GetComponent<Renderer>().material.color = ButtonColorEnabled;
                text2.color = EnableTextColor;
            }
            else
            {
                Button.GetComponent<Renderer>().material.color = ButtonColorDisable;
                text2.color = DIsableTextColor;
            }
        }
        void Awake()
        {
            instance = this;
        }
        public void Start()
        {
            InitCategories();
            Mods.AutoLoad();
            sessionStartTime = DateTime.Now;
            Draw();
        }
        public static void Toggle(string relatedText)
        {
            var currentButtons = MenuManager.CurrentButtons;
            if (currentButtons == null) return;
            int totalButtons = currentButtons.Count;
            int num = (totalButtons + pageSize - 1) / pageSize;
            if (num < 1) num = 1;

            if (relatedText == "NextPage")
            {
                if (pageNumber < num - 1) pageNumber++; else pageNumber = 0;
                DestroyMenu(); instance.Draw();
                return;
            }
            if (relatedText == "PreviousPage")
            {
                if (pageNumber > 0) pageNumber--; else pageNumber = num - 1;
                DestroyMenu(); instance.Draw();
                return;
            }
            if (relatedText == "DisconnectingButton")
            {
                PhotonNetwork.Disconnect();
                VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f);
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
            if (num2 >= 0 && num2 < currentButtons.Count && currentButtons[num2].enabled != null)
            {
                var btn = currentButtons[num2];
                if (btn.nontoggleable == true)
                {
                    btn.method?.Invoke();
                    return;
                }
                if (MenuManager.CurrentCategoryName == "Master Mods" && !PhotonNetwork.IsMasterClient)
                {
                    NotifiLib.SendNotification("[<color=red>MASTER</color>] You are not master client!");
                    return;
                }
                bool wasEnabled = btn.enabled.Value;
                btn.enabled = !wasEnabled;
                if (btn.enabled == true)
                    btn.method?.Invoke();
                else if (btn.disableMethod != null)
                    btn.disableMethod.Invoke();
                Mods.AutoSave();
                if (btn.enabled == true && !string.IsNullOrEmpty(btn.toolTip) && btn.toolTip != "This button doesn't have a tooltip/tutorial")
                {
                    NotifiLib.SendNotification("[<color=#00ccff>MOD</color>] " + btn.buttonText + ": " + btn.toolTip, 2);
                }
                if (menu != null)
                    UpdateButtonVisual(relatedText, btn.enabled.Value);
            }
        }
        private static void UpdateButtonVisual(string buttonText, bool isEnabled)
        {
            foreach (Transform child in menu.transform)
            {
                var bc = child.GetComponent<BtnCollider>();
                if (bc != null && bc.relatedText == buttonText)
                {
                    child.GetComponent<Renderer>().material.color = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
                    break;
                }
            }
            if (canvasObj != null)
            {
                foreach (Transform child in canvasObj.transform)
                {
                    var t = child.GetComponent<Text>();
                    if (t != null && t.text == buttonText)
                    {
                        t.color = isEnabled ? EnableTextColor : DIsableTextColor;
                        break;
                    }
                }
            }
        }
        #endregion
    }
}

internal class BtnCollider : MonoBehaviour
{
    #region Button Press Stuff
    public static int framePressCooldown = 0;
    private void OnTriggerEnter(Collider collider)
    {
        if (Time.frameCount >= framePressCooldown + WristMenu.ClickCooldown && collider.name == "buttonPresser")
        {
            if (!Mods.right)
            {
                GorillaTagger.Instance.StartVibration(false, .01f, 0.001f);
                VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, true, 0.1f);
            }
            else
            {
                GorillaTagger.Instance.StartVibration(true, .01f, 0.001f);
                VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f);
            }
            WristMenu.Toggle(relatedText);
            framePressCooldown = Time.frameCount;
        }
    }
    public string relatedText;
    #endregion
}
