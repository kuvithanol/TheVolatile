using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using SlugBase;

namespace TheVolatile
{
    [BepInPlugin("sov.sam.volatile", nameof(TheVolatile), "0.1")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public void OnEnable()
        {
            On.RainWorld.Start += RainWorld_Start;
            FisobRegistry fisobs = new FisobRegistry(new Fisob[] { FisLighter.Instance });
            fisobs.ApplyHooks();
        }

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            logger = this.Logger;
            orig(self);
            

            PlayerManager.RegisterCharacter(new SlugbaseVolatile());
        }
    }
}
