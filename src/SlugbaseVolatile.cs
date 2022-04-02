using SlugBase;
using System;
using System.Linq;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TheVolatile
{
    public class SlugbaseVolatile : SlugBaseCharacter
    {
        static System.Random r = new System.Random();
        public SlugbaseVolatile() : base("The Volatile", FormatVersion.V1, 0, true) 
        {
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
            On.Creature.Violence += Creature_Violence;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.Player.ThrowObject += Player_ThrowObject;
            IL.Player.ThrowObject += IL_Player_ThrowObject;

        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            orig(self, grasp, eu);

            if (notPrimedLighter(self, self.grasps[grasp].grabbed)) {
                self.ReleaseGrasp(grasp);
            }
        }

        bool notPrimedLighter(Player p, PhysicalObject l)
        {
            if (IsMe(p) && l is Lighter lig && lig.lit) {

                p.AddFood(-1);
                foreach (var cam in p.room.game.cameras)
                    if (cam.hud.owner == p && cam.hud.foodMeter is HUD.FoodMeter fm && fm.showCount > 0)
                        fm.circles[--fm.showCount].EatFade();


                return false;
            }
            return true;
        }

        private void IL_Player_ThrowObject(MonoMod.Cil.ILContext il)
        {
            Debug.Log("ilhook START");
            ILCursor baba = new ILCursor(il);
            ILCursor keke = new ILCursor(il);


            //Plugin.logger.Log(BepInEx.Logging.LogLevel.Warning, "First IL:\n" + il);
            //Debug.Log(baba);

            baba.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1),
                x => x.MatchCallOrCallvirt<Player>("ReleaseGrasp")); //baba marks the target IL chunk
            //Debug.Log(baba);

            keke.GotoNext(
                x => x.MatchRet());
            ILLabel oldret = keke.DefineLabel();
            oldret = keke.MarkLabel();
            //Debug.Log(keke);

            baba.RemoveRange(3); //baba removes the target IL chunk
            //Plugin.logger.Log(BepInEx.Logging.LogLevel.Warning, "Remove3 IL:\n" + il);

            ILLabel newret = baba.DefineLabel();
            newret = baba.MarkLabel();
            //Debug.Log(baba);

#warning make this not an out when you get the chance!
            baba.GotoPrev(
                x => x.MatchBrfalse(out oldret)); 
            //Debug.Log(baba);

            baba.Remove();
            //Plugin.logger.Log(BepInEx.Logging.LogLevel.Warning, "CullBR IL:\n" + il);

            baba.Emit(OpCodes.Brfalse, newret);
            //Plugin.logger.Log(BepInEx.Logging.LogLevel.Warning, "ReplaceBR IL:\n" + il);
        }

        private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if(self is Player p && IsMe(p) && type == Creature.DamageType.Explosion) {
                orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, 0, 0);
            } else orig(self,source,directionAndMomentum,hitChunk,hitAppendage,type,damage,stunBonus);
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

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (IsMe(self)) {
                Lighter l = Lighter.getMine(self);
                if (l != null && (!(self.grasps[0]?.grabbed == l || self.grasps[1]?.grabbed == l))) {
                    Vector2 playerLoc = self.firstChunk.pos;
                    Vector2 lighterLoc = l.firstChunk.pos;
                    float ropeLength = 40f;
                    float elasticity = 0.2f;
                    float dist = Vector2.Distance(self.bodyChunks[1].pos, l.firstChunk.pos);

                    if (dist < 20) {
                        elasticity = 0;
                    }
                    if (dist > ropeLength) {
                        dist -= ropeLength;
                        elasticity *= 1 + dist * 0.5f;
                    }
                    float ratio = self.bodyChunks[1].mass / (l.firstChunk.mass + self.bodyChunks[1].mass);
                    Vector2 motionDir = Custom.DirVec(playerLoc, lighterLoc);
                    self.firstChunk.pos += motionDir * (1f - ratio) * elasticity * 0.2f;
                    self.firstChunk.vel += motionDir * (1f - ratio) * elasticity * 0.2f;
                    motionDir = Custom.DirVec(lighterLoc, playerLoc);
                    l.firstChunk.pos += motionDir * ratio * elasticity;
                    l.firstChunk.vel += motionDir * ratio * elasticity;
                }
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
