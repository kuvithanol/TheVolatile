using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using SlugBase;
using UnityEngine;
using Fisobs.Items;
using Fisobs.Core;
using OptionalUI;

namespace TheVolatile
{
    [BepInPlugin("sov.sam.volatile", nameof(TheVolatile), "2.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public static System.Random r = new System.Random();
        public static ManualLogSource logger;
        public OptionInterface LoadOI() => new SlimeOI(this);
        public static Plugin instance;


        public void OnEnable()
        {
            On.RainWorld.Start += RainWorld_Start;
            On.Room.ctor += Room_ctor;
            Content.Register(new Fisob[] { FisLighter.Instance });
            instance = this;

            SlimeConsole.Apply();
            //PlacedObs.PlacedObs.Apply();

            
        }

        public void OnDisable()
        {
            logger = default;
            SlugbaseVolatile.instance = default;
            Lighter.allLighters = default;
            r = default;
        }

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            logger = this.Logger;
            orig(self);
            

            PlayerManager.RegisterCharacter(new SlugbaseVolatile());
        }

        private void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig(self, game, world, abstractRoom);
            if (game != null && self.abstractRoom.name == "ROOOOOOOOOOOM") {
                var pObj = new PlacedObject(PlacedObject.Type.None, null);
                pObj.FromString(new string[] { "MountainShrine","2000.05","367.0028","0~0~2~3" });
                self.roomSettings.placedObjects.Add(pObj);
                Debug.Log("this is me when i ammend the constitution");
            }
        }
    }
}
