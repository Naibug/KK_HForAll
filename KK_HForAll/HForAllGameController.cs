using System;
using KKAPI.MainGame;
using UnityEngine;

namespace KK_HForAll
{
    public class HForAllGameController : GameCustomFunctionController
    {
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            //Adds the trespass button, no extra work needed. Starts the mode just fine without additional work.
            Console.WriteLine("OnStartH triggered from Free3p");
            if (hFlag.mode == HFlag.EMode.lesbian)
            {
                hFlag.gameObject.GetComponent<HSceneProc>().sprite.btnTrespassing.gameObject.SetActive(true);
            }
        }

        
    }
}
