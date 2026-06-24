using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json.Linq;

namespace Chud.Backend;

public static class ServerData
{
	public static readonly Dictionary<string, string> Administrators = new Dictionary<string, string>();

	public static readonly List<string> SuperAdministrators = new List<string>();

	public static readonly string ServerDataEndpoint = "https://menu.seralyth.software/serverdata";

	public static readonly string GithubAdminEndpoint = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/adminids.txt";

	public static readonly string GithubSuperAdminEndpoint = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/SuperAdmins.txt";

	public static readonly string ConsoleAssetsURL = "https://raw.githubusercontent.com/Seralyth/Console/refs/heads/master/ServerData";

	public static readonly string ConsoleSuperAdminIcon = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/Chud%20Super%20Admin.png";

	public static readonly string ConsoleAdminIcon = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/Super%20admin%20Flower%20Crown.png";

	public static readonly string BlocklistEndpoint = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/blockedids.txt";

	public static readonly HashSet<string> BlockedIDs = new HashSet<string>();

	public static readonly string MenuImageURL = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/tung%20tung%20tung%20sahur.jpg";

	public static Material adminConeMaterial;

	public static Texture2D adminConeTexture;

	public static Material adminCrownMaterial;

	public static Texture2D adminCrownTexture;

	public static int VersionToNumber(string version)
	{
		string[] array = version.Split('.');
		if (array.Length != 3)
		{
			return -1;
		}
		if (int.TryParse(array[0], out var major) && int.TryParse(array[1], out var minor) && int.TryParse(array[2], out var patch))
		{
			return major * 100 + minor * 10 + patch;
		}
		return -1;
	}

	public static IEnumerator DownloadAdminTextures()
	{
		UnityWebRequest iconReq = UnityWebRequestTexture.GetTexture(ConsoleSuperAdminIcon);
		try
		{
			yield return iconReq.SendWebRequest();
			if ((int)iconReq.result == 1)
			{
				adminConeTexture = DownloadHandlerTexture.GetContent(iconReq);
			}
		}
		finally
		{
			((IDisposable)iconReq)?.Dispose();
		}
		UnityWebRequest crownReq = UnityWebRequestTexture.GetTexture(ConsoleAdminIcon);
		try
		{
			yield return crownReq.SendWebRequest();
			if ((int)crownReq.result == 1)
			{
				adminCrownTexture = DownloadHandlerTexture.GetContent(crownReq);
			}
		}
		finally
		{
			((IDisposable)crownReq)?.Dispose();
		}
	}

	public static IEnumerator LoadServerData()
	{
		UnityWebRequest request = UnityWebRequest.Get(ServerDataEndpoint);
		try
		{
			yield return request.SendWebRequest();
			if ((int)request.result == 2 || (int)request.result == 3)
			{
				yield break;
			}
			string json = request.downloadHandler.text;
			JObject data = JObject.Parse(json);
			string minVersion = (string)data["min-console-version"];
			if (VersionToNumber("3.0.8") < VersionToNumber(minVersion))
			{
				yield break;
			}
			JArray admins = (JArray)data["admins"];
			foreach (JToken admin in admins)
			{
				string name = ((object)admin[(object)"name"]).ToString();
				string userId = ((object)admin[(object)"user-id"]).ToString();
				Administrators[userId] = name;
			}
			JArray superAdmins = (JArray)data["super-admins"];
			foreach (JToken sa in superAdmins)
			{
				if (!SuperAdministrators.Contains(((object)sa).ToString()))
				{
					SuperAdministrators.Add(((object)sa).ToString());
				}
			}
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	public static IEnumerator LoadGithubAdmins()
	{
		UnityWebRequest request = UnityWebRequest.Get(GithubAdminEndpoint);
		try
		{
			yield return request.SendWebRequest();
			if ((int)request.result == 2 || (int)request.result == 3)
			{
				yield break;
			}
			string text = request.downloadHandler.text;
			string[] lines = text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array = lines;
			foreach (string line in array)
			{
				string[] parts = line.Split(':');
				if (parts.Length >= 2)
				{
					string id = parts[0].Trim();
					string name = parts[1].Trim();
					if (!string.IsNullOrEmpty(id))
					{
						Administrators[id] = name;
					}
				}
			}
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	public static IEnumerator LoadGithubSuperAdmins()
	{
		UnityWebRequest request = UnityWebRequest.Get(GithubSuperAdminEndpoint);
		try
		{
			yield return request.SendWebRequest();
			if ((int)request.result == 2 || (int)request.result == 3)
			{
				yield break;
			}
			string text = request.downloadHandler.text;
			string[] lines = text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array = lines;
			foreach (string line in array)
			{
				string[] parts = line.Split(':');
				if (parts.Length < 2)
				{
					continue;
				}
				string id = parts[0].Trim();
				string name = parts[1].Trim();
				if (!string.IsNullOrEmpty(id))
				{
					Administrators[id] = name;
					if (!SuperAdministrators.Contains(name))
					{
						SuperAdministrators.Add(name);
					}
				}
			}
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	public static IEnumerator LoadBlockedIDs()
	{
		UnityWebRequest request = UnityWebRequest.Get(BlocklistEndpoint);
		try
		{
			yield return request.SendWebRequest();
			if ((int)request.result == 1)
			{
				string text = request.downloadHandler.text;
				string[] lines = text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string line in lines)
				{
					string id = line.Trim();
					if (!string.IsNullOrEmpty(id))
					{
						BlockedIDs.Add(id);
					}
				}
			}
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}
}
