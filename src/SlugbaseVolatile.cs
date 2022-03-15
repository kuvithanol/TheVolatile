using SlugBase;
using System;
using System.Linq;
using UnityEngine;

namespace TheVolatile
{
    public class SlugbaseVolatile : SlugBaseCharacter
    {
        static System.Random r = new System.Random();
        public SlugbaseVolatile() : base("The Volatile", FormatVersion.V1, 0, true)
        {
            On.Player.ctor += Player_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (IsMe(self)) {
                self.bounce = 0.6f;

                AbstractLighter abstractLighter = new AbstractLighter(self.room.world, null, self.abstractCreature.pos, self.room.world.game.GetNewID(), self);
                abstractLighter.RealizeInRoom();
            }
        }

        private void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (IsMe(self.player)) {
                var myContainer = sLeaser.containers?.FirstOrDefault(x => x.data is string s && s == "slime");
                if(myContainer != null) {
                    rCam.ReturnFContainer("Midground").AddChild(myContainer);
                }
            }
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (IsMe(self.player)) {
                var myContainer = sLeaser.containers?.FirstOrDefault(x => x.data is string s && s == "slime");
                if (myContainer == null) return;

                int i = 0;
                foreach(FSprite vSprite in sLeaser.sprites) {
                    FSprite mSprite = (FSprite)myContainer.GetChildAt(i);
                    if (i == 2 || i == 7 || i == 8 || i == 11 || i == 10) {
                        mSprite.isVisible = false;
                    } else {



                        mSprite.SetPosition(vSprite.GetPosition());
                        mSprite.SetAnchor(vSprite.GetAnchor());
                        mSprite.rotation = vSprite.rotation;
                    }
                    if(i == 6 || i == 5) {
                        mSprite.isVisible = vSprite.isVisible; // <-----------
                    }

                    i++;
                }
            }
        }

        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (IsMe(self.player)) {
                FContainer fContainer = new FContainer {
                    data = $"slime"
                };

                int i = 0; foreach (FSprite oldSprite in sLeaser.sprites) {
                    FSprite newSprite = new FSprite(oldSprite.element);

                    newSprite.color = Color.green;
                    newSprite.SetPosition(oldSprite.GetPosition());
                    newSprite.scale = 1.5f;
                    fContainer.AddChild(newSprite);
                    i++;
                }
                if (sLeaser.containers == null)
                    sLeaser.containers = new FContainer[1];
                else
                    Array.Resize(ref sLeaser.containers, sLeaser.containers.Length + 1);
                sLeaser.containers[sLeaser.containers.Length - 1] = fContainer;

                sLeaser.AddSpritesToContainer(fContainer, rCam);
            }
        }


        public override string Description => "farded";
    }
}
