using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

internal class CustomPropSetter : MonoBehaviour
{
	private static bool hasSetSession = false;

	private void Update()
	{
		if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.IsConnectedAndReady)
		{
			if (!hasSetSession)
			{
				SetProp();
				hasSetSession = true;
			}
		}
	}

	public static void SetProp()
	{
		Hashtable val = new Hashtable();
		val[(object)"Chud menu"] = true;
		if (PhotonNetwork.LocalPlayer != null)
		{
			PhotonNetwork.LocalPlayer.SetCustomProperties(val, (Hashtable)null, (WebFlags)null);
		}
	}
}
