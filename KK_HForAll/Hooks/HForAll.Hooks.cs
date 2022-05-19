﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using KKAPI.MainGame;
using Manager;
using UnityEngine;


namespace KK_HForAll
{
    public partial class KK_HForAll
    {
        private static class Hooks
        {
            private static HFlag.EMode backmode;
            private static int backHoushi;
            private static int backTaii;

            internal static void InitHooks(Harmony hi)
            {
                hi.PatchAll(typeof(Hooks));
            }

            //Old code that was taken from StrawberryDarkness mod. Used it to flesh out some of the ideas I had and road blocks. Not sure how neccessary it was? Big stopper was animation getting stuck in drain.
            //Moved general idea into the VoiceProc hook instead; it calls these so I think setting the mode to 2p should accomplish the same thing?
           // [HarmonyPrefix]
           // [HarmonyPatch(typeof(HVoiceCtrl), "IsPtnConditionsHoushi", new Type[]
           //{
           //     typeof(List<int>),
           //     typeof(ChaControl),
           //     typeof(int)
           //})]
           // private static void IsPtnConditionsHoushiPre(HVoiceCtrl __instance, ref bool __result, List<int> _lstConditions, ChaControl _female, int _main)
           // {
           //     if (__instance.flags.isFreeH)
           //         return;
           //     if (_female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.淫乱 || _female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.慣れ)
           //     {
           //         return;
           //     }

           //     backmode = __instance.flags.mode;

           //     if (__instance.flags.mode == HFlag.EMode.houshi3P || __instance.flags.mode == HFlag.EMode.sonyu3P)
           //     {
           //         Console.WriteLine("HoushiPre detected 3p and fired");
           //         __instance.flags.mode = HFlag.EMode.houshi;
           //     }
           // }

           // [HarmonyPostfix]
           // [HarmonyPatch(typeof(HVoiceCtrl), "IsPtnConditionsHoushi", new Type[]
           // {
           //     typeof(List<int>),
           //     typeof(ChaControl),
           //     typeof(int)
           // })]
           // private static void IsPtnConditionsHoushiPost(HVoiceCtrl __instance, ref bool __result, List<int> _lstConditions, ChaControl _female, int _main)
           // {
           //     if (__instance.flags.isFreeH)
           //         return;
           //     if (_female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.淫乱 || _female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.慣れ)
           //         return;

           //     __instance.flags.mode = backmode;
           // }

           // [HarmonyPrefix]
           // [HarmonyPatch(typeof(HVoiceCtrl), "IsPtnConditionsSonyu", new Type[]
           // {
           //     typeof(List<int>),
           //     typeof(ChaControl),
           //     typeof(int)
           // })]
           // private static void IsPtnConditionsSonyuPre(HVoiceCtrl __instance, ref bool __result, List<int> _lstConditions, ChaControl _female, int _main)
           // {
           //     if (__instance.flags.isFreeH)
           //         return;
           //     if (_female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.淫乱 || _female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.慣れ)
           //     {
           //         return;
           //     }

           //     backmode = __instance.flags.mode;

           //     if (__instance.flags.mode == HFlag.EMode.houshi3P || __instance.flags.mode == HFlag.EMode.sonyu3P)
           //     {
           //         __instance.flags.mode = HFlag.EMode.sonyu;
           //     }
           // }
           // [HarmonyPostfix]
           // [HarmonyPatch(typeof(HVoiceCtrl), "IsPtnConditionsSonyu", new Type[]
           // {
           //     typeof(List<int>),
           //     typeof(ChaControl),
           //     typeof(int)
           // })]
           // private static void IsPtnConditionsSonyuPost(HVoiceCtrl __instance, ref bool __result, List<int> _lstConditions, ChaControl _female, int _main)
           // {
           //     if (__instance.flags.isFreeH)
           //         return;
           //     if (_female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.淫乱 || _female.GetHeroine().HExperience == SaveData.Heroine.HExperienceKind.慣れ)
           //         return;

           //     __instance.flags.mode = backmode;
           // }

            //Also from strawberry darkness, pretty sure this is needed to get breathing sounds to play right with less than experienced girls.
            //Maybe add experience check? Think its different than others but for now leaving his code as is until I understand more of this better
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HVoiceCtrl), "BreathProc", new Type[]
            {
                typeof(AnimatorStateInfo),
                typeof(ChaControl),
                typeof(int)
            })]
            private static void BreathProcPre(HVoiceCtrl __instance, ref bool __result, AnimatorStateInfo _ai, ChaControl _female, int _main)
            {
                if (__instance.flags.isFreeH)
                    return;

                backmode = __instance.flags.mode;
                //backHoushi = __instance.flags.nowAnimationInfo.kindHoushi;
                //backTaii = __instance.flags.nowAnimationInfo.sysTaii;
                if (__instance.flags.mode == HFlag.EMode.houshi3P)
                {
                    __instance.flags.mode = HFlag.EMode.houshi;
                    return;
                }
                if (__instance.flags.mode == HFlag.EMode.sonyu3P)
                {
                    __instance.flags.mode = HFlag.EMode.sonyu;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HVoiceCtrl), "BreathProc", new Type[]
            {
                typeof(AnimatorStateInfo),
                typeof(ChaControl),
                typeof(int)
            })]
            private static void BreathProcPost(HVoiceCtrl __instance, ref bool __result, AnimatorStateInfo _ai, ChaControl _female, int _main)
            {
                if (__instance.flags.isFreeH)
                {
                    return;
                }

                __instance.flags.mode = backmode;
                //__instance.flags.nowAnimationInfo.kindHoushi = backHoushi;
                //__instance.flags.nowAnimationInfo.sysTaii = backTaii;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(H3PSonyu), nameof(H3PSonyu.Proc))]
            public static void CheckForDropAnimation(HActionBase __instance)
            {
                //Fix for stuck "Drop" animation. Difference in how 3p vs 2p works. Borrow methods from 2p; check to see if either of them are talking in 3p.
                //If not, manually set the now voice to breath for both. Should trick the 3p proc code into unlocking the options and allowing player to select more positions, etc.
                if ((__instance.female.getAnimatorStateInfo(0).IsName("Drop")) && (!Singleton<Voice>.Instance.IsVoiceCheck(__instance.flags.transVoiceMouth[0]) || !Singleton<Voice>.Instance.IsVoiceCheck(__instance.flags.transVoiceMouth[1])))
                {
                    __instance.voice.nowVoices[0].state = HVoiceCtrl.VoiceKind.breath;
                    __instance.voice.nowVoices[1].state = HVoiceCtrl.VoiceKind.breath;
                }
                //Also a bug with OLoop, think its similar. Compare differences in HSonyu vs H3PSonyu classes to figure out what locks it in that animation.
                //Changing VoiceKind to breath like above broke the lock, just have to figure out what conditions to check. Triggered by clicking girl during animation.
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HVoiceCtrl), "VoiceProc", new Type[]
            {
            typeof(AnimatorStateInfo),
            typeof(ChaControl),
            typeof(int)
            })]
            private static void VoiceProcPre(HVoiceCtrl __instance, AnimatorStateInfo _ai, ChaControl _female, int _main)
            {
                backmode = __instance.flags.mode;
                backHoushi = __instance.flags.nowAnimationInfo.kindHoushi;
                backTaii = __instance.flags.nowAnimationInfo.sysTaii;
                if (__instance.flags.voice.playVoices[_main] == -1)
                    return;

                int num = __instance.flags.voice.playVoices[_main] / 100;
                int num2 = __instance.flags.voice.playVoices[_main] % 100;

                //Debugging crap that should be set up better, fix later so its not amateur hour
                Console.WriteLine("playing: " + __instance.flags.voice.playVoices[_main]);
                Console.WriteLine("num is: " + num);
                Console.WriteLine("num2 is: " + num2);
                Console.WriteLine("kindHoushi is: " + __instance.flags.nowAnimationInfo.kindHoushi);
                Console.WriteLine("sysTaii is: " + __instance.flags.nowAnimationInfo.sysTaii);

                if (__instance.flags.isFreeH)
                    return;

                
                if (__instance.flags.lstHeroine[_main].HExperience != SaveData.Heroine.HExperienceKind.淫乱 || __instance.flags.lstHeroine[_main].HExperience != SaveData.Heroine.HExperienceKind.慣れ)
                {
                    if (__instance.flags.mode == HFlag.EMode.houshi3P)
                    {
                        __instance.flags.mode = HFlag.EMode.houshi;
                    }
                    else if (__instance.flags.mode == HFlag.EMode.sonyu3P)
                    {
                        __instance.flags.mode = HFlag.EMode.sonyu;
                    }

                    //Transition voice lines? Not sure what to call them. Usually play when shifting positions or starting, but not consistent across HModes.
                    if (num == 0)
                    {
                        if (num2 == 12)
                            __instance.flags.voice.playVoices[_main] = 0;
                        if (num2 == 13)
                            __instance.flags.voice.playVoices[_main] = 1;
                        if (num2 == 14)
                            __instance.flags.voice.playVoices[_main] = 2;
                        if (num2 == 15 || num2 == 17 || num2 == 22) //lumping 17 in here for now, no idea where it goes 
                            __instance.flags.voice.playVoices[_main] = 4;
                        if (num2 == 16 || num2 == 23)
                            __instance.flags.voice.playVoices[_main] = 5;
                        if (num2 == 18 || num2 == 20)
                            __instance.flags.voice.playVoices[_main] = 8;
                        if (num2 == 19)
                            __instance.flags.voice.playVoices[_main] = 7;
                        if (num2 == 21)
                            __instance.flags.voice.playVoices[_main] = 10;
                    }
                    //Service mode, not so smooth. Current issue with inside finish. 3p plays audio files for both, 2p does not so there's no great mapping right now.
                    //I think its possible to extend the time it plays so it isn't so abrupt.
                    //Also licking lacks tongues, probably not hard to fix but a standing issue.
                    if (num == 7)
                    {
                        __instance.flags.nowAnimationInfo.sysTaii = 2;
                        __instance.flags.nowAnimationInfo.kindHoushi = 1;
                        if (num2 == 0)
                            __instance.flags.voice.playVoices[_main] = 200;
                        if (num2 == 1 || num2 == 2)
                            __instance.flags.voice.playVoices[_main] = 201;
                        if (num2 == 3 || num2 == 4)
                            __instance.flags.voice.playVoices[_main] = 202;
                        if (num2 == 5)
                            __instance.flags.voice.playVoices[_main] = 203;
                        if (num2 == 6)
                            __instance.flags.voice.playVoices[_main] = 204;
                        if (num2 == 7)
                            __instance.flags.voice.playVoices[_main] = 205;
                        if (num2 == 8)
                            __instance.flags.voice.playVoices[_main] = 206;
                        if (num2 == 13)
                            __instance.flags.voice.playVoices[_main] = 206;
                        if (num2 == 9)
                            __instance.flags.voice.playVoices[_main] = 207;
                        if (num2 == 10)
                            __instance.flags.voice.playVoices[_main] = 208;
                        if (num2 == 11)
                            __instance.flags.voice.playVoices[_main] = 209;
                    }
                    //Piston modes, translation is super easy. Maps basically 1:1 with 2p voice lines. 
                    //Different range for cowgirl positions, controlled by sysTaii. I believe -1 is cowgirl, 1 is behind and 0 is front. Doesn't control access to doggy I think?
                    //If there's a mismatch between what the voice thinks these should be, the voice just won't play.
                    //Cowgirl isn't consistent as of now, I thought I had it working but its silent again. Think systaii is unrelated? Frustrating as nothing I roll back resolves the issue
                    if (num == 8)
                    {
                        int target = __instance.flags.voice.playVoices[_main] - 500;
                        //if its a cowgirl position, reduce it to the missionary range of voices. workaround until i figure out how i broke it
                        if (num2 >= 39)
                            target -= 38;
                        
                        __instance.flags.voice.playVoices[_main] = target;
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HVoiceCtrl), "VoiceProc", new Type[]
            {
            typeof(AnimatorStateInfo),
            typeof(ChaControl),
            typeof(int)
            })]
            private static void VoiceProcPost(HVoiceCtrl __instance, AnimatorStateInfo _ai, ChaControl _female, int _main)
            {
                if (__instance.flags.isFreeH)
                {
                    return;
                }

                __instance.flags.mode = backmode;
                __instance.flags.nowAnimationInfo.kindHoushi = backHoushi;
                __instance.flags.nowAnimationInfo.sysTaii = backTaii;
            }
        }
    }
}
