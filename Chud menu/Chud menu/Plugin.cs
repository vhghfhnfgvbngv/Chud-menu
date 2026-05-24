using BepInEx;
using Loading;
using MalachiTemp.Backend;
using MalachiTemp.UI;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Malachis_Temp
{
    [BepInPlugin(Name, GUID, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "malachistemp";
        public const string GUID = "malachis.temp";
        public const string Version = "1.0";

        private bool patchedHarmony = false;

        void Awake()
        {
            if (!patchedHarmony && Loader.loaded == false)
            {
                Harmony harmony = new Harmony(GUID);
                harmony.PatchAll();
                patchedHarmony = true;
                Loader.loaded = true;
            }
        }
    }

    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "FixedUpdate")]
    internal class InitPatch
    {
        private static bool alreadyInit;

        static void Postfix()
        {
            if (alreadyInit) return;
            alreadyInit = true;

            GameObject go = new GameObject();
            go.AddComponent<Plugin>();
            go.AddComponent<WristMenu>();
            go.AddComponent<ModGUI>();
            go.AddComponent<Mods>();
            go.AddComponent<GTAG_NotificationLib.NotifiLib>();
            go.AddComponent<CustomPropSetter>();
            go.AddComponent<ConsoleIntegration>();
            try { Mods.AutoLoad(); } catch { }
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Object.DontDestroyOnLoad(go);
        }
    }
}
