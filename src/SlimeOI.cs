using BepInEx;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile
{
    public class SlimeOI : OptionInterface
    {
        public SlimeOI(Plugin plugin) : base(plugin: plugin)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[1];
            Tabs[0] = new OpTab("Main");

            OpRadioButtonGroup group = new OpRadioButtonGroup("Skins");
            Tabs[0].AddItems(group);
            OpRadioButton[] buttons = new OpRadioButton[] {
                    new OpRadioButton(300f, 450f), new OpRadioButton(330f, 450f), new OpRadioButton(360f, 450f), new OpRadioButton(390f, 450f), new OpRadioButton(420f, 450f) };
            buttons[0].description = "Default skin for all characters";
            buttons[1].description = "Volatile skin for all characters";
            buttons[2].description = "Gup (RoR2) skin for all characters";
            buttons[3].description = "King Slime (Terraria) skin for all characters";
            buttons[4].description = "Tabby Slime (Slime Rancher) skin for all characters";

            group.SetButtons(buttons);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
            Debug.Log(config["Skins"]);

            OIVars.skinSelection = int.Parse(config["Skins"]);
        }
    }

    public class OIVars
    {
        public static int skinSelection = 0;
    }
}
