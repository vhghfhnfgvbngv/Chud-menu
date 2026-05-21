using Photon.Realtime;

namespace MalachiTemp.Utilities
{
    internal static class RigHelpers
    {
        public static VRRig GetVRRigFromPlayer(Player p)
        {
            return GorillaGameManager.StaticFindRigForPlayer(p);
        }
    }
}
