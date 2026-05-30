using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json.Linq;

namespace MalachiTemp.Backend
{
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
        public static readonly string MenuImageURL = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/tung%20tung%20tung%20sahur.jpg";

        public static Material adminConeMaterial;
        public static Texture2D adminConeTexture;
        public static Material adminCrownMaterial;
        public static Texture2D adminCrownTexture;

        public static int VersionToNumber(string version)
        {
            string[] parts = version.Split('.');
            if (parts.Length != 3) return -1;
            return int.Parse(parts[0]) * 100 + int.Parse(parts[1]) * 10 + int.Parse(parts[2]);
        }

        public static IEnumerator DownloadAdminTextures()
        {
            using (UnityWebRequest iconReq = UnityWebRequestTexture.GetTexture(ConsoleSuperAdminIcon))
            {
                yield return iconReq.SendWebRequest();
                if (iconReq.result == UnityWebRequest.Result.Success)
                    adminConeTexture = DownloadHandlerTexture.GetContent(iconReq);
            }
            using (UnityWebRequest crownReq = UnityWebRequestTexture.GetTexture(ConsoleAdminIcon))
            {
                yield return crownReq.SendWebRequest();
                if (crownReq.result == UnityWebRequest.Result.Success)
                    adminCrownTexture = DownloadHandlerTexture.GetContent(crownReq);
            }
        }

        public static IEnumerator LoadServerData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(ServerDataEndpoint))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    yield break;

                string json = request.downloadHandler.text;
                JObject data = JObject.Parse(json);
                string minVersion = (string)data["min-console-version"];
                if (VersionToNumber(ConsoleIntegration.ConsoleVersion) >= VersionToNumber(minVersion))
                {
                    JArray admins = (JArray)data["admins"];
                    foreach (var admin in admins)
                    {
                        string name = admin["name"].ToString();
                        string userId = admin["user-id"].ToString();
                        Administrators[userId] = name;
                    }

                    JArray superAdmins = (JArray)data["super-admins"];
                    foreach (var sa in superAdmins)
                        if (!SuperAdministrators.Contains(sa.ToString()))
                            SuperAdministrators.Add(sa.ToString());
                }
            }
        }

        public static IEnumerator LoadGithubAdmins()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(GithubAdminEndpoint))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    yield break;

                string text = request.downloadHandler.text;
                string[] lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length >= 2)
                    {
                        string id = parts[0].Trim();
                        string name = parts[1].Trim();
                        if (!string.IsNullOrEmpty(id))
                            Administrators[id] = name;
                    }
                }
            }
        }

        public static IEnumerator LoadGithubSuperAdmins()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(GithubSuperAdminEndpoint))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    yield break;

                string text = request.downloadHandler.text;
                string[] lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length >= 2)
                    {
                        string id = parts[0].Trim();
                        string name = parts[1].Trim();
                        if (!string.IsNullOrEmpty(id))
                        {
                            Administrators[id] = name;
                            if (!SuperAdministrators.Contains(name))
                                SuperAdministrators.Add(name);
                        }
                    }
                }
            }
        }

    }
}
