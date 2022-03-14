using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlugBase;
using UnityEngine;

namespace TheVolatile
{
    public class SlugbaseVolatile : SlugBaseCharacter
    {
        public SlugbaseVolatile() : base("The Volatile", FormatVersion.V1, 0, true)
        {
            On.Player.Jump += Player_Jump;
        }

        public override string Description => "farded";

        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (IsMe(self)) 
            {
                AbstractLighter abstractLighter = new AbstractLighter(self.room.world, null, self.abstractCreature.pos, self.room.world.game.GetNewID());
                abstractLighter.RealizeInRoom();
            }
        }
    }
}
