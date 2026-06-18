using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using GTAG_NotificationLib;

namespace Chud.Backend;

public static class ConsoleMods
{
	static ConsoleMods()
	{
		if (PlayerPrefs.GetInt("ChudMuteRainbowSword", 0) == 1)
		{
			Console.muteRainbowSword = true;
		}
	}
	// ====== Helpers that play locally once + sync to others (avoids double-handle from ReceiverGroup.All) ======
	private static void PlaySound(int id, string path, string clipName)
	{
		Console.ExecuteCommand("asset-playsound", (ReceiverGroup)0, id, path, clipName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playsound", id, path, clipName }, "asset-playsound");
	}

	private static void PlayAnimation(int id, string objectName, string animationName)
	{
		Console.ExecuteCommand("asset-playanimation", (ReceiverGroup)0, id, objectName, animationName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playanimation", id, objectName, animationName }, "asset-playanimation");
	}

	private static void SendLaser(bool enabled, bool rightHand, float r, float g, float b)
	{
		Console.ExecuteCommand("laser", (ReceiverGroup)0, enabled, rightHand, r, g, b);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "laser", enabled, rightHand, r, g, b }, "laser");
	}

	private static void SendLaserColor(float r, float g, float b)
	{
		Console.ExecuteCommand("laserColor", (ReceiverGroup)0, r, g, b);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "laserColor", r, g, b }, "laserColor");
	}

	// ====== Run Method (called every frame from UpdateActiveMods) ======
	public static void Run()
	{
		if (NoliStar.Enabled) NoliStar.Run();
		if (BanHammer.Enabled) BanHammer.Run();
		if (RainbowSword.Enabled) RainbowSword.Run();

		if (PhysicsGun.Enabled) PhysicsGun.Run();
		if (Laser.Enabled) Laser.Run();
		if (AdminGrab.Enabled) AdminGrab.Run();
		if (Pistol.Enabled) Pistol.Run();
		if (Coin.Enabled) Coin.Run();
	}

	// ====== Helpers ======
	public static void DestroyAsset(ref int id)
	{
		if (id >= 0)
		{
			Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, id);
			id = -1;
		}
	}

	// ====== NoliStar ======
	public static class NoliStar
	{
		public static bool Enabled;
		private static int id = -1;
		private static int musicId = -1;
		private static float updateDelay;
		private static float respawnTime;
		private static bool holdingTrigger;
		private static Vector3 throwDirection;
		private static Vector3 networkedPos;
		private static Quaternion networkedRot;
		private static int state; // 0=Default, 1=Throwing, 2=Respawning

		public static void Enable()
		{
			if (Enabled) return;
			Enabled = true;
			id = -1;
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			if (musicId >= 0)
			{
				Console.ExecuteCommand("asset-destroy", (ReceiverGroup)1, musicId);
				musicId = -1;
			}
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Run()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Star", delegate(int assetId)
				{
					PlaySound(assetId, "Model", "StarSpawn");
				}));
			}
			if (!Console.ConsoleAssets.TryGetValue(id, out var starAsset) || starAsset.obj == null)
				return;
			GameObject starObj = starAsset.obj;
			ControllerInputPoller poller = (ControllerInputPoller)ControllerInputPoller.instance;
			float noliTrigger = poller.rightControllerIndexFloat;
			if (noliTrigger > 0.5f && state == 0)
			{
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				GameObject noliCrosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				noliCrosshair.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				noliCrosshair.transform.position = (noliRay.point == Vector3.zero) ? (noliRay.transform.position + noliRay.transform.forward * 20f) : noliRay.point;
				noliCrosshair.GetComponent<Renderer>().material.color = Color.white;
				Object.Destroy(noliCrosshair, Time.deltaTime);
				Object.Destroy(noliCrosshair.GetComponent<Collider>());
			}
			if (noliTrigger < 0.5f && holdingTrigger && state == 0)
			{
				state = 1;
				PlayAnimation(id, "Model", "Throw");
				PlaySound(id, "Model", "ThrowStar");
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliDirRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				throwDirection = (noliDirRay.point - starObj.transform.position).normalized;
			}
			holdingTrigger = noliTrigger > 0.5f;
			switch (state)
			{
			case 0:
				starObj.transform.position = GorillaTagger.Instance.rightHandTransform.position + Vector3.up * 0.2f;
				starObj.transform.rotation = Quaternion.Euler(Time.time * 32f, Time.time * 10f, Time.time * 47f);
				break;
			case 1:
			{
				Physics.Raycast(starObj.transform.position, throwDirection, out RaycastHit noliHitRay, 0.5f, GTPlayer.Instance.locomotionEnabledLayers);
				if (noliHitRay.point == Vector3.zero)
				{
					starObj.transform.position += throwDirection * (Time.deltaTime * 15f);
					starObj.transform.rotation = Quaternion.Euler(Time.time * 239f, Time.time * 201f, Time.time * 170f);
				}
				else
				{
					PlayAnimation(id, "Model", "Explode");
					bool noliKill = false;
					foreach (VRRig nRig in VRRigCache.ActiveRigs)
					{
						if (!nRig.isLocal && Vector3.Distance(starObj.transform.position, nRig.transform.position) < 2.32775f)
						{
							Console.ExecuteCommand("silkick", (ReceiverGroup)1, nRig.Creator.UserId);
							noliKill = true;
						}
					}
					PlaySound(id, "Model", noliKill ? "KillStar" : "BreakStar");
					state = 2;
					respawnTime = Time.time + 3f;
				}
				break;
			}
			case 2:
				if (Time.time > respawnTime)
				{
					PlayAnimation(id, "Model", "Default");
					PlaySound(id, "Model", "StarSpawn");
					state = 0;
				}
				break;
			}
			if (Time.time > updateDelay && (networkedRot != starObj.transform.rotation || networkedPos != starObj.transform.position))
			{
				updateDelay = Time.time + 0.05f;
				networkedPos = starObj.transform.position;
				networkedRot = starObj.transform.rotation;
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)0, id, starObj.transform.position);
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)0, id, starObj.transform.rotation);
			}
		}
	}

	// ====== BanHammer ======
	public static class BanHammer
	{
		public static bool Enabled;
		public static int id = -1;
		private static float slashDelayBH;
		private static float pauseSfxBH;
		private static bool lastVelTooHighBH;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "banhammer", "BanHammer", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}, addSurfaceOverride: true));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelayBH = 0f;
			pauseSfxBH = 0f;
			lastVelTooHighBH = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var bhAsset) || bhAsset.obj == null)
				return;
			Transform bhRayPoint = bhAsset.obj.transform.Find("Model/HitBox");
			if (bhRayPoint == null)
				return;
			if (!bhRayPoint.TryGetComponent(out MeshCollider _))
				bhRayPoint.gameObject.AddComponent<MeshCollider>();
			Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhRay, 0.4f, Mods.GetNoInvisLayerMask());
			Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhCRay, 0.4f, Mods.GetNoInvisLayerMask());
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
						((MonoBehaviour)Console.instance).StartCoroutine(KillFX());
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
					((MonoBehaviour)Console.instance).StartCoroutine(HitFX());
				}
			}
			if (bhVelTooHigh && !lastVelTooHighBH && Time.time > pauseSfxBH)
			{
				pauseSfxBH = Time.time + 0.3f;
				PlaySound(id, "Model/SwingSFX", "Swing");
			}
			lastVelTooHighBH = bhVelTooHigh;
		}

		private static IEnumerator KillFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/KillSFX", "HammerKill");
			PlayAnimation(id, "Model", "HitPlayer");
		}

		private static IEnumerator HitFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/SwingSFX", "HammerHit");
			PlayAnimation(id, "Model", "HitGround");
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.transform.position) < 2f)
					Console.ExecuteCommand("vel", rig.Creator.ActorNumber, (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 5f);
			}
		}
	}

	// ====== RainbowSword ======
	public static class RainbowSword
	{
		public static bool Enabled;
		public static int id = -1;
		private static float slashDelayRS;
		private static float pauseSfxRS;
		private static bool lastVelTooHighRS;

		public static void Enable()
		{
			if (id >= 0) return;
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "rbsword", "Sword", delegate(int assetId)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				if (Console.muteRainbowSword && Console.ConsoleAssets.TryGetValue(assetId, out var rsAsset) && rsAsset.obj != null)
				{
					Transform swordTf = rsAsset.obj.transform.Find("Sword");
					if (swordTf != null)
					{
						AudioSource src = ((Component)swordTf).GetComponent<AudioSource>();
						if ((Object)(object)src != (Object)null)
						{
							src.Stop();
							src.volume = 0f;
						}
					}
				}
			}, addSurfaceOverride: true));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelayRS = 0f;
			pauseSfxRS = 0f;
			lastVelTooHighRS = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var rsAsset) || rsAsset.obj == null)
				return;
			Transform rsRayPoint = rsAsset.obj.transform.Find("Sword/HitBox");
			if (rsRayPoint == null)
				return;
			Physics.SphereCast(rsRayPoint.position, 0.1f, rsRayPoint.forward, out RaycastHit rsRay, 0.7f, Mods.GetNoInvisLayerMask());
			if (Time.time > slashDelayRS && rsRay.collider != null)
			{
				try
				{
					VRRig rsTarget = rsRay.collider.GetComponentInParent<VRRig>();
					if (rsTarget != null && !rsTarget.isLocal)
					{
						slashDelayRS = Time.time + 0.5f;
						pauseSfxRS = Time.time + 1f;
						PlaySound(id, "Sword/SFX", "Slash" + Random.Range(1, 3));
						PlayAnimation(id, "Sword", "Particles");
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
				PlaySound(id, "Sword/SFX", "Swing" + Random.Range(1, 3));
			}
			lastVelTooHighRS = rsVelTooHigh;
		}
	}

	// ====== PhysicsGun ======
	public static class PhysicsGun
	{
		public static bool Enabled;
		public static int id = -1;
		private static VRRig targetHoldVRRig;
		private static float rigDistance;
		private static float positionDelay;
		private static bool lastGrip;
		private static float standaloneTriggerDelay;
		private static GameObject crosshair;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "PhysicsGun", delegate(int assetId)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			if (crosshair != null)
			{
				Object.Destroy(crosshair);
				crosshair = null;
			}
			targetHoldVRRig = null;
			rigDistance = 0f;
			positionDelay = 0f;
			lastGrip = false;
			standaloneTriggerDelay = 0f;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var pgAsset) || pgAsset.obj == null)
				return;
			Transform pgRayPoint = pgAsset.obj.transform.Find("raypoint");
			if (pgRayPoint == null)
				return;
			Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgCrosshairRay, 512f, Mods.GetNoInvisLayerMask());
			if (crosshair == null)
			{
				crosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				crosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
				Object.Destroy(crosshair.GetComponent<Collider>());
			}
			if (crosshair != null)
			{
				crosshair.GetComponent<Renderer>().material.color = Color.white;
				crosshair.transform.position = (pgCrosshairRay.point == Vector3.zero) ? (pgRayPoint.position + pgRayPoint.forward * 20f) : pgCrosshairRay.point;
			}
			bool pgGrab = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
			if (pgGrab)
			{
				if (targetHoldVRRig == null)
				{
					Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit, 512f, Mods.GetNoInvisLayerMask());
					VRRig pgNewTarget = pgHit.collider?.GetComponentInParent<VRRig>();
					if (pgNewTarget != null && !pgNewTarget.isLocal)
					{
						targetHoldVRRig = pgNewTarget;
						rigDistance = pgHit.distance;
						PlayAnimation(id, "model", "bright");
						PlaySound(id, "oneshot", "zap");
						PlaySound(id, "constant", "hold");
					}
				}
				else
				{
					Vector2 pgJoy = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimary2DAxis;
					if (Mathf.Abs(pgJoy.y) > 0.2f)
						rigDistance += Time.deltaTime * (pgJoy.y > 0f ? 1f : -1f) * 4f;
					Vector3 pgTargetPos = pgRayPoint.position + pgRayPoint.forward * rigDistance;
					targetHoldVRRig.syncPos = pgTargetPos;
					if (Time.time > positionDelay)
					{
						positionDelay = Time.time + 0.05f;
						Console.ExecuteCommand("tpnv", targetHoldVRRig.Creator.ActorNumber, pgTargetPos);
					}
				}
			}
			if (lastGrip && !pgGrab && targetHoldVRRig != null)
			{
				float pgTrigger = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat;
				if (pgTrigger > 0.5f)
					Console.ExecuteCommand("vel", targetHoldVRRig.Creator.ActorNumber, pgRayPoint.forward * 30f);
				PlayAnimation(id, "model", pgTrigger > 0.5f ? "flash" : "default");
				Console.ExecuteCommand("asset-stopsound", (ReceiverGroup)1, id, "constant");
				PlaySound(id, "oneshot", pgTrigger > 0.5f ? ("launch" + Random.Range(1, 4)) : "drop");
				standaloneTriggerDelay = Time.time + 0.5f;
				targetHoldVRRig = null;
			}
			lastGrip = pgGrab;
			float pgTrigger2 = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat;
			if (pgTrigger2 > 0.5f && !pgGrab && Time.time > standaloneTriggerDelay)
			{
				Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit2, 512f, Mods.GetNoInvisLayerMask());
				VRRig pgTarget2 = pgHit2.collider?.GetComponentInParent<VRRig>();
				if (pgTarget2 != null && !pgTarget2.isLocal)
				{
					standaloneTriggerDelay = Time.time + 0.5f;
					Console.ExecuteCommand("vel", pgTarget2.Creator.ActorNumber, pgRayPoint.forward * 30f);
					PlayAnimation(id, "model", "flash");
					PlaySound(id, "oneshot", "launch" + Random.Range(1, 4));
				}
			}
		}
	}

	// ====== Laser ======
	public static class Laser
	{
		public static bool Enabled;
		private static float laserDelayRight;
		private static float laserDelayLeft;
		private static bool lastLaserLeft;
		private static bool lastLaserRight;

		public static void Enable()
		{
			Enabled = true;
			Console.laserEnabled = true;
			laserDelayRight = 0f;
			laserDelayLeft = 0f;
			lastLaserLeft = false;
			lastLaserRight = false;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.laserEnabled = false;
			SendLaser(false, true, 0f, 0f, 0f);
			SendLaser(false, false, 0f, 0f, 0f);
			lastLaserLeft = false;
			lastLaserRight = false;
		}

		public static void CycleColor()
		{
			laserColorIndex = (laserColorIndex + 1) % laserColors.Length;
			Color color = GetLaserColor();
			SendLaserColor(color.r, color.g, color.b);
			NotifiLib.SendNotification("[<color=red>ADMIN</color>] Laser color: " + laserColorNames[laserColorIndex]);
		}

		public static void Run()
		{
			if (!Console.laserEnabled)
				return;
			bool leftControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimaryButton;
			bool rightControllerPrimaryButton = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
			if (rightControllerPrimaryButton && Time.time > laserDelayRight)
			{
				laserDelayRight = Time.time + 0.1f;
				Color laserColor = GetLaserColor();
				SendLaser(true, true, laserColor.r, laserColor.g, laserColor.b);
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
				SendLaser(true, false, laserColor2.r, laserColor2.g, laserColor2.b);
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

		public static int laserColorIndex;
		public static readonly string[] laserColorNames = new string[6] { "Blue", "Red", "Purple", "Pink", "Yellow", "Gray" };
		public static readonly Color[] laserColors = new Color[6]
		{
			new Color(0f, 0f, 1f),
			new Color(1f, 0f, 0f),
			new Color(0.5f, 0.2f, 0.8f),
			new Color(0.9f, 0.4f, 0.9f),
			new Color(0.9f, 0.7f, 0.1f),
			new Color(0.4f, 0.4f, 0.4f)
		};
		private static Color GetLaserColor()
		{
			return laserColors[laserColorIndex];
		}
	}

	// ====== Pistol ======
	public static class Pistol
	{
		public static bool Enabled;
		public static int id = -1;
		private static float fireDelay;
		private static bool lastTrigger;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Pistol", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			fireDelay = 0f;
			lastTrigger = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
				return;
			bool flag = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat > 0.5f;
			bool flag2 = false;
			if (Console.fullAutoPistol)
			{
				if (flag && Time.time > fireDelay)
				{
					fireDelay = Time.time + 0.0667f;
					flag2 = true;
				}
				if (!flag && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			else
			{
				if (flag && !lastTrigger)
					flag2 = true;
				if (!flag && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			if (flag2)
			{
				PlayAnimation(id, "Model", "Default");
				PlaySound(id, "Model", "PistolShoot");
				PlayAnimation(id, "Model", "Shoot");
				PlayAnimation(id, "Flash", "Shoot");
			}
			lastTrigger = flag;
		}
	}

	// ====== Coin ======
	public static class Coin
	{
		public static bool Enabled;
		public static int id = -1;
		private static bool lastSecondary;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Coin", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			lastSecondary = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
				return;
			bool rightSecondary = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
			if (rightSecondary && !lastSecondary)
			{
				bool heads = Random.value > 0.5f;
				PlayAnimation(id, "CoinHolder", heads ? "Heads" : "Tails");
				PlaySound(id, "CoinHolder", "Flip");
			}
			lastSecondary = rightSecondary;
		}
	}

	// ====== AdminGrab ======
	public static class AdminGrab
	{
		public static bool Enabled;
		private static VRRig grabbedPlayer;

		public static void Enable()
		{
			Enabled = true;
			grabbedPlayer = null;
		}

		public static void Disable()
		{
			Enabled = false;
			grabbedPlayer = null;
		}

		public static void Run()
		{
			if ((Object)(object)ControllerInputPoller.instance == (Object)null)
				return;
			bool rightGrip = ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
			bool leftGrip = ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
			if (rightGrip || leftGrip)
			{
				if (grabbedPlayer == null)
				{
					Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					VRRig nearest = null;
					float minDist = 2f;
					foreach (VRRig rig in VRRigCache.ActiveRigs)
					{
						if (!((Object)(object)rig == (Object)null) && !rig.isLocal)
						{
							float dist = Vector3.Distance(hand.position, ((Component)rig).transform.position);
							if (dist < minDist)
							{
								minDist = dist;
								nearest = rig;
							}
						}
					}
					grabbedPlayer = nearest;
				}
				if (grabbedPlayer != null && grabbedPlayer.Creator != null)
				{
					Transform hand2 = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					Console.ExecuteCommand("tp", (ReceiverGroup)0, grabbedPlayer.Creator.ActorNumber, hand2.position + new Vector3(0f, 0.5f, 0f));
				}
			}
			else
			{
				grabbedPlayer = null;
			}
		}
	}

	// ====== Simple Spawnable Assets ======
	private static void SpawnSimpleAsset(ref int id, string bundle, string asset, Action<int> setup)
	{
		if (id < 0)
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, bundle, asset, setup));
		}
	}

	// ====== Karambit ======
	public static class Karambit
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "karambit", "karambit", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.045f, 0.065f, 0f)); Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(270f, 60f, 0f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== Knife ======
	public static class Knife
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "knife", "knife", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.02866926f, 0.0961746f, 0.1409995f)); Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(79.12813f, 337.5215f, 347.2383f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== RblxCarpet ======
	public static class RblxCarpet
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "rblxcarpet", "robloxrainbowcarpet", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.2574666f, -0.007336602f, 0.1125555f)); Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(1.562481f, 359.7548f, 155.0262f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== McSword ======
	public static class McSword
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnMcSword(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnMcSword()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "mcsword", "Sword", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.03233476f, 0.0433403f, -0.08071579f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(302.1735f, 351.6904f, 280.6184f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, new Vector3(0.01450266f, 0.01450266f, 0.01450266f));
				if (Console.ConsoleAssets.TryGetValue(aid, out var val) && val.obj != null)
				{
					Transform t = val.obj.transform.Find("Music");
					if (t != null) Object.Destroy(t.gameObject);
				}
				Console.ExecuteCommand("asset-setsound", (ReceiverGroup)1, aid, "Music", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
			}));
		}
	}

	// ====== RobloxSword ======
	public static class RobloxSword
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "console.main1", "Sword", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== Bag ======
	public static class Bag
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnBag(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnBag()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "bag", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.1427352f, 0.08271359f, 0.06961101f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(355.0145f, 350.4344f, 162.7124f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, new Vector3(9.717054f, 9.717054f, 9.717054f));
			}));
		}
	}

	// ====== Kormakur ======
	public static class Kormakur
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnKormakur(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnKormakur()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "KormakurSign", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.29f, -0.2f, -0.1272f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(355f, 275f, 265f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one);
			}));
		}
	}

	// ====== Coin (already defined above) ======
	// ====== Boombox ======
	public static class Boombox
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnBoombox(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnBoombox()
		{
			string clipboardUrl = GUIUtility.systemCopyBuffer;
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Boombox", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 1, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0f, 0f, 0.15f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(0f, 90f, 90f));
				if (!string.IsNullOrEmpty(clipboardUrl) && clipboardUrl.StartsWith("http"))
					Console.ExecuteCommand("asset-setsound", (ReceiverGroup)1, aid, "Model", clipboardUrl);
			}));
		}
	}

	// ====== Samsung ======
	public static class Samsung
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "consolehamburburassets", "samsungphone", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 1, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(-0.075f, 0.1f, 0f)); Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(80f, 90f, 180f)); Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one * 0.3f); Console.ExecuteCommand("asset-destroycolliders", (ReceiverGroup)1, aid); string cu = GUIUtility.systemCopyBuffer; if (!string.IsNullOrEmpty(cu) && cu.StartsWith("http")) Console.ExecuteCommand("asset-setvideo", (ReceiverGroup)1, aid, "VideoPlayer", cu); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== VideoPlayer ======
	public static class VideoPlayer
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "console.main1", "VideoPlayer", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 1, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0f, 0f, 0.15f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== TV ======
	public static class TV
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTV(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTV()
		{
			string tvUrl = GUIUtility.systemCopyBuffer;
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "TV", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, aid, new Vector3(-57.1f, 5.6f, -37f));
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, aid, Quaternion.Euler(270f, 0f, 0f));
				if (!string.IsNullOrEmpty(tvUrl) && tvUrl.StartsWith("http"))
					Console.ExecuteCommand("asset-setvideo", (ReceiverGroup)1, aid, "VideoPlayer", tvUrl);
			}));
		}
	}

	// ====== Shreksophone ======
	public static class Shreksophone
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnShreksophone(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnShreksophone()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "shrek", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, aid, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, aid, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one * 5f);
			}));
		}
	}

	// ====== Carti ======
	public static class Carti
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnCarti(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnCarti()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "carti", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, aid, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", (ReceiverGroup)1, aid, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one * 5f);
			}));
		}
	}

	// ====== Travis ======
	public static class Travis
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravis(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravis()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", (ReceiverGroup)1, aid, new Vector3(-70f, 2f, -52f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one * 0.38f);
			}));
		}
	}

	// ====== TravisBeach ======
	public static class TravisBeach
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisBeach(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisBeach()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(16.38702f, 12.29928f, 23.63119f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(352.4303f, 49.92272f, 0.8915782f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, new Vector3(0.38f, 0.38f, 0.38f));
			}));
		}
	}

	// ====== TravisCritters ======
	public static class TravisCritters
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisCritters(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisCritters()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(229.5867f, -98.26467f, 178.8833f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(4.141929f, 52.20211f, 2.67847f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, new Vector3(1.784783f, 1.784783f, 1.784783f));
			}));
		}
	}

	// ====== TravisCity ======
	public static class TravisCity
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisCity(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisCity()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(-52.68209f, 16.36728f, -118.7615f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(0.9019919f, 345.8464f, 1.200598f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, new Vector3(0.02183428f, 0.02183428f, 0.02183428f));
			}));
		}
	}

	// ====== Kormakur (already defined above) ======

	// ====== AllowKickSelf ======
	public static class AllowKickSelf
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.allowKickSelf = true; }
		public static void Disable() { Enabled = false; Console.allowKickSelf = false; }
	}

	// ====== AllowTpSelf ======
	public static class AllowTpSelf
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.allowTpSelf = true; }
		public static void Disable() { Enabled = false; Console.allowTpSelf = false; }
	}

	// ====== DetectConsoleUsers ======
	public static class DetectConsoleUsers
	{
		public static bool Enabled;
		public static void Enable()
		{
			Enabled = true;
			Console.autoDetectConsoleUsers = true;
			Console.indicatorDelay = 5f;
			Console.ExecuteCommand("isusing", (ReceiverGroup)1);
		}
		public static void Disable()
		{
			Enabled = false;
			Console.autoDetectConsoleUsers = false;
			Console.ClearConsoleUserIndicators();
			Console.userDictionary.Clear();
		}
	}

	// ====== NoAdminIndicator ======
	public static class NoAdminIndicator
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.ExecuteCommand("nocone", (ReceiverGroup)1, false); }
		public static void Disable() { Enabled = false; Console.ExecuteCommand("nocone", (ReceiverGroup)1, true); }
	}

	// ====== FullAutoPistol ======
	public static class FullAutoPistol
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.fullAutoPistol = true; }
		public static void Disable() { Enabled = false; Console.fullAutoPistol = false; }
	}

	// ====== MuteRainbowSword ======
	public static class MuteRainbowSword
	{
		public static bool Enabled;
		public static void Enable()
		{
			if (Enabled) return;
			Enabled = true;
			Console.muteRainbowSword = true;
			PlayerPrefs.SetInt("ChudMuteRainbowSword", 1);
			if (RainbowSword.id >= 0)
			{
				SetVolume(RainbowSword.id, "Sword", 0f);
			}
		}
		public static void Disable()
		{
			if (!Enabled) return;
			Enabled = false;
			Console.muteRainbowSword = false;
			PlayerPrefs.SetInt("ChudMuteRainbowSword", 0);
			if (RainbowSword.id >= 0)
			{
				SetVolume(RainbowSword.id, "Sword", 1f);
			}
		}
		private static void SetVolume(int id, string path, float volume)
		{
			Console.ExecuteCommand("asset-setvolume", (ReceiverGroup)0, id, path, volume);
			Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-setvolume", id, path, volume }, "asset-setvolume");
		}
	}

	// ====== KickAll ======
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

	// ====== MinosPrime ======
	public static class MinosPrime
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable()
		{
			id = Console.GetFreeAssetID();
			((MonoBehaviour)Console.instance).StartCoroutine(Console.SpawnAndSetupAsset(id, "minosprime", "minosprime", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", (ReceiverGroup)1, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", (ReceiverGroup)1, aid, new Vector3(0.06263994f, 0.05301395f, -0.04137805f));
				Console.ExecuteCommand("asset-setlocalrotation", (ReceiverGroup)1, aid, Quaternion.Euler(286.3085f, 201.7456f, 347.1011f));
				Console.ExecuteCommand("asset-setscale", (ReceiverGroup)1, aid, Vector3.one * 0.3518889f);
			}));
			Enabled = true;
		}
		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	// ====== DestroyAllAssets ======
	public static void DestroyAllAssets()
	{
		foreach (KeyValuePair<int, Console.ConsoleAsset> kvp in Console.ConsoleAssets)
		{
			kvp.Value.DestroyObject();
		}
		Console.ConsoleAssets.Clear();
	}
}
