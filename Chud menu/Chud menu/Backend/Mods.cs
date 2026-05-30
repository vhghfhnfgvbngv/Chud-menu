using GorillaLocomotion;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using GTAG_NotificationLib;
using HarmonyLib;
using Fusion;
using Photon.Pun;
using Photon.Realtime;
using MalachiTemp.Classes;
using MalachiTemp.UI;
using MalachiTemp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

using Text = UnityEngine.UI.Text;
using TMPro;


namespace MalachiTemp.Backend
{
    internal class Mods : MonoBehaviour
    {
        public static Mods instance;
        void Awake() { instance = this; }

        #region Utility
        public static void DisableButton(string name)
        {
            GetButton(name).enabled = new bool?(false);
            WristMenu.DestroyMenu();
            WristMenu.instance.Draw();
        }

        #endregion
        #region Movement
        private static bool joystickFlyActive = false;
        public static void JoystickFly(float speed)
        {
            joystickFlyActive = true;
        }
        public static float flySpeed = 8f;
        public static void ChangeFlySpeed(bool positive = true)
        {
            float[] speeds = { 2f, 5f, 8f, 11f, 14f, 17f, 20f };
            string[] names = { "2", "5", "8", "11", "14", "17", "20" };
            int idx = Array.IndexOf(speeds, flySpeed);
            if (idx < 0) idx = 2;
            if (positive) idx++; else idx--;
            idx %= speeds.Length;
            if (idx < 0) idx = speeds.Length - 1;
            flySpeed = speeds[idx];
            NotifiLib.SendNotification("[<color=white>[</color><color=blue>SETTINGS</color><color=white>] Fly Speed: " + names[idx] + "</color>");
            AutoSave();
        }
        public static void DisableJoystickFly()
        {
            GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().useGravity = true;
            joystickFlyActive = false;
        }
        private static bool wasdFlyActive = false;
        private static float wasdFlyMouseSense = 1f;
        private static float wasdPitch;
        public static void EnableWASDFly()
        {
            wasdFlyActive = true;
            wasdPitch = 0f;
        }
        public static void DisableWASDFly()
        {
            wasdFlyActive = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        private static bool flyActive = false;
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
            if (!flyActive) return;
            Rigidbody rb = GorillaTagger.Instance.rigidbody;
            if (rb == null) return;
            if (ControllerInputPoller.instance != null && ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flySpeed);
                rb.linearVelocity = Vector3.zero;
            }
        }
        public static void ChangeWASDFlyMouseSense()
        {
            float[] senses = { 0.5f, 1f, 1.5f, 2f, 3f };
            int idx = Array.IndexOf(senses, wasdFlyMouseSense);
            if (idx < 0) idx = 1;
            idx = (idx + 1) % senses.Length;
            wasdFlyMouseSense = senses[idx];
            NotifiLib.SendNotification("[<color=white>[</color><color=blue>SETTINGS</color><color=white>] WASD Mouse Sense: " + wasdFlyMouseSense.ToString("0.0") + "</color>");
            AutoSave();
        }
        private static void UpdateWASDFly()
        {
            if (!wasdFlyActive) return;
            Rigidbody rb = GorillaTagger.Instance.rigidbody;
            if (rb == null) return;
            rb.useGravity = false;

            Transform head = GorillaLocomotion.GTPlayer.Instance.headCollider.transform;
            Transform player = GorillaLocomotion.GTPlayer.Instance.transform;

            Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;

            Vector3 moveDir = Vector3.zero;
            bool wantsUp = false;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed) moveDir += flatForward;
                if (kb.sKey.isPressed) moveDir -= flatForward;
                if (kb.aKey.isPressed) moveDir -= flatRight;
                if (kb.dKey.isPressed) moveDir += flatRight;
                if (kb.spaceKey.isPressed) { moveDir += Vector3.up; wantsUp = true; }
                if (kb.ctrlKey.isPressed) { moveDir -= Vector3.up; wantsUp = true; }
            }

            if (!wantsUp)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            Vector3 targetVel = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized * flySpeed : Vector3.zero;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVel, 0.12875f);

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Vector2 delta = mouse.delta.ReadValue() * wasdFlyMouseSense * 0.15f;
                player.Rotate(Vector3.up, delta.x, Space.World);
                wasdPitch = Mathf.Clamp(wasdPitch - delta.y, -90f, 90f);
                head.localRotation = Quaternion.Euler(wasdPitch, 0f, 0f);
            }
            else
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        public static void NoGravity()
        {
            Rigidbody rb = GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>();
            rb.useGravity = false;
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }
        public static void DisableNoGravity()
        {
            GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().useGravity = true;
        }
        public static void Platforms()
        {
            PlatformsThing(false, stickyplatforms);
        }
        public static void Noclip()
        {
            noclipCacheFrame++;
            if (noclipCacheFrame % 60 == 0 || noclipCache.Length == 0)
                noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider m in noclipCache)
            {
                if (m == null) continue;
                m.enabled = !WristMenu.bbuttonDown;
            }
        }
        public static void NoclipOff()
        {
            noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider m in noclipCache)
            {
                if (m == null) continue;
                m.enabled = true;
            }
        }
        #endregion
        #region Rig Mods


        #region Speed Boost
        public static int speedboostCycle = 1;
        public static float jspeed = 7.5f;
        public static float jmulti = 1.1f;

        public static void ChangeSpeedBoostAmount(bool positive = true)
        {
            float[] jspeedamounts = { 2f, 7.5f, 8f, 9f, 200f };
            float[] jmultiamounts = { 0.5f, 1.1f, 1.5f, 2f, 10f };
            string[] speedNames = { "Slow", "Normal", "Middle", "Fast", "Ultra Fast" };

            if (positive) speedboostCycle++;
            else speedboostCycle--;
            speedboostCycle %= jspeedamounts.Length;
            if (speedboostCycle < 0) speedboostCycle = jspeedamounts.Length - 1;

            jspeed = jspeedamounts[speedboostCycle];
            jmulti = jmultiamounts[speedboostCycle];

            NotifiLib.SendNotification("[<color=orange>MOVEMENT</color>] Speed: " + speedNames[speedboostCycle]);
            AutoSave();
        }

        public static void SpeedBoost()
        {
            float jspt = jspeed;
            float jmpt = jmulti;
            GTPlayer.Instance.maxJumpSpeed = jspt;
            GTPlayer.Instance.jumpMultiplier = jmpt;
        }

        public static void DisableSpeedBoost()
        {
            GTPlayer.Instance.maxJumpSpeed = 6.5f;
            GTPlayer.Instance.jumpMultiplier = 1.1f;
        }
        #endregion

        #region Pull Mod
        public static int pullPowerInt;
        private static float pullPower = 0.05f;
        private static readonly Dictionary<bool, bool> previousTouchingGround = new Dictionary<bool, bool>();

        public static void ChangePullModPower(bool positive = true)
        {
            float[] powers = { 0.05f, 0.1f, 0.2f, 0.4f };
            string[] powerNames = { "Normal", "Medium", "Strong", "Powerful" };
            if (positive) pullPowerInt++;
            else pullPowerInt--;
            pullPowerInt %= powerNames.Length;
            if (pullPowerInt < 0) pullPowerInt = powerNames.Length - 1;
            pullPower = powers[pullPowerInt];
            NotifiLib.SendNotification("[<color=orange>MOVEMENT</color>] Pull power: " + powerNames[pullPowerInt]);
            AutoSave();
        }

        private static void ProcessPullHand(bool left)
        {
            if ((left ? !ControllerInputPoller.instance.leftGrab : !ControllerInputPoller.instance.rightGrab)) return;
            bool touchingGround = GTPlayer.Instance.IsHandTouching(left);
            previousTouchingGround.TryGetValue(left, out bool wasTouchingGround);
            if (!touchingGround && wasTouchingGround)
            {
                Vector3 normal = Vector3.up;
                Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                Vector3 direction = rb.linearVelocity.X_Z();
                GTPlayer.Instance.transform.position +=
                    (direction - normal * Vector3.Dot(direction, normal)).normalized *
                    (direction.magnitude / GTPlayer.Instance.maxJumpSpeed * (pullPower * 5f)) * GTPlayer.Instance.scale;
            }
            previousTouchingGround[left] = touchingGround;
        }

        public static void PullMod()
        {
            ProcessPullHand(false);
            ProcessPullHand(true);
        }
        #endregion

        #endregion
        #region Fun
        internal static bool ghostMonkeOn = false;
        private static bool ghostMonkeLastPress = false;
        private static Vector3 ghostMonkeFrozenPos;
        private static Quaternion ghostMonkeFrozenRot;
        private static TransformSnapshot ghostMonkeSnapshot;
        private struct TransformSnapshot
        {
            public Vector3 headPos;
            public Quaternion headRot;
            public Vector3 leftHandPos;
            public Quaternion leftHandRot;
            public Vector3 rightHandPos;
            public Quaternion rightHandRot;
            public float leftIndexT, leftMiddleT, leftThumbT;
            public float rightIndexT, rightMiddleT, rightThumbT;
        }
        private static void TakeRigSnapshot(out TransformSnapshot s)
        {
            var rig = VRRig.LocalRig;
            s = new TransformSnapshot();
            if (rig.head != null && rig.head.rigTarget != null)
            {
                s.headPos = rig.head.rigTarget.transform.position;
                s.headRot = rig.head.rigTarget.transform.rotation;
            }
            if (rig.leftHand != null && rig.leftHand.rigTarget != null)
            {
                s.leftHandPos = rig.leftHand.rigTarget.transform.position;
                s.leftHandRot = rig.leftHand.rigTarget.transform.rotation;
            }
            if (rig.rightHand != null && rig.rightHand.rigTarget != null)
            {
                s.rightHandPos = rig.rightHand.rigTarget.transform.position;
                s.rightHandRot = rig.rightHand.rigTarget.transform.rotation;
            }
            s.leftIndexT = rig.leftIndex.calcT;
            s.leftMiddleT = rig.leftMiddle.calcT;
            s.leftThumbT = rig.leftThumb.calcT;
            s.rightIndexT = rig.rightIndex.calcT;
            s.rightMiddleT = rig.rightMiddle.calcT;
            s.rightThumbT = rig.rightThumb.calcT;
        }
        private static void ApplyRigSnapshot(ref TransformSnapshot s)
        {
            var rig = VRRig.LocalRig;
            if (rig.head != null && rig.head.rigTarget != null)
            {
                rig.head.rigTarget.transform.SetPositionAndRotation(s.headPos, s.headRot);
            }
            if (rig.leftHand != null && rig.leftHand.rigTarget != null)
            {
                rig.leftHand.rigTarget.transform.SetPositionAndRotation(s.leftHandPos, s.leftHandRot);
            }
            if (rig.rightHand != null && rig.rightHand.rigTarget != null)
            {
                rig.rightHand.rigTarget.transform.SetPositionAndRotation(s.rightHandPos, s.rightHandRot);
            }
            rig.leftIndex.calcT = s.leftIndexT; rig.leftIndex.LerpFinger(1f, false);
            rig.leftMiddle.calcT = s.leftMiddleT; rig.leftMiddle.LerpFinger(1f, false);
            rig.leftThumb.calcT = s.leftThumbT; rig.leftThumb.LerpFinger(1f, false);
            rig.rightIndex.calcT = s.rightIndexT; rig.rightIndex.LerpFinger(1f, false);
            rig.rightMiddle.calcT = s.rightMiddleT; rig.rightMiddle.LerpFinger(1f, false);
            rig.rightThumb.calcT = s.rightThumbT; rig.rightThumb.LerpFinger(1f, false);
        }
        public static void GhostMonke()
        {
            if (VRRig.LocalRig == null) return;
            bool pressed = ControllerInputPoller.instance.rightControllerSecondaryButton;
            if (pressed && !ghostMonkeLastPress)
            {
                ghostMonkeOn = !ghostMonkeOn;
                if (ghostMonkeOn)
                {
                    ghostMonkeFrozenPos = VRRig.LocalRig.transform.position;
                    ghostMonkeFrozenRot = VRRig.LocalRig.transform.rotation;
                    TakeRigSnapshot(out ghostMonkeSnapshot);
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                }
            }
            ghostMonkeLastPress = pressed;
            if (ghostMonkeOn)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
                ApplyRigSnapshot(ref ghostMonkeSnapshot);
            }
        }
        public static void DisableGhostMonke()
        {
            if (VRRig.LocalRig != null)
                VRRig.LocalRig.enabled = true;
            ghostMonkeOn = false;
        }
        internal static bool invisMonkeOn = false;
        private static Vector3 invisMonkeSavedPos;
        private static bool invisMonkeLastPress = false;
        private static bool invisMonkeSkinsDisabled = false;
        private static void InvisMonkeSetSkins(bool disable)
        {
            if (VRRig.LocalRig == null) return;
            if (disable == invisMonkeSkinsDisabled) return;
            SkinnedMeshRenderer skin = VRRig.LocalRig.mainSkin;
            if (skin == null) return;
            skin.enabled = !disable;
            invisMonkeSkinsDisabled = disable;
        }
        public static void InvisMonke()
        {
            if (VRRig.LocalRig == null) return;
            bool pressed = ControllerInputPoller.instance.rightControllerPrimaryButton;
            if (pressed && !invisMonkeLastPress)
            {
                if (!invisMonkeOn)
                {
                    invisMonkeSavedPos = VRRig.LocalRig.transform.position;
                    invisMonkeOn = true;
                    InvisMonkeSetSkins(true);
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                    VRRig.LocalRig.transform.position = invisMonkeSavedPos;
                    InvisMonkeSetSkins(false);
                    invisMonkeOn = false;
                }
            }
            invisMonkeLastPress = pressed;
            if (invisMonkeOn)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
            }
        }
        public static void DisableInvisMonke()
        {
            if (VRRig.LocalRig != null && invisMonkeOn)
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.transform.position = invisMonkeSavedPos;
                InvisMonkeSetSkins(false);
            }
            invisMonkeOn = false;
        }
        void Update()
        {
            if (wasdFlyActive) UpdateWASDFly();
            if (flyActive) UpdateFly();
        }
        void LateUpdate()
        {
            if (VRRig.LocalRig == null) return;
            if (ghostMonkeOn)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
                ApplyRigSnapshot(ref ghostMonkeSnapshot);
            }
            if (invisMonkeOn)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
            }
            UpdateFlail();
            UpdateSpazBody();
        }
        private static void UpdateJoystickFly()
        {
            Rigidbody rb = GorillaTagger.Instance.rigidbody;
            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);

            Vector2 leftJoy = WristMenu.joyL;
            Vector2 rightJoy = WristMenu.joy;
            Transform head = GorillaLocomotion.GTPlayer.Instance.headCollider.transform;

            Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;

            Vector3 direction = flatForward * leftJoy.y + flatRight * leftJoy.x + Vector3.up * rightJoy.y;

            Vector3 targetVel = direction.sqrMagnitude > 0.01f ? direction.normalized * flySpeed : Vector3.zero;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVel, 0.12875f);
        }
        private static bool antiNameBanApplied = true;
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
        #endregion
        #region Bitcrunch Mic
        private static bool bitcrunchMicActive = false;
        private static int bitcrunchOrigSampleRate = 16000;
        private static int bitcrunchOrigBitrate = 24000;
        public static void BitcrunchMic()
        {
            if (bitcrunchMicActive) return;
            var recorder = GorillaTagger.Instance.myRecorder;
            if (recorder == null) return;
            bitcrunchOrigSampleRate = (int)recorder.SamplingRate;
            bitcrunchOrigBitrate = recorder.Bitrate;
            recorder.SamplingRate = (POpusCodec.Enums.SamplingRate)8000;
            recorder.Bitrate = 8000;
            recorder.RestartRecording(true);
            bitcrunchMicActive = true;
            NotifiLib.SendNotification("[<color=green>FUN</color>] Bitcrunch Mic: ON");
        }
        public static void DisableBitcrunchMic()
        {
            if (!bitcrunchMicActive) return;
            var recorder = GorillaTagger.Instance.myRecorder;
            if (recorder != null)
            {
                recorder.SamplingRate = (POpusCodec.Enums.SamplingRate)bitcrunchOrigSampleRate;
                recorder.Bitrate = bitcrunchOrigBitrate;
                recorder.RestartRecording(true);
            }
            bitcrunchMicActive = false;
            NotifiLib.SendNotification("[<color=green>FUN</color>] Bitcrunch Mic: OFF");
        }
        #endregion
        #region Minos Prime
        private static bool minosPrimedForSlam = false;
        private static bool minosWaitingForImpact = false;
        private static AudioClip minosCrushClip = null;
        private static AudioClip minosSlamClip = null;
        private static bool minosClipsLoaded = false;
        private static bool minosSecondaryWasDown = false;
        private static bool minosPrimaryWasDown = false;
        private static Coroutine minosRestoreCoroutine = null;
        private static AudioSource minosLocalSource = null;
        private const string MinosSoundDir = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag\Chud menu\";
        private const string MinosCrushUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/CRUSH%20!.mp3";
        private const string MinosSlamUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/slam%20sound.mp3";
        public static void MinosPrime()
        {
            if (!minosClipsLoaded)
            {
                minosClipsLoaded = true;
                instance.StartCoroutine(LoadMinosSounds());
            }
            bool secDown = ControllerInputPoller.instance.rightControllerSecondaryButton;
            bool priDown = ControllerInputPoller.instance.rightControllerPrimaryButton;
            if (secDown && !minosSecondaryWasDown)
            {
                GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(GorillaTagger.Instance.rigidbody.linearVelocity.x, 20f, GorillaTagger.Instance.rigidbody.linearVelocity.z);
                PlayMinosClip(minosCrushClip);
                minosPrimedForSlam = true;
                minosWaitingForImpact = false;
            }
            if (priDown && !minosPrimaryWasDown && minosPrimedForSlam)
            {
                Vector3 dir = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
                GorillaTagger.Instance.rigidbody.linearVelocity = dir * 35f;
                minosPrimedForSlam = false;
                minosWaitingForImpact = true;
            }
            if (minosWaitingForImpact && GorillaTagger.Instance.rigidbody.linearVelocity.magnitude < 5f)
            {
                minosWaitingForImpact = false;
                PlayMinosClip(minosSlamClip);
            }
            minosSecondaryWasDown = secDown;
            minosPrimaryWasDown = priDown;
        }
        public static void DisableMinosPrime()
        {
            minosPrimedForSlam = false;
            minosWaitingForImpact = false;
            minosSecondaryWasDown = false;
            minosPrimaryWasDown = false;
            if (minosRestoreCoroutine != null)
            {
                instance.StopCoroutine(minosRestoreCoroutine);
                minosRestoreCoroutine = null;
            }
            RestoreRecorder();
        }
        private static void PlayMinosClip(AudioClip clip)
        {
            if (clip == null) return;
            if (minosLocalSource == null)
            {
                GameObject go = new GameObject("MinosAudio");
                UnityEngine.Object.DontDestroyOnLoad(go);
                minosLocalSource = go.AddComponent<AudioSource>();
                minosLocalSource.spatialBlend = 0f;
                minosLocalSource.volume = 1f;
            }
            minosLocalSource.Stop();
            minosLocalSource.PlayOneShot(clip, 2f);
            var recorder = GorillaTagger.Instance.myRecorder;
            if (recorder != null)
            {
                if (minosRestoreCoroutine != null)
                    instance.StopCoroutine(minosRestoreCoroutine);
                recorder.SourceType = Photon.Voice.Unity.Recorder.InputSourceType.AudioClip;
                recorder.AudioClip = clip;
                recorder.RestartRecording(true);
                recorder.DebugEchoMode = true;
                minosRestoreCoroutine = instance.StartCoroutine(RestoreMicAfter(clip.length));
            }
        }
        private static IEnumerator RestoreMicAfter(float delay)
        {
            yield return new WaitForSeconds(delay + 0.4f);
            if (!instance || !instance.isActiveAndEnabled) yield break;
            RestoreRecorder();
            minosRestoreCoroutine = null;
        }
        private static void RestoreRecorder()
        {
            var recorder = GorillaTagger.Instance.myRecorder;
            if (recorder == null) return;
            recorder.SourceType = Photon.Voice.Unity.Recorder.InputSourceType.Microphone;
            recorder.AudioClip = null;
            recorder.RestartRecording(true);
            recorder.DebugEchoMode = false;
        }
        private static IEnumerator LoadMinosSounds()
        {
            if (!Directory.Exists(MinosSoundDir))
                Directory.CreateDirectory(MinosSoundDir);
            string crushPath = MinosSoundDir + "CRUSH !.mp3";
            string slamPath = MinosSoundDir + "slam sound.mp3";
            if (!File.Exists(crushPath))
            {
                using (var req = UnityEngine.Networking.UnityWebRequest.Get(MinosCrushUrl))
                {
                    req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                    yield return req.SendWebRequest();
                    if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        File.WriteAllBytes(crushPath, req.downloadHandler.data);
                }
            }
            if (!File.Exists(slamPath))
            {
                using (var req = UnityEngine.Networking.UnityWebRequest.Get(MinosSlamUrl))
                {
                    req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                    yield return req.SendWebRequest();
                    if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        File.WriteAllBytes(slamPath, req.downloadHandler.data);
                }
            }
            string crushFileUrl = "file:///" + crushPath.Replace("\\", "/");
            using (var req = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(crushFileUrl, AudioType.MPEG))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    minosCrushClip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(req);
            }
            string slamFileUrl = "file:///" + slamPath.Replace("\\", "/");
            using (var req = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(slamFileUrl, AudioType.MPEG))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    minosSlamClip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(req);
            }
        }
        #endregion
        #region Per-Frame Mod Updates
        public static void UpdateActiveMods()
        {
            if (joystickFlyActive) UpdateJoystickFly();
            UpdateCosmeticNotifier();

            bool anyGunActive = false;
            foreach (var cat in MenuManager.Categories)
            {
                if (cat.Buttons == null) continue;
                foreach (var btn in cat.Buttons)
                {
                    if (btn.enabled != true || btn.nontoggleable == true || btn.method == null) continue;
                    string t = btn.buttonText;
                    if (t == "Noclip" || t == "Tracers" || t == "2D Box ESP" || t == "Pull Mod" || t == "Platforms"
                        || t == "Name Tags" || t == "FPS Name Tags" || t == "ID Name Tags" || t == "Cosmetic Name Tags" || t == "Cosmetic Notifier")
                        btn.method.Invoke();
                    else if (t == "Speed Boost" || t == "Ghost Monke" || t == "Invis Monke" || t == "Tag While Not Tagged" || t == "No Gravity" || t == "Minos Prime")
                        btn.method.Invoke();
                    else if (t == "Tag Gun" || t == "Tug Gan" || t.EndsWith("Gun"))
                    {
                        btn.method.Invoke();
                        anyGunActive = true;
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
            if (!anyGunActive && pointer != null)
            {
                UnityEngine.Object.Destroy(pointer);
                pointer = null;
                if (Line != null) { Destroy(Line.gameObject); Line = null; }
                gunTriggerWasDown = false;
            }
        }
        private static readonly Dictionary<VRRig, GameObject> boxEspObjects = new Dictionary<VRRig, GameObject>();
        public static void BoxEspRender()
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var box in boxEspObjects.Where(box => !VRRigCache.ActiveRigs.Contains(box.Key)))
            {
                toRemove.Add(box.Key);
                UnityEngine.Object.Destroy(box.Value);
            }
            foreach (VRRig rig in toRemove)
                boxEspObjects.Remove(rig);

            foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal))
            {
                if (!boxEspObjects.TryGetValue(rig, out GameObject box))
                {
                    box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(box.GetComponent<BoxCollider>());
                    box.GetComponent<Renderer>().enabled = false;
                    box.transform.localScale = new Vector3(0.8f, 0.85f, 0f);

                    Shader shader = Shader.Find("GUI/Text Shader");
                    float edgeThick = 0.08f;

                    GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(edge.GetComponent<BoxCollider>());
                    edge.transform.SetParent(box.transform);
                    edge.transform.localPosition = new Vector3(0f, 0.425f, 0f);
                    edge.transform.localScale = new Vector3(0.8f, edgeThick, 1f);
                    edge.GetComponent<Renderer>().material.shader = shader;

                    edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(edge.GetComponent<BoxCollider>());
                    edge.transform.SetParent(box.transform);
                    edge.transform.localPosition = new Vector3(0f, -0.425f, 0f);
                    edge.transform.localScale = new Vector3(0.8f, edgeThick, 1f);
                    edge.GetComponent<Renderer>().material.shader = shader;

                    edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(edge.GetComponent<BoxCollider>());
                    edge.transform.SetParent(box.transform);
                    edge.transform.localPosition = new Vector3(0.4f, 0f, 0f);
                    edge.transform.localScale = new Vector3(edgeThick, 0.85f, 1f);
                    edge.GetComponent<Renderer>().material.shader = shader;

                    edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(edge.GetComponent<BoxCollider>());
                    edge.transform.SetParent(box.transform);
                    edge.transform.localPosition = new Vector3(-0.4f, 0f, 0f);
                    edge.transform.localScale = new Vector3(edgeThick, 0.85f, 1f);
                    edge.GetComponent<Renderer>().material.shader = shader;

                    boxEspObjects.Add(rig, box);
                }
                Color color = rig.playerColor;
                box.transform.position = rig.transform.position;
                box.transform.LookAt(GorillaTagger.Instance.headCollider.transform.position);
                foreach (Transform child in box.transform)
                {
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null) r.material.color = color;
                }
            }
        }

        public static void DisableBoxEsp()
        {
            foreach (var box in boxEspObjects)
                UnityEngine.Object.Destroy(box.Value);
            boxEspObjects.Clear();
        }



        public static void Tracers()
        {
            List<Player> toRemove = null;
            foreach (var kvp in tracerLines)
            {
                if (!PhotonNetwork.PlayerListOthers.Contains(kvp.Key))
                {
                    if (toRemove == null) toRemove = new List<Player>();
                    toRemove.Add(kvp.Key);
                }
            }
            if (toRemove != null)
            {
                foreach (var p in toRemove)
                {
                    Destroy(tracerLines[p].gameObject);
                    tracerLines.Remove(p);
                }
            }
            foreach (Player p in PhotonNetwork.PlayerListOthers)
            {
                VRRig rig = RigHelpers.GetVRRigFromPlayer(p);
                if (rig == null) continue;
                if (!tracerLines.TryGetValue(p, out LineRenderer l))
                {
                    GameObject g = new GameObject("TracerLine");
                    g.hideFlags = HideFlags.HideAndDontSave;
                    l = g.AddComponent<LineRenderer>();
                    l.startWidth = 0.01f;
                    l.endWidth = 0.01f;
                    l.positionCount = 2;
                    l.useWorldSpace = true;
                    l.material.shader = Shader.Find("GUI/Text Shader");
                    tracerLines[p] = l;
                }
                l.SetPosition(0, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position);
                l.SetPosition(1, rig.transform.position);
                Color tracerColor = rig.playerColor;
                try
                {
                    GorillaGameManager gm = GorillaGameManager.instance;
                    if (gm != null && gm is GorillaTagManager gtm && rig.Creator != null && gtm.IsInfected(rig.Creator))
                    {
                        tracerColor = new Color(1f, 0.5f, 0f);
                    }
                }
                catch { }
                tracerColor.a = 0.3f;
                l.startColor = tracerColor;
                l.endColor = tracerColor;
            }
        }
        public static void DisableTracers()
        {
            foreach (var l in tracerLines.Values)
                Destroy(l.gameObject);
            tracerLines.Clear();
        }

        private static readonly Dictionary<VRRig, GameObject> nameTagObjects = new Dictionary<VRRig, GameObject>();
        private static readonly Dictionary<VRRig, GameObject> fpsNameTagObjects = new Dictionary<VRRig, GameObject>();
        private static readonly Dictionary<VRRig, GameObject> idNameTagObjects = new Dictionary<VRRig, GameObject>();
        public static Font comicSansFont;
        private static FieldInfo _fpsField;
        private static int GetFps(VRRig rig)
        {
            if (_fpsField == null)
                _fpsField = AccessTools.Field(typeof(VRRig), "fps");
            return _fpsField != null ? (int)_fpsField.GetValue(rig) : 0;
        }
        private static Vector3 GetTagPosition(VRRig rig, float offset)
        {
            Vector3 basePos = rig.head?.rigTarget?.position ?? rig.transform.position + Vector3.up * 1.6f;
            return basePos + Vector3.up * offset;
        }
        private static void BillboardTag(GameObject obj)
        {
            if (Camera.main == null) return;
            Vector3 pos = obj.transform.position;
            obj.transform.LookAt(2 * pos - Camera.main.transform.position);
        }
        private static Text CreateTagObj(string name, Dictionary<VRRig, GameObject> dict, VRRig rig)
        {
            if (comicSansFont == null)
                comicSansFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 36);
            GameObject go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * 0.003f;
            Text tmp = go.AddComponent<Text>();
            if (comicSansFont != null) tmp.font = comicSansFont;
            tmp.fontSize = 30;
            tmp.horizontalOverflow = HorizontalWrapMode.Overflow;
            tmp.alignment = TextAnchor.MiddleCenter;
            tmp.color = rig.playerColor;
            dict[rig] = go;
            return tmp;
        }
        private static Color TagColor(VRRig rig)
        {
            Color c = rig.playerColor;
            if (c.r == 0f && c.g == 0f && c.b == 0f)
                return Color.white;
            return c;
        }
        private static void CleanTagDict(Dictionary<VRRig, GameObject> dict)
        {
            List<VRRig> toRemove = null;
            foreach (var kvp in dict)
            {
                if (!VRRigCache.ActiveRigs.Contains(kvp.Key))
                {
                    if (toRemove == null) toRemove = new List<VRRig>();
                    toRemove.Add(kvp.Key);
                }
            }
            if (toRemove != null)
            {
                foreach (var rig in toRemove)
                {
                    Destroy(dict[rig]);
                    dict.Remove(rig);
                }
            }
        }
        public static void NameTags()
        {
            CleanTagDict(nameTagObjects);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.isLocal) continue;
                if (!nameTagObjects.TryGetValue(rig, out GameObject obj))
                {
                    Text tmp = CreateTagObj("Chud_Nametag", nameTagObjects, rig);
                    obj = tmp.gameObject;
                    string name = rig.Creator?.NickName ?? "?";
                    tmp.text = name.Length > 24 ? name.Substring(0, 24) : name;
                    tmp.color = TagColor(rig);
                }
                else
                {
                    Text tmp = obj.GetComponent<Text>();
                    if (tmp != null)
                    {
                        string name = rig.Creator?.NickName ?? "?";
                        tmp.text = name.Length > 24 ? name.Substring(0, 24) : name;
                        tmp.color = TagColor(rig);
                    }
                }
                obj.transform.position = GetTagPosition(rig, 0.7f);
                BillboardTag(obj);
            }
        }
        public static void DisableNameTags()
        {
            foreach (var obj in nameTagObjects.Values) Destroy(obj);
            nameTagObjects.Clear();
        }
        public static void FPSTags()
        {
            CleanTagDict(fpsNameTagObjects);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.isLocal) continue;
                if (!fpsNameTagObjects.TryGetValue(rig, out GameObject obj))
                {
                    Text tmp = CreateTagObj("Chud_FPStag", fpsNameTagObjects, rig);
                    obj = tmp.gameObject;
                    string fpsText = GetFps(rig) + " FPS";
                    tmp.text = fpsText.Length > 24 ? fpsText.Substring(0, 24) : fpsText;
                    tmp.color = TagColor(rig);
                }
                else
                {
                    Text tmp = obj.GetComponent<Text>();
                    if (tmp != null)
                    {
                        string fpsText = GetFps(rig) + " FPS";
                        tmp.text = fpsText.Length > 24 ? fpsText.Substring(0, 24) : fpsText;
                        tmp.color = TagColor(rig);
                    }
                }
                obj.transform.position = GetTagPosition(rig, 0.5f);
                BillboardTag(obj);
            }
        }
        public static void DisableFPSTags()
        {
            foreach (var obj in fpsNameTagObjects.Values) Destroy(obj);
            fpsNameTagObjects.Clear();
        }
        public static void IDTags()
        {
            CleanTagDict(idNameTagObjects);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.isLocal) continue;
                if (!idNameTagObjects.TryGetValue(rig, out GameObject obj))
                {
                    Text tmp = CreateTagObj("Chud_IDtag", idNameTagObjects, rig);
                    obj = tmp.gameObject;
                    string id = rig.Creator?.UserId ?? "?";
                    tmp.text = id.Length > 24 ? id.Substring(0, 24) : id;
                    tmp.color = TagColor(rig);
                }
                else
                {
                    Text tmp = obj.GetComponent<Text>();
                    if (tmp != null)
                    {
                        string id = rig.Creator?.UserId ?? "?";
                        tmp.text = id.Length > 24 ? id.Substring(0, 24) : id;
                        tmp.color = TagColor(rig);
                    }
                }
                obj.transform.position = GetTagPosition(rig, 0.6f);
                BillboardTag(obj);
            }
        }
        public static void DisableIDTags()
        {
            foreach (var obj in idNameTagObjects.Values) Destroy(obj);
            idNameTagObjects.Clear();
        }
        private static readonly Dictionary<string, string> cosmeticNames = new Dictionary<string, string>
        {
            { "LBAAK.", "Dev stick" },
            { "LBANI.", "AA BADGE" },
            { "LMAPY.", "Forest guide" },
            { "LBADE.", "Finger painter" },
            { "LBAGS.", "illustrator" },
        };
        private static readonly Dictionary<VRRig, GameObject> cosmeticNameTagObjects = new Dictionary<VRRig, GameObject>();
        private static FieldInfo _ownedCosmeticsField;
        private static HashSet<string> GetOwnedCosmetics(VRRig rig)
        {
            if (_ownedCosmeticsField == null)
                _ownedCosmeticsField = AccessTools.Field(typeof(VRRig), "_playerOwnedCosmetics");
            return _ownedCosmeticsField?.GetValue(rig) as HashSet<string>;
        }
        public static void CosmeticNameTags()
        {
            CleanTagDict(cosmeticNameTagObjects);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.isLocal) continue;
                if (cosmeticNameTagObjects.ContainsKey(rig)) continue;
                HashSet<string> owned = GetOwnedCosmetics(rig);
                if (owned == null || owned.Count == 0) continue;
                List<string> names = new List<string>(owned.Count);
                foreach (string playFabId in owned)
                {
                    string name;
                    if (cosmeticNames.TryGetValue(playFabId, out name))
                        names.Add(name);
                }
                if (names.Count == 0) continue;
                string text = string.Join(", ", names);
                Text tmp = CreateTagObj("Chud_CosmeticTag", cosmeticNameTagObjects, rig);
                tmp.text = text;
                tmp.color = Color.red;
            }
            foreach (var kvp in cosmeticNameTagObjects)
            {
                kvp.Value.transform.position = GetTagPosition(kvp.Key, 0.8f);
                BillboardTag(kvp.Value);
            }
        }
        public static void DisableCosmeticNameTags()
        {
            foreach (var obj in cosmeticNameTagObjects.Values) Destroy(obj);
            cosmeticNameTagObjects.Clear();
        }

        private static bool cosmeticNotifierActive = false;
        private static HashSet<string> cosmeticNotifierNotified = new HashSet<string>();
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
            if (!cosmeticNotifierActive) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.isLocal) continue;
                if (rig.Creator == null) continue;
                HashSet<string> owned = GetOwnedCosmetics(rig);
                if (owned == null || owned.Count == 0) continue;
                string playerKey = rig.Creator.UserId;
                if (cosmeticNotifierNotified.Contains(playerKey)) continue;
                List<string> found = new List<string>(owned.Count);
                foreach (string id in owned)
                {
                    string name;
                    if (cosmeticNames.TryGetValue(id, out name))
                        found.Add(name);
                }
                if (found.Count == 0) continue;
                cosmeticNotifierNotified.Add(playerKey);
                NotifiLib.SendNotification("[<color=red>COSMETIC</color>] " + rig.Creator.NickName + ": " + string.Join(", ", found), 5);
            }
        }
        #endregion
        #region Save-Load Buttons
        private static string SavePath { get { return WristMenu.FolderName + "\\Saved_Buttons.txt"; } }
        private static string SettingsPath { get { return WristMenu.FolderName + "\\Saved_Settings.txt"; } }

        public static void AutoSave()
        {
            try
            {
                if (!Directory.Exists(WristMenu.FolderName)) Directory.CreateDirectory(WristMenu.FolderName);
                // Save enabled buttons
                List<string> list = new List<string>();
                foreach (var category in MenuManager.Categories)
                {
                    foreach (ButtonInfo buttonInfo in category.Buttons)
                    {
                        if (buttonInfo.enabled.GetValueOrDefault() == true & buttonInfo.enabled != null)
                        {
                            list.Add(buttonInfo.buttonText);
                        }
                    }
                }
                File.WriteAllLines(SavePath, list);
                // Save settings values
                List<string> settings = new List<string>
                {
                    "flySpeed=" + flySpeed,
                    "speedboostCycle=" + speedboostCycle,
                    "pullPowerInt=" + pullPowerInt,
                    "laserColorIndex=" + laserColorIndex,
                    "stickyplatforms=False",
                    "wasdFlyMouseSense=" + wasdFlyMouseSense,
                    "right=" + right.ToString()
                };
                File.WriteAllLines(SettingsPath, settings);
            }
            catch { }
        }
        public static void AutoLoad()
        {
            try
            {
                // Load enabled buttons
                if (File.Exists(SavePath))
                {
                    string[] array = File.ReadAllLines(SavePath);
                    foreach (string b in array)
                    {
                        foreach (var category in MenuManager.Categories)
                        {
                            foreach (ButtonInfo buttonInfo in category.Buttons)
                            {
                                if (buttonInfo.buttonText == b)
                                {
                                    buttonInfo.enabled = new bool?(true);
                                    if (buttonInfo.nontoggleable != true)
                                        buttonInfo.method?.Invoke();
                                }
                            }
                        }
                    }
                }
                // Load settings
                if (File.Exists(SettingsPath))
                {
                    string[] lines = File.ReadAllLines(SettingsPath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2) continue;
                        string key = parts[0].Trim();
                        string val = parts[1].Trim();
                        if (key == "flySpeed") float.TryParse(val, out flySpeed);
                        else if (key == "speedboostCycle") int.TryParse(val, out speedboostCycle);
                        else if (key == "pullPowerInt") int.TryParse(val, out pullPowerInt);
                        else if (key == "laserColorIndex") { int.TryParse(val, out laserColorIndex); if (laserColorIndex >= laserColors.Length) laserColorIndex = 0; }
                        else if (key == "wasdFlyMouseSense") float.TryParse(val, out wasdFlyMouseSense);
                        else if (key == "right") bool.TryParse(val, out right);
                        /* stickyplatforms saved for compatibility */
                    }
                }
            }
            catch { }
        }
        public static void ReapplyActiveMods()
        {
            foreach (var category in MenuManager.Categories)
            {
                if (category.Buttons == null) continue;
                foreach (ButtonInfo btn in category.Buttons)
                {
                    if (btn.enabled == true && btn.nontoggleable != true)
                        btn.method?.Invoke();
                }
            }
        }
        #endregion
        #region Settings Methods
        private static bool notificationsEnabled = true;
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
        #endregion
        #region Platforms
        private static void PlatformsThing(bool invis, bool sticky)
        {
            RPlat = WristMenu.gripDownR;
            LPlat = WristMenu.gripDownL;
            if (RPlat)
            {
                if (!once_right && jump_right_local == null)
                {
                    if (sticky)
                    {
                        jump_right_local = GameObject.CreatePrimitive(0);
                    }
                    else
                    {
                        jump_right_local = GameObject.CreatePrimitive((PrimitiveType)3);
                    }
                    if (invis)
                    {
                        Destroy(jump_right_local.GetComponent<Renderer>());
                    }
                    jump_right_local.transform.localScale = scale;
                    jump_right_local.transform.position = new Vector3(0f, -0.01f, 0f) + GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                    jump_right_local.transform.rotation = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.rotation;
                    jump_right_local.AddComponent<GorillaSurfaceOverride>().overrideIndex = jump_right_local.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    once_right = true;
                    once_right_false = false;
                    jump_right_local.GetComponent<Renderer>().material.color = Color.black;
                }
            }
            else
            {
                if (!once_right_false && jump_right_local != null)
                {
                    Destroy(jump_right_local);
                    jump_right_local = null;
                    once_right = false;
                    once_right_false = true;
                }
            }
            if (LPlat)
            {
                if (!once_left && jump_left_local == null)
                {
                    if (sticky)
                    {
                        jump_left_local = GameObject.CreatePrimitive(0);
                    }
                    else
                    {
                        jump_left_local = GameObject.CreatePrimitive((PrimitiveType)3);
                    }
                    if (invis)
                    {
                        Destroy(jump_left_local.GetComponent<Renderer>());
                    }
                    jump_left_local.transform.localScale = scale;
                    jump_left_local.transform.position = new Vector3(0f, -0.01f, 0f) + GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform.position;
                    jump_left_local.transform.rotation = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform.rotation;
                    jump_left_local.AddComponent<GorillaSurfaceOverride>().overrideIndex = jump_left_local.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    once_left = true;
                    once_left_false = false;
                    jump_left_local.GetComponent<Renderer>().material.color = Color.black;
                }
            }
            else
            {
                if (!once_left_false && jump_left_local != null)
                {
                    Destroy(jump_left_local);
                    jump_left_local = null;
                    once_left = false;
                    once_left_false = true;
                }
            }
        }
#endregion
        #region GetButton
        public static ButtonInfo GetButton(string name)
        {
            foreach (var category in MenuManager.Categories)
            {
                foreach (ButtonInfo b in category.Buttons)
                {
                    if (b.buttonText == name)
                        return b;
                }
            }
            return null;
        }
        #endregion
        public static void KickGun()
        {
            MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    ConsoleIntegration.ExecuteCommand("kick", Photon.Realtime.ReceiverGroup.All, rig.Creator.UserId);
                }
            }, delegate { });
        }
        public static void SilentKickGun()
        {
            MakeGun(new Color(0.5f, 0f, 0f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    ConsoleIntegration.ExecuteCommand("silkick", Photon.Realtime.ReceiverGroup.All, rig.Creator.UserId);
                }
            }, delegate { });
        }
        public static void TPGun()
        {
            MakeGun(Color.cyan, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                ConsoleIntegration.TeleportPlayer(pointer.transform.position);
            }, delegate { });
        }
        private static Coroutine flingGunCoroutine;
        private static int flingTargetActor;
        public static void FlingGun()
        {
            MakeGun(Color.yellow, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    Photon.Realtime.Player p = ConsoleIntegration.GetPlayerFromID(rig.Creator.UserId);
                    if (p != null)
                    {
                        flingTargetActor = p.ActorNumber;
                        if (flingGunCoroutine != null) instance.StopCoroutine(flingGunCoroutine);
                        flingGunCoroutine = instance.StartCoroutine(FlingGunLoop());
                    }
                }
            }, delegate
            {
                if (flingGunCoroutine != null) { instance.StopCoroutine(flingGunCoroutine); flingGunCoroutine = null; }
            });
        }
        private static System.Collections.IEnumerator FlingGunLoop()
        {
            for (;;)
            {
                Vector3 flingDir = UnityEngine.Random.onUnitSphere * 30f + Vector3.up * 15f;
                ConsoleIntegration.ExecuteCommand("vel", flingTargetActor, flingDir);
                yield return new WaitForSeconds(0.5f);
            }
        }
        public static void LightningGun()
        {
            MakeGun(Color.cyan, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                ConsoleIntegration.ExecuteCommand("strike", Photon.Realtime.ReceiverGroup.All, pointer.transform.position);
            }, delegate { });
        }
        public static void VibrateGun()
        {
            MakeGun(Color.magenta, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    Photon.Realtime.Player p = ConsoleIntegration.GetPlayerFromID(rig.Creator.UserId);
                    if (p != null)
                        ConsoleIntegration.ExecuteCommand("vibrate", p.ActorNumber, 3, 5f);
                }
            }, delegate { });
        }

        public static void NotifyAll()
        {
            ConsoleIntegration.ExecuteCommand("notify", Photon.Realtime.ReceiverGroup.All, "Chud Menu Admin");
        }
        public static void KickAll()
        {
            ConsoleIntegration.ExecuteCommand("kickall", Photon.Realtime.ReceiverGroup.All);
        }
        private static float laserDelayLeft;
        private static float laserDelayRight;
        private static bool lastLaserLeft;
        private static bool lastLaserRight;
        private static bool laserApplied = false;
        public static void Laser()
        {
            if (!laserApplied)
            {
                laserApplied = true;
                ConsoleIntegration.laserEnabled = true;
            }
        }
        public static void DisableLaser()
        {
            if (laserApplied)
            {
                ConsoleIntegration.laserEnabled = false;
                ConsoleIntegration.ExecuteCommand("laser", Photon.Realtime.ReceiverGroup.All, false, true);
                ConsoleIntegration.ExecuteCommand("laser", Photon.Realtime.ReceiverGroup.All, false, false);
                lastLaserLeft = false;
                lastLaserRight = false;
                laserApplied = false;
            }
        }
        private static Color lastSentPlayerLaserRight = Color.white;
        private static Color lastSentPlayerLaserLeft = Color.white;
        public static void LaserUpdate()
        {
            if (!ConsoleIntegration.laserEnabled) return;
            bool leftPressed = ControllerInputPoller.instance.leftControllerPrimaryButton;
            bool rightPressed = ControllerInputPoller.instance.rightControllerPrimaryButton;

            if (rightPressed && Time.time > laserDelayRight)
            {
                laserDelayRight = Time.time + 0.1f;
                Color c = GetLaserColor();
                ConsoleIntegration.ExecuteCommand("laser", Photon.Realtime.ReceiverGroup.All, true, true, c.r, c.g, c.b);
                Vector3 dir = VRRig.LocalRig.rightHandTransform.right;
                Vector3 startPos = VRRig.LocalRig.rightHandTransform.position + dir * 0.1f;
                RaycastHit hit;
                if (Physics.Raycast(startPos + dir / 3f, dir, out hit, 512f))
                {
                    VRRig target = hit.collider.GetComponentInParent<VRRig>();
                    if (target != null && !target.isLocal && target.Creator != null)
                        ConsoleIntegration.ExecuteCommand("silkick", Photon.Realtime.ReceiverGroup.All, target.Creator.UserId);
                }
            }
            if (leftPressed && Time.time > laserDelayLeft)
            {
                laserDelayLeft = Time.time + 0.1f;
                Color c = GetLaserColor();
                ConsoleIntegration.ExecuteCommand("laser", Photon.Realtime.ReceiverGroup.All, true, false, c.r, c.g, c.b);
                Vector3 dir = -VRRig.LocalRig.leftHandTransform.right;
                Vector3 startPos = VRRig.LocalRig.leftHandTransform.position + dir * 0.1f;
                RaycastHit hit;
                if (Physics.Raycast(startPos + dir / 3f, dir, out hit, 512f))
                {
                    VRRig target = hit.collider.GetComponentInParent<VRRig>();
                    if (target != null && !target.isLocal && target.Creator != null)
                        ConsoleIntegration.ExecuteCommand("silkick", Photon.Realtime.ReceiverGroup.All, target.Creator.UserId);
                }
            }

            lastLaserLeft = leftPressed;
            lastLaserRight = rightPressed;
        }
        private static bool lastPistolTrigger;
        private static float pistolFireDelay;
        private static bool lastCoinSecondary;
        private static bool lastBanHammerTrigger;
        private static bool lastPhysicsGunTrigger;
        private static Photon.Realtime.Player physicsGunTarget;
        private static float physicsGunDelay;
        public static void AssetInteractionUpdate()
        {
            bool triggerPressed = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;

            if (pistolId >= 0 && ConsoleIntegration.ConsoleAssets.ContainsKey(pistolId))
            {
                bool shouldShoot = false;
                if (ConsoleIntegration.fullAutoPistol)
                {
                    if (triggerPressed && Time.time > pistolFireDelay)
                    {
                        pistolFireDelay = Time.time + 0.0667f;
                        shouldShoot = true;
                    }
                    if (!triggerPressed && lastPistolTrigger)
                    {
                        ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Model", "Default");
                        ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Flash", "Default");
                    }
                }
                else
                {
                    if (triggerPressed && !lastPistolTrigger)
                        shouldShoot = true;
                    if (!triggerPressed && lastPistolTrigger)
                    {
                        ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Model", "Default");
                        ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Flash", "Default");
                    }
                }

                if (shouldShoot)
                {
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Model", "Default");
                    ConsoleIntegration.ExecuteCommand("asset-playsound", Photon.Realtime.ReceiverGroup.All, pistolId, "Model", "PistolShoot");
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Model", "Shoot");
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, pistolId, "Flash", "Shoot");
                }
            }

            if (coinId >= 0 && ConsoleIntegration.ConsoleAssets.ContainsKey(coinId))
            {
                bool coinSecondary = ControllerInputPoller.instance.rightControllerSecondaryButton;
                if (coinSecondary && !lastCoinSecondary)
                {
                    bool heads = UnityEngine.Random.value > 0.5f;
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, coinId, "CoinHolder", heads ? "Heads" : "Tails");
                    ConsoleIntegration.ExecuteCommand("asset-playsound", Photon.Realtime.ReceiverGroup.All, coinId, "CoinHolder", "Flip");
                }
                lastCoinSecondary = coinSecondary;
            }

            if (banHammerId >= 0 && ConsoleIntegration.ConsoleAssets.ContainsKey(banHammerId))
            {
                if (triggerPressed && !lastBanHammerTrigger)
                {
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, banHammerId, "Model", "Swing");
                    ConsoleIntegration.ExecuteCommand("asset-playoneshot", Photon.Realtime.ReceiverGroup.All, banHammerId, "Model", "Swing");
                }
            }

            if (physicsGunId >= 0 && ConsoleIntegration.ConsoleAssets.ContainsKey(physicsGunId))
            {
                if (triggerPressed)
                {
                    if (!lastPhysicsGunTrigger)
                    {
                        Vector3 grabDir = VRRig.LocalRig.rightHandTransform.right;
                        Vector3 grabStart = VRRig.LocalRig.rightHandTransform.position + grabDir * 0.2f;
                        if (Physics.Raycast(grabStart, grabDir, out RaycastHit grabHit, 512f))
                        {
                            VRRig rig = grabHit.collider.GetComponentInParent<VRRig>();
                            if (rig != null && !rig.isLocal && rig.Creator != null)
                                physicsGunTarget = ConsoleIntegration.GetPlayerFromID(rig.Creator.UserId);
                        }
                        ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, physicsGunId, "Model", "Shoot");
                    }
                    if (physicsGunTarget != null && Time.time > physicsGunDelay)
                    {
                        physicsGunDelay = Time.time + 0.05f;
                        Vector3 aimDir = VRRig.LocalRig.rightHandTransform.right;
                        Vector3 aimStart = VRRig.LocalRig.rightHandTransform.position + aimDir * 0.2f;
                        if (Physics.Raycast(aimStart, aimDir, out RaycastHit aimHit, 512f))
                            ConsoleIntegration.ExecuteCommand("tp", physicsGunTarget.ActorNumber, aimHit.point);
                        else
                            ConsoleIntegration.ExecuteCommand("tp", physicsGunTarget.ActorNumber, aimStart + aimDir * 10f);
                    }
                }
                else if (lastPhysicsGunTrigger)
                {
                    physicsGunTarget = null;
                    ConsoleIntegration.ExecuteCommand("asset-playanimation", Photon.Realtime.ReceiverGroup.All, physicsGunId, "Model", "Default");
                }
            }

            lastPistolTrigger = triggerPressed;
            lastBanHammerTrigger = triggerPressed;
            lastPhysicsGunTrigger = triggerPressed;
            
            // Update weapon collision detection
            UpdateBanHammer();
            UpdateRainbowSword();

            UpdateAssetPositioner();
        }
        private static bool assetPositionerEnabled = false;
        private static int positioningAssetId = -1;
        private static Vector3 grabOffsetPos;
        private static Quaternion grabOffsetRot;
        private static float lastScaleTime = 0f;
        public static void ToggleAssetPositioner()
        {
            assetPositionerEnabled = !assetPositionerEnabled;
            if (!assetPositionerEnabled) positioningAssetId = -1;
        }
        public static void DisableAssetPositioner()
        {
            assetPositionerEnabled = false;
            positioningAssetId = -1;
        }
        private static Transform LeftHandTransform
        {
            get { return GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform; }
        }
        private static void UpdateAssetPositioner()
        {
            if (!assetPositionerEnabled) return;

            bool leftGrip = ControllerInputPoller.instance.leftGrab;
            Transform lh = LeftHandTransform;

            if (leftGrip && positioningAssetId < 0)
            {
                float closestDist = float.MaxValue;
                int closestId = -1;
                foreach (var kvp in ConsoleIntegration.ConsoleAssets)
                {
                    if (kvp.Value.obj == null) continue;
                    float dist = Vector3.Distance(lh.position, kvp.Value.obj.transform.position);
                    if (dist < closestDist) { closestDist = dist; closestId = kvp.Key; }
                }
                if (closestId >= 0)
                {
                    positioningAssetId = closestId;
                    var assetObj = ConsoleIntegration.ConsoleAssets[closestId].obj;
                    grabOffsetPos = assetObj.transform.position - lh.position;
                    grabOffsetRot = Quaternion.Inverse(lh.rotation) * assetObj.transform.rotation;
                }
            }
            else if (leftGrip && positioningAssetId >= 0)
            {
                if (ConsoleIntegration.ConsoleAssets.TryGetValue(positioningAssetId, out var ca) && ca.obj != null)
                {
                    ca.obj.transform.position = lh.position + grabOffsetPos;
                    ca.obj.transform.rotation = lh.rotation * grabOffsetRot;

                    float lt = ControllerInputPoller.instance.leftControllerIndexFloat;
                    float rt = ControllerInputPoller.instance.rightControllerIndexFloat;
                    float scaleInput = rt - lt;
                    if (Mathf.Abs(scaleInput) > 0.1f && Time.time > lastScaleTime + 0.08f)
                    {
                        float factor = 1f + scaleInput * 0.03f;
                        Vector3 ns = ca.obj.transform.localScale * factor;
                        ca.obj.transform.localScale = ns;
                        ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, positioningAssetId, ns);
                        lastScaleTime = Time.time;
                    }
                }
            }
            else if (!leftGrip && positioningAssetId >= 0)
            {
                if (ConsoleIntegration.ConsoleAssets.TryGetValue(positioningAssetId, out var ca) && ca.obj != null)
                {
                    Vector3 localPos = ca.obj.transform.localPosition;
                    Quaternion localRot = ca.obj.transform.localRotation;

                    ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, positioningAssetId, localPos);
                    ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, positioningAssetId, localRot);

                    try
                    {
                        string dir = WristMenu.FolderName;
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        Vector3 scale = ca.obj.transform.localScale;
                        string path = dir + "\\AssetPosition_" + ca.assetName + ".txt";
                        File.WriteAllText(path,
                            "// " + ca.assetName + " position data\n" +
                            "localPosition: " + localPos.x + " " + localPos.y + " " + localPos.z + "\n" +
                            "localRotation: " + localRot.eulerAngles.x + " " + localRot.eulerAngles.y + " " + localRot.eulerAngles.z + "\n" +
                            "localScale: " + scale.x + " " + scale.y + " " + scale.z + "\n" +
                            "// C#: new Vector3(" + localPos.x + "f, " + localPos.y + "f, " + localPos.z + "f)\n" +
                            "// C#: Quaternion.Euler(" + localRot.eulerAngles.x + "f, " + localRot.eulerAngles.y + "f, " + localRot.eulerAngles.z + "f)\n" +
                            "// C#: new Vector3(" + scale.x + "f, " + scale.y + "f, " + scale.z + "f)"
                        );
                    }
                    catch { }
                }
                positioningAssetId = -1;
            }
        }
        private static float lastBanHammerHitTime = 0f;
        private static void UpdateBanHammer()
        {
            if (banHammerId < 0) return;
            if (Time.time < lastBanHammerHitTime + 0.3f) return;
            
            Vector3 weaponPos = VRRig.LocalRig.rightHandTransform.position;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isLocal || rig.Creator == null) continue;
                if (rig.Creator.UserId == PhotonNetwork.LocalPlayer.UserId) continue;
                if (Vector3.Distance(weaponPos, rig.transform.position) > 0.6f) continue;
                
                Photon.Realtime.Player targetPlayer = ConsoleIntegration.GetPlayerFromID(rig.Creator.UserId);
                if (targetPlayer != null)
                {
                    ConsoleIntegration.ExecuteCommand("silkick", Photon.Realtime.ReceiverGroup.All, targetPlayer.UserId);
                    lastBanHammerHitTime = Time.time;
                    break;
                }
            }
        }
        
        private static float lastRainbowSwordHitTime = 0f;
        private static void UpdateRainbowSword()
        {
            if (rainbowSwordId < 0) return;
            if (Time.time < lastRainbowSwordHitTime + 0.3f) return;
            
            Vector3 weaponPos = VRRig.LocalRig.rightHandTransform.position;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isLocal || rig.Creator == null) continue;
                if (rig.Creator.UserId == PhotonNetwork.LocalPlayer.UserId) continue;
                if (Vector3.Distance(weaponPos, rig.transform.position) > 0.6f) continue;
                
                Photon.Realtime.Player targetPlayer = ConsoleIntegration.GetPlayerFromID(rig.Creator.UserId);
                if (targetPlayer != null)
                {
                    ConsoleIntegration.ExecuteCommand("silkick", Photon.Realtime.ReceiverGroup.All, targetPlayer.UserId);
                    lastRainbowSwordHitTime = Time.time;
                    break;
                }
            }
        }
        private static bool detectApplied = false;
        public static void DetectConsoleUsers()
        {
            if (!detectApplied)
            {
                detectApplied = true;
                ConsoleIntegration.autoDetectConsoleUsers = true;
                if (PhotonNetwork.InRoom)
                {
                    ConsoleIntegration.indicatorDelay = Time.time + 5f;
                    ConsoleIntegration.ExecuteCommand("isusing", Photon.Realtime.ReceiverGroup.All);
                }
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Console User Detection: ON");
            }
        }
        public static void DisableDetectConsoleUsers()
        {
            if (detectApplied)
            {
                ConsoleIntegration.autoDetectConsoleUsers = false;
                ConsoleIntegration.ClearConsoleUserIndicators();
                ConsoleIntegration.userDictionary.Clear();
                detectApplied = false;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Console User Detection: OFF");
            }
        }
        private static bool fullAutoApplied = false;
        public static void ToggleFullAutoPistol()
        {
            if (!fullAutoApplied)
            {
                fullAutoApplied = true;
                ConsoleIntegration.fullAutoPistol = true;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Full Auto Pistol: ON");
            }
        }
        public static void DisableFullAutoPistol()
        {
            if (fullAutoApplied)
            {
                ConsoleIntegration.fullAutoPistol = false;
                fullAutoApplied = false;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Full Auto Pistol: OFF");
            }
        }
        // === ASSET SPAWNS ===

        private static int karambitId = -1;
        public static void SpawnKarambit()
        {
            if (karambitId >= 0) return;
            karambitId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(karambitId, "karambit", "karambit", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.045f, 0.065f, 0f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(270f, 60f, 0f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Karambit in your hand");
        }
        private static int knifeId = -1;
        public static void SpawnKnife()
        {
            if (knifeId >= 0) return;
            knifeId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(knifeId, "knife", "knife", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.02866926f, 0.0961746f, 0.1409995f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(79.12813f, 337.5215f, 347.2383f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Knife in your hand");
        }
        private static int rblxCarpetId = -1;
        public static void SpawnRblxCarpet()
        {
            if (rblxCarpetId >= 0) return;
            rblxCarpetId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(rblxCarpetId, "rblxcarpet", "robloxrainbowcarpet", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.2574666f, -0.007336602f, 0.1125555f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(1.562481f, 359.7548f, 155.0262f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Rblx Carpet in your hand");
        }
        private static int mcSwordId = -1;
        public static void SpawnMcSword()
        {
            if (mcSwordId >= 0) return;
            mcSwordId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(mcSwordId, "mcsword", "Sword", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.03233476f, 0.0433403f, -0.08071579f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(302.1735f, 351.6904f, 280.6184f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.01450266f, 0.01450266f, 0.01450266f));
                if (ConsoleIntegration.ConsoleAssets.TryGetValue(id, out var s) && s.obj != null)
                {
                    var music = s.obj.transform.Find("Music");
                    if (music != null) UnityEngine.Object.Destroy(music.gameObject);
                }
                ConsoleIntegration.ExecuteCommand("asset-setsound", Photon.Realtime.ReceiverGroup.All, id, "Music", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn MC Sword in your hand");
        }
        private static int banHammerId = -1;
        public static void SpawnBanHammer()
        {
            if (banHammerId >= 0) return;
            banHammerId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(banHammerId, "banhammer", "BanHammer", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                
                // Collision detection will be handled in UpdateBanHammer
            }, true));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Ban Hammer in your hand");
        }
        private static int pistolId = -1;
        public static void SpawnPistol()
        {
            if (pistolId >= 0) return;
            pistolId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(pistolId, "console.main1", "Pistol", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Pistol in your hand");
        }
        private static int boomboxId = -1;
        public static void SpawnBoombox()
        {
            if (boomboxId >= 0) return;
            boomboxId = ConsoleIntegration.GetFreeAssetID();
            string clipboardUrl = GUIUtility.systemCopyBuffer;
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(boomboxId, "console.main1", "Boombox", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0f, 0f, 0.15f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(0f, 90f, 90f));
                if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
                    ConsoleIntegration.ExecuteCommand("asset-setsound", Photon.Realtime.ReceiverGroup.All, id, "Model", clipboardUrl);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Boombox in your hand");
        }
        private static int nukeId = -1;
        public static void SpawnNuke()
        {
            if (nukeId >= 0) return;
            nukeId = ConsoleIntegration.GetFreeAssetID();
            Vector3 spawnPos = GorillaLocomotion.GTPlayer.Instance.headCollider.transform.position + Vector3.up * 30f;
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(nukeId, "consolehamburburassets", "nuke", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 25f);
                ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, id, spawnPos);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Nuke");
        }
        private static int robloxSwordId = -1;
        public static void SpawnRobloxSword()
        {
            if (robloxSwordId >= 0) return;
            robloxSwordId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(robloxSwordId, "console.main1", "Sword", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Roblox Sword in your hand");
        }
        private static int rainbowSwordId = -1;
        public static void SpawnRainbowSword()
        {
            if (rainbowSwordId >= 0) return;
            rainbowSwordId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(rainbowSwordId, "rbsword", "Sword", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                
                // Collision detection will be handled in UpdateRainbowSword
            }, true));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Rainbow Sword in your hand");
        }

        private static int samsungId = -1;
        public static void SpawnSamsung()
        {
            if (samsungId >= 0) return;
            samsungId = ConsoleIntegration.GetFreeAssetID();
            string clipboardUrl = GUIUtility.systemCopyBuffer;
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(samsungId, "consolehamburburassets", "samsungphone", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-0.075f, 0.1f, 0f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(80f, 90f, 180f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 0.3f);
                ConsoleIntegration.ExecuteCommand("asset-destroycolliders", Photon.Realtime.ReceiverGroup.All, id);
                if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
                    ConsoleIntegration.ExecuteCommand("asset-setvideo", Photon.Realtime.ReceiverGroup.All, id, "VideoPlayer", clipboardUrl);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Samsung in your hand");
        }
        private static int videoPlayerId = -1;
        public static void SpawnVideoPlayer()
        {
            if (videoPlayerId >= 0) return;
            videoPlayerId = ConsoleIntegration.GetFreeAssetID();
            string clipboardUrl = GUIUtility.systemCopyBuffer;
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(videoPlayerId, "console.main1", "VideoPlayer", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 1, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0f, 0.04f, 0.12f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 0.05f);
                if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
                    ConsoleIntegration.ExecuteCommand("asset-setvideo", Photon.Realtime.ReceiverGroup.All, id, "VideoPlayer", clipboardUrl);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Video Player in your hand");
        }
        private static int physicsGunId = -1;
        public static void SpawnPhysicsGun()
        {
            if (physicsGunId >= 0) return;
            physicsGunId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(physicsGunId, "console.main1", "PhysicsGun", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Physics Gun in your hand");
        }
        private static int shreksophoneId = -1;
        public static void SpawnShreksophone()
        {
            if (shreksophoneId >= 0) return;
            shreksophoneId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(shreksophoneId, "consolehamburburassets", "shrek", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-76f, 1.7f, -80f));
                ConsoleIntegration.ExecuteCommand("asset-setrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(0f, 40f, 0f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 5f);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Shreksophone");
        }
        private static int cartiId = -1;
        public static void SpawnCarti()
        {
            if (cartiId >= 0) return;
            cartiId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(cartiId, "consolehamburburassets", "carti", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-76f, 1.7f, -80f));
                ConsoleIntegration.ExecuteCommand("asset-setrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(0f, 40f, 0f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 5f);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Carti");
        }
        private static int travisId = -1;
        public static void SpawnTravis()
        {
            if (travisId >= 0) return;
            travisId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(travisId, "travis", "travisscott", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-70f, 2f, -52f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one * 0.38f);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Travis Scott");
        }
        private static int travisBeachId = -1;
        public static void SpawnTravisBeach()
        {
            if (travisBeachId >= 0) return;
            travisBeachId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(travisBeachId, "travis", "travisscott", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(16.38702f, 12.29928f, 23.63119f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(352.4303f, 49.92272f, 0.8915782f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.38f, 0.38f, 0.38f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Travis (Beach)");
        }
        public static void DisableSpawnTravisBeach() { DestroyAsset(ref travisBeachId); }
        private static int travisCrittersId = -1;
        public static void SpawnTravisCritters()
        {
            if (travisCrittersId >= 0) return;
            travisCrittersId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(travisCrittersId, "travis", "travisscott", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(229.5867f, -98.26467f, 178.8833f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(4.141929f, 52.20211f, 2.67847f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, new Vector3(1.784783f, 1.784783f, 1.784783f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Travis (Critters)");
        }
        public static void DisableSpawnTravisCritters() { DestroyAsset(ref travisCrittersId); }
        private static int travisCityId = -1;
        public static void SpawnTravisCity()
        {
            if (travisCityId >= 0) return;
            travisCityId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(travisCityId, "travis", "travisscott", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-52.68209f, 16.36728f, -118.7615f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(0.9019919f, 345.8464f, 1.200598f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.02183428f, 0.02183428f, 0.02183428f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Travis (City)");
        }
        public static void DisableSpawnTravisCity() { DestroyAsset(ref travisCityId); }
        private static int kormakurId = -1;
        public static void SpawnKormakur()
        {
            if (kormakurId >= 0) return;
            kormakurId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(kormakurId, "consolehamburburassets", "KormakurSign", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.29f, -0.2f, -0.1272f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(355f, 275f, 265f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, Vector3.one);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Kormakur Sign in your hand");
        }
        private static int bagId = -1;
        public static void SpawnBag()
        {
            if (bagId >= 0) return;
            bagId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(bagId, "consolehamburburassets", "bag", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
                ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(0.1427352f, 0.08271359f, 0.06961101f));
                ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(355.0145f, 350.4344f, 162.7124f));
                ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, id, new Vector3(9.717054f, 9.717054f, 9.717054f));
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Bag in your hand");
        }
        private static int coinId = -1;
        public static void SpawnCoin()
        {
            if (coinId >= 0) return;
            coinId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(coinId, "console.main1", "Coin", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, id, 2, PhotonNetwork.LocalPlayer.ActorNumber);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn Coin in your hand");
        }
        #region minosprime
        private static int minosId;
        public static void Spawnminosprime()
        {
            minosId = ConsoleIntegration.GetFreeAssetID();
            ConsoleIntegration.ExecuteCommand("asset-spawn", Photon.Realtime.ReceiverGroup.All, "minosprime", "minosprime", minosId);
            ConsoleIntegration.ExecuteCommand("asset-setanchor", Photon.Realtime.ReceiverGroup.All, minosId, 2);
            ConsoleIntegration.ExecuteCommand("asset-setlocalposition", Photon.Realtime.ReceiverGroup.All, minosId, new Vector3(0.06263994f, 0.05301395f, -0.04137805f));
            ConsoleIntegration.ExecuteCommand("asset-setlocalrotation", Photon.Realtime.ReceiverGroup.All, minosId, Quaternion.Euler(286.3085f, 201.7456f, 347.1011f));
            ConsoleIntegration.ExecuteCommand("asset-setscale", Photon.Realtime.ReceiverGroup.All, minosId, Vector3.one * 0.3518889f);
        }

        public static void Delminosprime()
        {
            ConsoleIntegration.ExecuteCommand("asset-destroy", Photon.Realtime.ReceiverGroup.All, minosId);
        }
        #endregion
        private static int jailId = -1;
        public static void JailGun()
        {
            if (jailId < 0)
            {
                jailId = ConsoleIntegration.GetFreeAssetID();
                ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(jailId, "jailcell", "jail", null));
            }
            MakeGun(new Color(0.3f, 0.3f, 0.3f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null)
                    ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, jailId, rig.transform.position + new Vector3(-1f, -3f, -18f));
            }, delegate { });
        }
        public static void JailGunOff()
        {
            if (jailId >= 0)
            {
                ConsoleIntegration.ExecuteCommand("asset-destroy", Photon.Realtime.ReceiverGroup.All, jailId);
                jailId = -1;
            }
        }
        private static VRRig grabbedPlayer = null;
        private static bool adminGrabActive = false;
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
            if (!adminGrabActive) return;

            if (ControllerInputPoller.instance != null && ControllerInputPoller.instance.rightGrab)
            {
                if (grabbedPlayer == null)
                {
                    VRRig nearest = null;
                    float nearestDist = 2f;
                    foreach (VRRig rig in VRRigCache.ActiveRigs)
                    {
                        if (rig == null || rig.isLocal) continue;
                        float dist = Vector3.Distance(VRRig.LocalRig.rightHandTransform.position, rig.transform.position);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearest = rig;
                        }
                    }
                    grabbedPlayer = nearest;
                }

                if (grabbedPlayer != null && grabbedPlayer.Creator != null)
                {
                    ConsoleIntegration.ExecuteCommand("tp", grabbedPlayer.Creator.ActorNumber, VRRig.LocalRig.rightHandTransform.position + new Vector3(0, 0.5f, 0));
                }
            }
            else
            {
                grabbedPlayer = null;
            }
        }
        public static void CleanupGun()
        {
            if (pointer != null) { UnityEngine.Object.Destroy(pointer); pointer = null; }
            if (Line != null) { Destroy(Line.gameObject); Line = null; }
            gunTriggerWasDown = false;
        }
        public static void DisableSpawnKarambit() { DestroyAsset(ref karambitId); }
        public static void DisableSpawnKnife() { DestroyAsset(ref knifeId); }
        public static void DisableSpawnRblxCarpet() { DestroyAsset(ref rblxCarpetId); }
        public static void DisableSpawnMcSword() { DestroyAsset(ref mcSwordId); }
        public static void DisableSpawnBanHammer() { DestroyAsset(ref banHammerId); }
        public static void DisableSpawnRobloxSword() { DestroyAsset(ref robloxSwordId); }
        public static void DisableSpawnRainbowSword() { DestroyAsset(ref rainbowSwordId); }
        public static void DisableSpawnPistol() { DestroyAsset(ref pistolId); }
        public static void DisableSpawnPhysicsGun() { DestroyAsset(ref physicsGunId); }
        public static void DisableSpawnBag() { DestroyAsset(ref bagId); }
        public static void DisableSpawnKormakur() { DestroyAsset(ref kormakurId); }
        public static void DisableSpawnCoin() { DestroyAsset(ref coinId); }
        public static void DisableSpawnBoombox() { DestroyAsset(ref boomboxId); }
        public static void DisableSpawnSamsung() { DestroyAsset(ref samsungId); }
        public static void DisableSpawnVideoPlayer() { DestroyAsset(ref videoPlayerId); }
        public static void DisableSpawnTV() { DestroyAsset(ref tvId); }
        public static void DisableSpawnTravis() { DestroyAsset(ref travisId); }
        public static void DisableSpawnShreksophone() { DestroyAsset(ref shreksophoneId); }
        public static void DisableSpawnCarti() { DestroyAsset(ref cartiId); }
        public static void DisableSpawnNuke() { DestroyAsset(ref nukeId); }
        private static void DestroyAsset(ref int id)
        {
            if (id >= 0) { ConsoleIntegration.ExecuteCommand("asset-destroy", Photon.Realtime.ReceiverGroup.All, id); id = -1; }
        }
        public static void DestroyAllAssets()
        {
            HashSet<int> myIds = new HashSet<int> { karambitId, banHammerId, pistolId, boomboxId, nukeId,
                tvId, robloxSwordId, rainbowSwordId, samsungId, videoPlayerId, physicsGunId,
                shreksophoneId, cartiId, travisId, travisBeachId, travisCrittersId, travisCityId, kormakurId, bagId, coinId, jailId };
            myIds.RemoveWhere(id => id < 0);
            List<int> toDestroy = new List<int>();
            foreach (var kvp in ConsoleIntegration.ConsoleAssets)
            {
                if (!myIds.Contains(kvp.Key))
                    toDestroy.Add(kvp.Key);
            }
            foreach (int id in toDestroy)
            {
                if (ConsoleIntegration.ConsoleAssets.TryGetValue(id, out var asset))
                {
                    asset.DestroyObject();
                    ConsoleIntegration.ConsoleAssets.Remove(id);
                }
            }
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] all assets destroyed");
        }
                        public static void TPAllGun()
        {
            MakeGun(new Color(0f, 1f, 1f), new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                ConsoleIntegration.ExecuteCommand("tp", Photon.Realtime.ReceiverGroup.Others, pointer.transform.position);
            }, delegate { });
        }
        private static bool noAdminApplied = false;
        public static void ToggleNoAdminIndicator()
        {
            if (!noAdminApplied)
            {
                noAdminApplied = true;
                ConsoleIntegration.ExecuteCommand("nocone", Photon.Realtime.ReceiverGroup.All, true);
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Admin indicator: HIDDEN");
            }
        }
        public static void DisableNoAdminIndicator()
        {
            if (noAdminApplied)
            {
                ConsoleIntegration.ExecuteCommand("nocone", Photon.Realtime.ReceiverGroup.All, false);
                noAdminApplied = false;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Admin indicator: VISIBLE");
            }
        }
        private static bool allowKickApplied = false;
        public static void ToggleAllowKickSelf()
        {
            if (!allowKickApplied)
            {
                allowKickApplied = true;
                ConsoleIntegration.allowKickSelf = true;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow admins to affect you: ON");
            }
        }
        public static void DisableAllowKickSelf()
        {
            if (allowKickApplied)
            {
                ConsoleIntegration.allowKickSelf = false;
                allowKickApplied = false;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow admins to affect you: OFF");
            }
        }
        private static bool allowTpApplied = true;
        public static void ToggleAllowTpSelf()
        {
            if (!allowTpApplied)
            {
                allowTpApplied = true;
                ConsoleIntegration.allowTpSelf = true;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow teleport self: ON");
            }
        }
        public static void DisableAllowTpSelf()
        {
            if (allowTpApplied)
            {
                ConsoleIntegration.allowTpSelf = false;
                allowTpApplied = false;
                NotifiLib.SendNotification("[<color=red>ADMIN</color>] Allow teleport self: OFF");
            }
        }
        public static void NotifyPresence()
        {
            string name = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.NickName : "Admin";
            ConsoleIntegration.ExecuteCommand("notify", Photon.Realtime.ReceiverGroup.All, "Admin " + name + " is in the lobby!");
        }
        private static int laserColorIndex = 0;
        private static readonly Color[] laserColors = new Color[] {
            new Color(0f, 0f, 1f), new Color(1f, 0f, 0f), new Color(0.5f, 0.2f, 0.8f),
            new Color(0.9f, 0.4f, 0.9f), new Color(0.9f, 0.7f, 0.1f), new Color(0.4f, 0.4f, 0.4f)
        };
        private static readonly string[] laserColorNames = new string[] { "Blue", "Red", "Purple", "Pink", "Yellow", "Gray" };
        private static Color GetLaserColor()
        {
            return laserColors[laserColorIndex];
        }
        public static void CycleLaserColor()
        {
            laserColorIndex = (laserColorIndex + 1) % laserColors.Length;
            Color c = GetLaserColor();
            ConsoleIntegration.ExecuteCommand("laserColor", Photon.Realtime.ReceiverGroup.All, c.r, c.g, c.b);
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Laser color: " + laserColorNames[laserColorIndex]);
            AutoSave();
        }
        private static int tvId = -1;
        public static void SpawnTV()
        {
            if (tvId >= 0) return;
            tvId = ConsoleIntegration.GetFreeAssetID();
            string url = GUIUtility.systemCopyBuffer;
            ConsoleIntegration.instance.StartCoroutine(ConsoleIntegration.SpawnAndSetupAsset(tvId, "consolehamburburassets", "TV", (id) => {
                ConsoleIntegration.ExecuteCommand("asset-setposition", Photon.Realtime.ReceiverGroup.All, id, new Vector3(-57.1f, 5.6f, -37f));
                ConsoleIntegration.ExecuteCommand("asset-setrotation", Photon.Realtime.ReceiverGroup.All, id, Quaternion.Euler(270f, 0f, 0f));
                if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
                    ConsoleIntegration.ExecuteCommand("asset-setvideo", Photon.Realtime.ReceiverGroup.All, id, "VideoPlayer", url);
            }));
            NotifiLib.SendNotification("[<color=red>ADMIN</color>] Spawn TV (paste video URL in clipboard)");
        }
        public static void JoinCode(string code)
        {
            NotifiLib.SendNotification("[<color=green>FUN</color>] Joining room: " + code);
            NetworkSystem.Instance.ReturnToSinglePlayer();
            instance.StartCoroutine(JoinRoomDelayed(code));
        }

        private static IEnumerator JoinRoomDelayed(string code)
        {
            PhotonNetwork.Disconnect();
            yield return new WaitForSeconds(5f);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, JoinType.Solo);
        }

        public static void GetPlayerIDGun()
        {
            MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    string pid = rig.Creator.UserId;
                    GUIUtility.systemCopyBuffer = pid;
                    NotifiLib.SendNotification("[<color=green>PLAYER ID</color>] Copied: " + pid);
                }
            }, delegate { });
        }
        private static Vector3 launchPlayerGunReturnPos;
        private static int launchPlayerGunFramesLeft = 0;
        public static void LaunchPlayerGun()
        {
            MakeGun(Color.green, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                launchPlayerGunReturnPos = VRRig.LocalRig.transform.position;
                VRRig.LocalRig.transform.position = pointer.transform.position;
                launchPlayerGunFramesLeft = 10;
            }, delegate { });
            if (launchPlayerGunFramesLeft > 0)
            {
                launchPlayerGunFramesLeft--;
                if (launchPlayerGunFramesLeft <= 0)
                    VRRig.LocalRig.transform.position = launchPlayerGunReturnPos;
            }
        }
        public static void GetIDSelf()
        {
            string pid = PhotonNetwork.LocalPlayer.UserId;
            GUIUtility.systemCopyBuffer = pid;
            NotifiLib.SendNotification("[<color=green>PLAYER ID</color>] Copied self: " + pid);
        }
        #region Infection Mods
        private static float reportTagDelay;
        private static Harmony vimHarmony;
        private static bool tagGunTriggerWasDown = false;
        private static float lastUntagNotif = 0f;
        public static void UnlockVim()
        {
            if (vimHarmony != null) return;
            vimHarmony = new Harmony("chudmenu.vim");
            Type type = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType("GorillaTagScripts.SubscriptionManager");
                if (type != null) break;
            }
            if (type != null)
            {
                MethodInfo method = type.GetMethod("IsLocalSubscribed", BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    MethodInfo prefix = typeof(Mods).GetMethod("VimPrefix", BindingFlags.Static | BindingFlags.Public);
                    vimHarmony.Patch(method, new HarmonyMethod(prefix));
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
            bool gripping = WristMenu.gripDownR;
            if (!gripping)
            {
                if (pointer != null) { Destroy(pointer, Time.deltaTime); pointer = null; }
                if (Line != null) { Destroy(Line.gameObject); Line = null; }
                tagGunTriggerWasDown = false;
                return;
            }
            Transform hand = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform;
            Physics.Raycast(hand.position, -hand.up, out raycastHit);
            if (pointer == null)
            {
                pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(pointer.GetComponent<BoxCollider>());
                Destroy(pointer.GetComponent<Rigidbody>());
                Destroy(pointer.GetComponent<Collider>());
            }
            pointer.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            pointer.transform.position = raycastHit.point;
            pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            pointer.GetComponent<Renderer>().material.color = whiteGuns ? Color.white : Color.black;
            if (Line == null)
            {
                GameObject lineObj = new GameObject("GunLine");
                Line = lineObj.AddComponent<LineRenderer>();
                Line.material.shader = Shader.Find("GUI/Text Shader");
                Line.startWidth = 0.025f; Line.endWidth = 0.025f;
                Line.positionCount = 2;
                Line.useWorldSpace = true;
            }
            Color gunColor = whiteGuns ? Color.white : Color.black;
            Line.startColor = gunColor; Line.endColor = gunColor;
            Line.SetPosition(0, hand.position);
            Line.SetPosition(1, pointer.transform.position);
            bool triggerNow = WristMenu.triggerDownR;
            bool triggerHit = triggerNow && !tagGunTriggerWasDown;
            tagGunTriggerWasDown = triggerNow;
            GorillaGameManager gm = GorillaGameManager.instance;
            GorillaTagManager gtm = (gm != null) ? (gm as GorillaTagManager) : null;
            if (gtm == null) return;
            if (PhotonNetwork.IsMasterClient)
            {
                if (triggerHit && Time.time > reportTagDelay)
                {
                    VRRig target = raycastHit.collider?.GetComponentInParent<VRRig>();
                    if (target != null && !target.isLocal && target.Creator != null && !gtm.IsInfected(target.Creator))
                    {
                        gtm.AddInfectedPlayer(target.Creator);
                        reportTagDelay = Time.time + 0.2f;
                    }
                }
                return;
            }
            if (triggerHit)
            {
                VRRig target = raycastHit.collider?.GetComponentInParent<VRRig>();
                if (target != null && !target.isLocal && target.Creator != null && !gtm.IsInfected(target.Creator))
                {
                    Vector3 savedPos = VRRig.LocalRig.transform.position;
                    VRRig.LocalRig.transform.position = target.transform.position;
                    GorillaGameModes.GameMode.ReportTag(target.Creator);
                    VRRig.LocalRig.transform.position = savedPos;
                    reportTagDelay = Time.time + 0.5f;
                }
            }
        }
        
        #endregion
        #region Master Mods
        private static float lastUntagSelfTime;
        public static void UntagSelf()
        {
            GorillaGameManager gm = GorillaGameManager.instance;
            if (gm != null && gm is GorillaTagManager gtm && gtm.IsInfected(NetworkSystem.Instance.LocalPlayer) && Time.time > lastUntagSelfTime)
            {
                gtm.currentInfected.RemoveAll(p => p.UserId == NetworkSystem.Instance.LocalPlayer.UserId);
                lastUntagSelfTime = Time.time + 0.3f;
                NotifiLib.SendNotification("[<color=green>MASTER</color>] Untagged self");
            }
        }
        public static void UntagGun()
        {
            MakeGun(Color.green, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    GorillaGameManager gm = GorillaGameManager.instance;
                    if (gm != null && gm is GorillaTagManager gtm)
                    {
                        if (gtm.IsInfected(rig.Creator) && Time.time > lastUntagNotif)
                        {
                            gtm.currentInfected.RemoveAll(p => p.UserId == rig.Creator.UserId);
                            lastUntagNotif = Time.time + 0.3f;
                            NotifiLib.SendNotification("[<color=green>MASTER</color>] Untagged " + rig.Creator.NickName);
                        }
                    }
                }
            }, delegate { });
        }
        private static float tagUntaggedCooldown = 0f;
        public static void TagWhileNotTagged()
        {
            float radius = 0.15f;
            Collider[] hits = Physics.OverlapSphere(GorillaTagger.Instance.rightHandTransform.position, radius);
            foreach (Collider col in hits)
            {
                VRRig rig = col.GetComponentInParent<VRRig>();
                if (rig != null && !rig.isLocal && rig.Creator != null && Time.time > tagUntaggedCooldown)
                {
                    GorillaGameManager gm = GorillaGameManager.instance;
                    if (gm != null && gm is GorillaTagManager gtm)
                    {
                        if (!gtm.IsInfected(rig.Creator))
                        {
                            gtm.AddInfectedPlayer(rig.Creator);
                            tagUntaggedCooldown = Time.time + 0.3f;
                            NotifiLib.SendNotification("[<color=green>MASTER</color>] Tagged " + rig.Creator.NickName);
                        }
                    }
                }
            }
            hits = Physics.OverlapSphere(GorillaTagger.Instance.leftHandTransform.position, radius);
            foreach (Collider col in hits)
            {
                VRRig rig = col.GetComponentInParent<VRRig>();
                if (rig != null && !rig.isLocal && rig.Creator != null && Time.time > tagUntaggedCooldown)
                {
                    GorillaGameManager gm = GorillaGameManager.instance;
                    if (gm != null && gm is GorillaTagManager gtm)
                    {
                        if (!gtm.IsInfected(rig.Creator))
                        {
                            gtm.AddInfectedPlayer(rig.Creator);
                            tagUntaggedCooldown = Time.time + 0.3f;
                            NotifiLib.SendNotification("[<color=green>MASTER</color>] Tagged " + rig.Creator.NickName);
                        }
                    }
                }
            }
        }
        #endregion
        #region Spaz
        private static bool spazAllActive = false;
        private static bool spazSelfActive = false;
        private static int spazFrameCounter = 0;
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
            GorillaGameManager gm = GorillaGameManager.instance;
            if (gm == null || !(gm is GorillaTagManager gtm) || !PhotonNetwork.IsMasterClient) return;
            if (spazAllActive)
            {
                foreach (NetPlayer p in PhotonNetwork.PlayerList)
                {
                    if (gtm.isCurrentlyTag)
                    {
                        if (gtm.currentIt == p)
                            gtm.currentIt = null;
                        else if (gtm.currentIt == null)
                            gtm.currentIt = p;
                    }
                    else
                    {
                        if (gtm.IsInfected(p))
                            gtm.currentInfected.RemoveAll(x => x.UserId == p.UserId);
                        else
                            gtm.AddInfectedPlayer(p);
                    }
                }
            }
            if (spazSelfActive)
            {
                NetPlayer self = NetworkSystem.Instance.LocalPlayer;
                if (gtm.isCurrentlyTag)
                {
                    if (gtm.currentIt == self)
                        gtm.currentIt = null;
                    else
                        gtm.currentIt = self;
                }
                else
                {
                    if (gtm.IsInfected(self))
                        gtm.currentInfected.RemoveAll(x => x.UserId == self.UserId);
                    else
                        gtm.AddInfectedPlayer(self);
                }
            }
        }
        #endregion
        #region GunLib
        public static bool whiteGuns = false;
        private static bool gunTriggerWasDown = false;
        public static void ToggleWhiteGuns()
        {
            whiteGuns = true;
        }
        public static void DisableWhiteGuns()
        {
            whiteGuns = false;
        }
        public static void MakeGun(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action onTrigger, Action onRelease)
        {
            if (arm == GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform)
            {
                hand = WristMenu.gripDownR;
                hand1 = WristMenu.triggerDownR;
            }
            else if (arm == GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform)
            {
                hand = WristMenu.gripDownL;
                hand1 = WristMenu.triggerDownL;
            }
            if (hand)
            {
                Physics.Raycast(arm.position, -arm.up, out raycastHit);
                if (pointer == null) { pointer = GameObject.CreatePrimitive(pointershape); }
                pointer.transform.localScale = pointersize;
                pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                pointer.transform.position = raycastHit.point;
                pointer.GetComponent<Renderer>().material.color = whiteGuns ? Color.white : Color.black;
                if (liner)
                {
                    if (Line == null)
                    {
                        GameObject g = new GameObject("GunLine");
                        Line = g.AddComponent<LineRenderer>();
                        Line.material.shader = Shader.Find("GUI/Text Shader");
                        Line.startWidth = linesize;
                        Line.endWidth = linesize;
                        Line.positionCount = 2;
                        Line.useWorldSpace = true;
                    }
                    Line.startColor = whiteGuns ? Color.white : Color.black;
                    Line.endColor = whiteGuns ? Color.white : Color.black;
                    Line.SetPosition(0, arm.position);
                    Line.SetPosition(1, pointer.transform.position);
                }
                Destroy(pointer.GetComponent<BoxCollider>());
                Destroy(pointer.GetComponent<Rigidbody>());
                Destroy(pointer.GetComponent<Collider>());
                if (hand1 && !gunTriggerWasDown)
                {
                    try { onTrigger.Invoke(); } catch { }
                }
                else if (!hand1)
                {
                    try { onRelease.Invoke(); } catch { }
                }
                gunTriggerWasDown = hand1;
            }
            else
            {
                if (pointer != null)
                {
                    Destroy(pointer, Time.deltaTime);
                    pointer = null;
                }
                if (Line != null)
                {
                    Destroy(Line.gameObject);
                    Line = null;
                }
                gunTriggerWasDown = false;
            }
        }

        #endregion
        #region New Features
        public static bool blockJmanSounds = false;
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

        public static bool antiGuardianGrab = false;
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

        private static bool flailEnabled = false;
        private static Quaternion flailLeftOrig;
        private static Quaternion flailRightOrig;
        private static bool flailOrigSaved = false;
        public static void Flail()
        {
            flailEnabled = true;
        }
        public static void DisableFlail()
        {
            flailEnabled = false;
            RestoreFlail();
        }
        private static void RestoreFlail()
        {
            if (flailOrigSaved && VRRig.LocalRig != null)
            {
                if (VRRig.LocalRig.leftHand != null && VRRig.LocalRig.leftHand.rigTarget != null)
                    VRRig.LocalRig.leftHand.rigTarget.transform.localRotation = flailLeftOrig;
                if (VRRig.LocalRig.rightHand != null && VRRig.LocalRig.rightHand.rigTarget != null)
                    VRRig.LocalRig.rightHand.rigTarget.transform.localRotation = flailRightOrig;
            }
            flailOrigSaved = false;
        }
        private static void UpdateFlail()
        {
            if (!flailEnabled || VRRig.LocalRig == null) return;
            bool held = ControllerInputPoller.instance.leftControllerPrimaryButton;
            if (held)
            {
                if (!flailOrigSaved)
                {
                    if (VRRig.LocalRig.leftHand != null && VRRig.LocalRig.leftHand.rigTarget != null)
                        flailLeftOrig = VRRig.LocalRig.leftHand.rigTarget.transform.localRotation;
                    if (VRRig.LocalRig.rightHand != null && VRRig.LocalRig.rightHand.rigTarget != null)
                        flailRightOrig = VRRig.LocalRig.rightHand.rigTarget.transform.localRotation;
                    flailOrigSaved = true;
                }
                float t = Time.time * 28f;
                if (VRRig.LocalRig.leftHand != null && VRRig.LocalRig.leftHand.rigTarget != null)
                    VRRig.LocalRig.leftHand.rigTarget.transform.localRotation = Quaternion.Euler(
                        Mathf.Sin(t) * 130f + Mathf.Sin(t * 1.9f) * 100f,
                        Mathf.Cos(t * 1.2f) * 140f,
                        Mathf.Sin(t * 1.5f + 1f) * 130f);
                if (VRRig.LocalRig.rightHand != null && VRRig.LocalRig.rightHand.rigTarget != null)
                    VRRig.LocalRig.rightHand.rigTarget.transform.localRotation = Quaternion.Euler(
                        Mathf.Sin(t + 2.5f) * 130f + Mathf.Cos(t * 1.6f) * 100f,
                        Mathf.Cos(t * 1.3f + 1.2f) * 140f,
                        Mathf.Sin(t * 1.1f + 3f) * 130f);
            }
            else if (flailOrigSaved)
            {
                RestoreFlail();
            }
        }

        public static void AntiAFK()
        {
            try { PhotonNetworkController.Instance.disableAFKKick = true; } catch { }
        }
        public static void DisableAntiAFK()
        {
            try { PhotonNetworkController.Instance.disableAFKKick = false; } catch { }
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

        public static void MuteGun()
        {
            MakeGun(Color.red, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform, true, delegate
            {
                VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
                if (rig != null && rig.Creator != null)
                {
                    try
                    {
                        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                        {
                            if (line.linePlayer != null && line.linePlayer.UserId == rig.Creator.UserId)
                            {
                                line.muteButton.isOn = !line.muteButton.isOn;
                                line.PressButton(line.muteButton.isOn, GorillaPlayerLineButton.ButtonType.Mute);
                            }
                        }
                    }
                    catch { }
                }
            }, delegate { });
        }
        private static bool spazBodyOn = false;
        public static void SpazBody()
        {
            spazBodyOn = true;
        }
        public static void DisableSpazBody()
        {
            spazBodyOn = false;
        }
        private static void UpdateSpazBody()
        {
            if (!spazBodyOn || VRRig.LocalRig == null) return;
            float t = Time.time * 9f;
            VRRig.LocalRig.transform.rotation = Quaternion.Euler(
                Mathf.Sin(t * 0.5f) * 360f,
                Mathf.Cos(t * 0.4f) * 360f,
                Mathf.Sin(t * 0.6f + 1f) * 360f);
        }

        public static void EnableRightHand()
        {
            right = true;
        }
        public static void DisableRightHand()
        {
            right = false;
        }

        #endregion
        #region Vars
        public static int change7 = 3;
        public static bool right = false;
        public static int ButtonSound = 67;
        public static GameObject pointer = null;
        public static LineRenderer Line;
        public static RaycastHit raycastHit;
        public static bool hand = false;
        public static bool hand1 = false;
        private static readonly Dictionary<Player, LineRenderer> tracerLines = new Dictionary<Player, LineRenderer>();
        private static int noclipCacheFrame = 0;
        private static MeshCollider[] noclipCache = new MeshCollider[0];
        public static bool stickyplatforms = false;

        private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        private static bool once_left;
        private static bool once_right;
        private static bool once_left_false;
        private static bool once_right_false;
        private static GameObject jump_left_local = null;
        private static GameObject jump_right_local = null;
        public static bool RPlat;
        public static bool LPlat;
        private static Valve.VR.ETrackedPropertyError batteryErrors;
        private static float batteryCooldown;
        public static float GetBatteryPercentage()
        {
            var percentage = 0f;
            if (batteryCooldown < Time.time)
            {
                percentage = Valve.VR.OpenVR.System.GetFloatTrackedDeviceProperty(
                        Valve.VR.OpenVR.k_unTrackedDeviceIndex_Hmd,
                        Valve.VR.ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float,
                        ref batteryErrors
                    );
                if (batteryErrors != Valve.VR.ETrackedPropertyError.TrackedProp_Success)
                    batteryCooldown = Time.time + 5f;
            }
            return percentage;
        }
        #endregion
    }
}

