using System;
using BepInEx;
using HarmonyLib;
using KKAPI;
using KKAPI.MainGame;
using KKAPI.Studio;
using UnityEngine;

namespace KK_HForAll
{
    [BepInPlugin(GUID, "Koikatsu HForAll", Version)]
    [BepInProcess(GameProcessName)]
    [BepInProcess(GameProcessNameSteam)]
    [BepInProcess(VRProcessName)]
    [BepInProcess(VRProcessNameSteam)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class KK_HForAll : BaseUnityPlugin
    {
        public const string GUID = "HForAll-Gameplaymod";
        public const string Version = "0.0.1";
        private const string GameProcessName = "Koikatu";
        private const string GameProcessNameSteam = "Koikatsu Party";
        private const string VRProcessName = "KoikatuVR";
        private const string VRProcessNameSteam = "Koikatsu Party VR";

        public static bool IsInsideVR { get; } = Application.productName == VRProcessName || Application.productName == VRProcessNameSteam;

        private void Start()
        {
            GameAPI.RegisterExtraBehaviour<HForAllGameController>(GUID);
            var hi = new Harmony(GUID);
            if (StudioAPI.InsideStudio)
            {
                return;
            }
            Hooks.InitHooks(hi);
        }
    }
}
