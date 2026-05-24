using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace MalachiTemp.Backend
{
    [HarmonyPatch(typeof(Slingshot), "GetLaunchVelocity")]
    public static class GetLaunchPatch
    {
        public static bool enabled;
        public static void Postfix(Slingshot __instance, ref Vector3 __result)
        {
            if (!enabled) return;
            VRRig target = null;
            float bestScore = float.MaxValue;
            foreach (var rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isLocal || rig.Creator == null) continue;
                if (rig.Creator.UserId == PhotonNetwork.LocalPlayer.UserId) continue;
                Vector3 dir = rig.headMesh.transform.position - __instance.centerOrigin.position;
                float dist = dir.magnitude;
                float angle = Vector3.Angle(GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward, dir);
                float score = angle + dist * 0.1f;
                if (score < bestScore) { bestScore = score; target = rig; }
            }
            if (target == null) return;
            Vector3 dirToTarget = (target.headMesh.transform.position - __instance.centerOrigin.position).normalized;
            float speed = __result.magnitude;
            if (speed < 1f) speed = 15f;
            __result = dirToTarget * speed;
        }
    }
}
