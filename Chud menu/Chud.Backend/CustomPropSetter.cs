using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

internal class CustomPropSetter : MonoBehaviour
{
	private bool hasSet = false;

	private void Update()
	{
		if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.IsConnectedAndReady)
		{
			if (!hasSet)
			{
				SetProp();
				hasSet = true;
			}
		}
		else
		{
			hasSet = false;
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
