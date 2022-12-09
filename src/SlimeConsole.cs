using DevConsole.Commands;
using static DevConsole.GameConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile
{
    internal static class SlimeConsole
    {

        public static bool infBoom = false;
        public static bool LW = false;
        public static int forcecat = -1;


        public static void Apply()
        {
            try {
                AddCommands();
            } catch (Exception ex) { Debug.Log("failed to introduce commands due to: \n" + ex.ToString()); }
        }

        private static void AddCommands()
        {
            new CommandBuilder("skin").Run((args) => {
                if(UnityEngine.Object.FindObjectOfType<RainWorld>()?.processManager?.currentMainLoop is RainWorldGame) {
                    WriteLine("you cant force a skin during gameplay!");
                    return;
                }

                switch (args[0]) {
                    case "slime":
                    case "Slime":
                    case "1":
                        forcecat = 0;
                        WriteLine("forcing volatile skin");
                        break;
                    case "gup":
                    case "Gup":
                    case "2":
                        forcecat = 1;
                        WriteLine("forcing gup skin");
                        break;
                    case "king":
                    case "King":
                    case "3":
                        forcecat = 2;
                        WriteLine("forcing king slime skin");
                        break;
                    case "tabby":
                    case "Tabby":
                    case "4":
                        forcecat = 3;
                        WriteLine("forcing tabby slime skin");
                        break;
                    case "none":
                    case "None":
                    case "0":
                        forcecat = -1;
                        WriteLine("no forced skins");
                        break;
                    default:
                        WriteLine("what do you MEEEAN \n  - use numbers 1-4 or slime/gup/king/tabby to set a skin\n  - use 0 or none to disable skin forcing");
                        break;
                }
            }).Register();

            new CommandBuilder("LMstart").Run((args) => {
                LW = !LW;
                WriteLine("Start in LM: " + LW);
            }).Register();

            new CommandBuilder("infLighter").Run((args) => {
                infBoom = !infBoom;
                WriteLine("infinite explosions: " + infBoom);
            }).Register();
        }
    }
}
