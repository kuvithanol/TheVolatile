using SlugBase;
using System;
using System.Linq;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.IO;

namespace TheVolatile
{
    public class SlugbaseVolatile : SlugBaseCharacter
    {
        public static SlugbaseVolatile instance;

        

        public SlugbaseVolatile() : base("The Volatile", FormatVersion.V1, 0, true)
        {
            instance = this;
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
            On.Creature.Violence += Creature_Violence;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.Player.ThrowObject += Player_ThrowObject;
            IL.Player.ThrowObject += IL_Player_ThrowObject;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.Player.Grabability += Player_Grabability;
            On.Player.SlugcatGrab += Player_SlugcatGrab;
        }

        private void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            orig(self, obj, graspUsed);
            if (IsMe(self) && self.FoodInStomach != self.MaxFoodInStomach) {
                if (obj is IPlayerEdible icr && !(obj is KarmaFlower) && !(obj is Mushroom) && (!(obj is Creature) || (obj is Creature cr && (cr.dead || cr is Fly || cr is SmallNeedleWorm)))) {
                    obj.slatedForDeletetion = true;
                    self.AddFood(icr.FoodPoints);
                    self.room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, obj.firstChunk.pos, 1f, 1.2f);
                }
                Lighter.getMine(self).semicost = false;
            }
        }

        private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is Lighter l && Lighter.getMine(self) != l) {
                return 0;
            }
            return orig(self, obj);
        }

        private bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            if(testObj is Lighter) {
                return false;
            }
            return orig(self, testObj);
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            orig(self, grasp, eu);

            if (!primedLighter(self, self.grasps[grasp].grabbed)) {
                self.ReleaseGrasp(grasp);
            }
        }

        bool primedLighter(Player p, PhysicalObject l)
        {
            if (IsMe(p) && l is Lighter lig && lig.lit) {
                lig.lit = false;
                return true;
            }
            return false;
        }

        private void IL_Player_ThrowObject(MonoMod.Cil.ILContext il)
        {
            //Debug.Log("ilhook START");
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

                self.bounce = 0.4f;

                AbstractLighter abstractLighter = new AbstractLighter(self.room.world, null, self.abstractCreature.pos, self.room.world.game.GetNewID(), self);
                abstractLighter.RealizeInRoom();

                self.abstractCreature.stuckObjects.Add(new LighterStick(abstractCreature, abstractLighter));
            }
        }
        private class LighterStick : AbstractPhysicalObject.AbstractObjectStick
        {
            public LighterStick(AbstractPhysicalObject A, AbstractPhysicalObject B) : base(A, B)
            {

            }

            public override string SaveToString(int roomIndex)
            {
                return string.Concat(new string[]
                {
                roomIndex.ToString(),
                "<stkA>gripGoob<stkA>",
                this.A.ID.ToString(),
                "<stkA>",
                this.B.ID.ToString()
                });
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

                    newSprite.color = volatileColor(self.player)[1];
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



                CustomAtlases.FetchAtlas("OutlineTail");
                
                TriangleMesh tailMesh = new TriangleMesh("OutlineTail", (sLeaser.sprites[2] as TriangleMesh).triangles, true) {
                    color = instance.volatileColor(self.player)[0],
                    element = new FSprite("OutlineTail").element
                };
                for (int j = tailMesh.vertices.Length - 1; j >= 0; j--) {
                    float num = (float)(j / 2) / (float)(tailMesh.vertices.Length / 2);
                    Vector2 vector;
                    if (j % 2 == 0) {                                                                
                        vector = new Vector2(num, 0f);
                    } else if (j < tailMesh.vertices.Length - 1) {
                        vector = new Vector2(num, 1f);
                    } else {
                        vector = new Vector2(1f, 0f);
                    }
                    vector.x = Mathf.Lerp(tailMesh.element.uvBottomLeft.x, tailMesh.element.uvTopRight.x, vector.x);
                    vector.y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y, tailMesh.element.uvTopRight.y, vector.y);
                    tailMesh.UVvertices[j] = vector;
                }

                self.tail[0] = new TailSegment(self, 8f, 6f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 8f, 10f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 5.5f, 10f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 2f, 10f, self.tail[2], 0.85f, 1f, 0.5f, true);

                sLeaser.AddSpritesToContainer(fContainer, rCam);
            }
        }


        public override string Description => "this cat is s";

        public Color[] volatileColor(Player p)
        {
            Color[] ret = new Color[2];
            ret[0] = (Color)SlugcatColor(p.playerState.playerNumber, Color.white);
            ret[1] = (Color)SlugcatEyeColor(p.playerState.playerNumber);
            return ret;
        }

        public override Color? SlugcatColor(int slugcatCharacter, Color baseColor)
        {
            switch (slugcatCharacter) {
                case 0: return Color.green;
                case 1: return new Color(1, 0.5f, 0);
                case 2: return Color.cyan;
                case 3: return Color.Lerp(Color.magenta, Color.gray, 0.4f);
                default: return Color.green;
            }
        }

        public override Color? SlugcatEyeColor(int slugcatCharacter)
        {
            switch (slugcatCharacter) {
                case 0: return Color.Lerp(Color.green, Color.black, 0.5f);
                case 1: return Color.yellow;
                case 2: return Color.blue;
                case 3: return Color.white;
                default: return Color.green;
            }
        }

        public override float? GetCycleLength()
        {
            return Mathf.Lerp(6, 8, UnityEngine.Random.value);
        }

        protected override void GetStats(SlugcatStats stats)
        {
            stats.throwingSkill = 0;
            stats.lungsFac = 0.1f;
            stats.runspeedFac *= 0.9f;
        }

        public override void GetFoodMeter(out int maxFood, out int foodToSleep)
        {
            maxFood = 6;
            foodToSleep = 3;
        }

        public override Stream GetResource(params string[] path)
        {
            var patchedPath = new string[path.Length];
            for (int i = path.Length - 1; i > -1; i--) patchedPath[i] = path[i];
            //this is needed because there are two scenes that have most assets in common, you probably won't have to use it

            if (path[path.Length - 2] == "SelectMenuDisrupt" && path.Last() != "scene.json")
                patchedPath[path.Length - 2] = "SelectMenu";
            //join the path parts from new array to get resource name to request
            string oresname = "TheVolatile.graphics." + string.Join(".", patchedPath);
            //attempt getting the resource. If name is wrong, null is returned
            var tryret = Assembly.GetExecutingAssembly().GetManifestResourceStream(oresname);
            if (tryret != null) Console.WriteLine($"BUILDING SCENE FROM ER: {oresname}");
            //if tryret is null, it means my name conversion was wrong or that i just didn't have the requested thing, let slugbase deal with it
            return tryret ?? base.GetResource(path);
        }
    }
}
