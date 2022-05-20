using System;
using KKAPI.MainGame;
using UnityEngine;

namespace KK_HForAll
{
    public class HForAllGameController : GameCustomFunctionController
    {
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            HSceneProc hSceneProc = hFlag.gameObject.GetComponent<HSceneProc>();
            //Adds the trespass button, no extra work needed. Starts the mode just fine without additional work.
            Console.WriteLine("OnStartH triggered from Free3p");
            if (hFlag.mode == HFlag.EMode.lesbian)
            {
                hFlag.gameObject.GetComponent<HSceneProc>().sprite.btnTrespassing.gameObject.SetActive(true);
            }

            if (hFlag.mode == HFlag.EMode.houshi3P || hFlag.mode == HFlag.EMode.sonyu3P)
            {
                hSceneProc.sprite.menuAction.lstButton[1].gameObject.SetActive(true);
                hSceneProc.sprite.menuAction.lstButton[2].gameObject.SetActive(true);
            }
        }

        
    }
}
