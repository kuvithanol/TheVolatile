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


        public static void Apply()
        {
            try {
                AddCommands();
            } catch (Exception ex) { Debug.Log("failed to introduce commands due to: \n" + ex.ToString()); }
        }

        private static void AddCommands()
        {
            new CommandBuilder("GWStart").Run((args) => {
                LW = !LW;
                WriteLine("Start in GW: " + LW);
            }).Register();

            new CommandBuilder("infLighter").Run((args) => {
                infBoom = !infBoom;
                WriteLine("infinite explosions: " + infBoom);
            }).Register();
        }
    }
}
