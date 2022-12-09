using Fisobs.Core;
using Fisobs.Sandbox;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheVolatile
{
    public class Lighter : Rock
    {
        public int rollRep = 0;
        public int grabHoldLength = 0;
        bool open = false;
        public bool lit = false;
        public int timeSinceUsed = 0;

        bool bladeOwned = false;
        public Player player;
        //const int resetDelay = 20;
        public static List<Lighter> allLighters = new List<Lighter>();


        public Lighter(AbstractPhysicalObject abstractPhysicalObject, World world, Player player) : base(abstractPhysicalObject, world)
        {
            this.player = player;
            allLighters.Add(this);

            soundLoop.sound = SoundID.Fire_Spear_Ignite;
            soundLoop.Start();
#warning sound loop does nothing

            this.flicker = new float[2, 3];
            for (int i = 0; i < this.flicker.GetLength(0); i++) {
                this.flicker[i, 0] = 1f;
                this.flicker[i, 1] = 1f;
                this.flicker[i, 2] = 1f;
            }
        }

        public static Lighter getMine(Player p)
        {
            foreach(Lighter lighter in allLighters) {
                if(lighter.player == p) {
                    return lighter;
                }
            }
            return null;
        } // finds the lighter of a given player
        

        bool firstTickOfExisting = true;
        public override void Update(bool eu)
        {
            if (firstTickOfExisting) {

                player.Grab(this, 1, 0, Creature.Grasp.Shareability.CanNotShare, 1, true, false); // player grabs the lighter when it is spawned in, and blade ownership is decided
                if (player.room.game.IsArenaSession || (player.room.game.session is StoryGameSession sgs && sgs.saveState.deathPersistentSaveData.theMark)) {
                    bladeOwned = true;
                }

                firstTickOfExisting = false;
            }

            base.Update(eu);

            if (player.input[0].pckp && ((player.grasps[0]?.grabbed != null && player.grasps[0]?.grabbed == this) || (player.grasps[0]?.grabbed == null && player.grasps[1]?.grabbed != null && player.grasps[1]?.grabbed == this))) {
                grabHoldLength++;
            } else {
                grabHoldLength = 0;
            }  // identifies how long the player has held grab for

            if (grabHoldLength >= 7) {
                if (grabHoldLength == 7) {
                    room.PlaySound(SoundID.Snail_Warning_Click, firstChunk.pos, 0.8f, 2.5f);
                }
                open = true;
            } else {
                if (open && bladeOwned) {
                    for (int i = 0; i < 7; i++)
                        room.AddObject(new Spark(firstChunk.pos, UnityEngine.Random.insideUnitCircle * 3 + rotation * 7 + firstChunk.vel, Color.white, null, 5, 18));
                    room.PlaySound(SoundID.Bullet_Drip_Strike, firstChunk.pos, 1.5f, 1f);
                }

                open = false;
            }  // manages the open and closed state of the lighter, as well as vfx for it

            if (open && player.FoodInStomach > 0 && timeSinceUsed >= 10) {
                lit = true;
            } else {
                timeSinceUsed++;
                lit = false;
            }  // manages the lit and unlit state of the lighter





            if (Plugin.r.Next(7) <= (int)semiCost && lit) {
                room.AddObject(new HolyFire.HolyFireSprite(firstChunk.pos + new Vector2(0, 4)));
            }  // randomly adds holyfire particles to lighter, when appropriate

            #region LightBullshit
            for (int i = 0; i < this.flicker.GetLength(0); i++) {
                this.flicker[i, 1] = this.flicker[i, 0];
                this.flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                this.flicker[i, 0] = Custom.LerpAndTick(this.flicker[i, 0], this.flicker[i, 2], 0.05f, 0.053333335f);
                if (UnityEngine.Random.value < 0.2f) {
                    this.flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                }
                this.flicker[i, 2] = Mathf.Lerp(this.flicker[i, 2], 1f, 0.01f);
            }
            if (this.lightSource == null) {
                this.lightSource = new LightSource(base.firstChunk.pos, false, SlugbaseVolatile.instance.volatileColor(player, SlugbaseVolatile.LorO.Outline), this);
                this.lightSource.affectedByPaletteDarkness = 0.5f;
                this.room.AddObject(this.lightSource);
            } else {
                this.lightSource.setPos = new Vector2?(base.firstChunk.pos);
                this.lightSource.setRad = new float?((70f + ((int)semiCost + 2) * 20f) * this.flicker[0, 0]);
                this.lightSource.setAlpha = lit ? new float?(1f) : 0;
                if (this.lightSource.slatedForDeletetion || this.lightSource.room != this.room) {
                    this.lightSource = null;
                }
            }
            #endregion
        }

        enumSemicost semiCost = Lighter.enumSemicost.no;
        enum enumSemicost
        {
            no = 7,
            notYet = 3,
            NOW = 1
        }
        public bool semiCostIncrement()
        {
            if (semiCost == enumSemicost.no) {
                semiCost = enumSemicost.notYet;

            } else if (semiCost == enumSemicost.notYet) { 
                semiCost = enumSemicost.NOW;

            } else { 
                semiCost = enumSemicost.no;

                return true;
            }
            return false;
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            timeSinceUsed = 0;
            if (!lit)
                base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc * 1.3f, eu);
            else {
                if(!SlimeConsole.infBoom)
                if (player.superLaunchJump < 20)
                    splode();
                else bigSplode();
            }
        }

        private void bigSplode()
        {
            Vector2 pos = Vector2.Lerp(player.bodyChunks[0].pos, player.bodyChunks[1].pos, 0.5f);

            float X = -player.rollDirection;
            
            pos -= new Vector2(X * .5f, -.05f) * 10f;

            room.AddObject(new Explosion(room, null, pos, 2, 60, 12, 0, 20, 0, player, 0, 0, 0));
            room.AddObject(new ExplosionSpikes(room, pos, 12, 0.75f, 2, 7, 23, SlugbaseVolatile.instance.volatileColor(player, 0)));
            room.AddObject(new Explosion.ExplosionLight(pos, 340f, 0.8f, 9, SlugbaseVolatile.instance.volatileColor(player, 0)));
            room.AddObject(new Explosion.FlashingSmoke(pos, new Vector2(0, 1), 1.3f, SlugbaseVolatile.instance.volatileColor(player, 0), SlugbaseVolatile.instance.SlugcatColor(player.playerState.playerNumber, Color.white) ?? Color.white, UnityEngine.Random.Range(3, 11)));
            room.AddObject(new SootMark(room, pos, 80, true));


            if (!SlimeConsole.infBoom)
                reduceFood();

            room.PlaySound(SoundID.Bomb_Explode, pos, 0.85f, 1.05f);
            room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, pos, 4.5f, 0.7f);

            room.InGameNoise(new InGameNoise(firstChunk.pos, 12000f, player, 1f));
        }

        private void splode()
        {
            Vector2 pos = Vector2.Lerp(player.bodyChunks[0].pos, player.bodyChunks[1].pos, 0.5f);

            float X = player.input[0].x;
            float Y = player.input[0].y;

            if (gravity != 0 || firstChunk.submersion > 0.9f) pos -= new Vector2(X * 0.3f, (Y >= 0) ? 1.7f : -1.7f) * 10f;
            else
                pos -= new Vector2(X, Y) * 10f;

            room.AddObject(new Explosion(room, null, pos, 2, 40, 5, 0, 0, 0, player, 0, 0, 0));
            room.AddObject(new ExplosionSpikes(room, pos, 10, 0.5f, 2, 5, 15, SlugbaseVolatile.instance.volatileColor(player, 0)));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 0.7f, 7, SlugbaseVolatile.instance.volatileColor(player, 0)));
            room.AddObject(new Explosion.FlashingSmoke(pos, new Vector2(0, 1), 1, SlugbaseVolatile.instance.volatileColor(player, 0), SlugbaseVolatile.instance.SlugcatColor(player.playerState.playerNumber, Color.white) ?? Color.white, UnityEngine.Random.Range(3, 11)));
            room.AddObject(new SootMark(room, pos, 50, false));

            if (semiCostIncrement()) {
                if (!SlimeConsole.infBoom)
                    reduceFood();
                room.PlaySound(SoundID.Bomb_Explode, pos, 0.5f, 1.15f);
            } else
                room.PlaySound(SoundID.Bomb_Explode, pos, 0.4f, 1.2f);
            room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, pos, 3.5f, 0.8f);

            room.InGameNoise(new InGameNoise(firstChunk.pos, 6000f, player, .8f));
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null) {
                return false;
            }
            this.vibrate = 20;
            this.ChangeMode(Weapon.Mode.Free);
            if (result.obj is Creature) {
                (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, bladeOwned ? Creature.DamageType.Stab : Creature.DamageType.Blunt, bladeOwned ? 0.4f : 0.01f, bladeOwned ? 75f : 45f);
            } else if (result.chunk != null) result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass; else if (result.onAppendagePos != null) (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
            
            base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
            
            this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
            if (bladeOwned) this.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk, false, 1.2f, .8f);

            if (result.chunk != null) this.room.AddObject(new ExplosionSpikes(this.room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            
            this.SetRandomSpin();
            return true;
        }

        void reduceFood()
        {
            if (!SlimeConsole.infBoom) { 
            player.AddFood(-1);
            foreach (var cam in player.room.game.cameras)
                if (cam.hud.owner == player && cam.hud.foodMeter is HUD.FoodMeter fm && fm.showCount > 0)
                    fm.circles[--fm.showCount].EatFade();
            }
        }

        float[,] flicker;
        LightSource lightSource;
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 a = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            if (this.vibrate > 0) {
                a += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
            }
            sLeaser.sprites[0].x = a.x - camPos.x;
            sLeaser.sprites[0].y = a.y - camPos.y;
            sLeaser.sprites[0].element = open ? new FSprite("lighterOpen").element : bladeOwned ? new FSprite("lighterBlade").element : new FSprite("lighterClosed").element;
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), Vector3.Slerp(this.lastRotation, this.rotation, timeStacker));
            sLeaser.sprites[0].scaleX = (player.grasps[0]?.grabbed == null && player.grasps[1]?.grabbed != null && player.grasps[1]?.grabbed == this) ? -1f : 1f;

            sLeaser.sprites[2].isVisible = !(player.grasps[0]?.grabbed is Lighter || player.grasps[1]?.grabbed is Lighter);

            RoomCamera.SpriteLeaser playerLeaser = null;
            foreach (RoomCamera.SpriteLeaser potentialPlayerLeaser in rCam.spriteLeasers) {

                if(potentialPlayerLeaser.drawableObject is PlayerGraphics p && p.owner == player) {
                    playerLeaser = potentialPlayerLeaser;
                }
            }


            if (playerLeaser != null) {
                Vector2 A = playerLeaser.sprites[0].GetPosition();
                Vector2 B = sLeaser.sprites[0].GetPosition();

                Vector2 perpTheta = new Vector2((A - B).y, -(A - B).x).normalized;
                float bonus = Mathf.Lerp(1f, .3f, Mathf.InverseLerp(1, 7, Vector2.Distance(A, B) / 20));

                for (int i = 0; i <= 7; i++) {
                    Vector2 haver = Vector2.Lerp(A, B, i/7f);

                    /*top*/
                    mesh.MoveVertice(i * 2, (haver + sagPer(i) * bonus * new Vector2(0,-20)) + (perpTheta * 10 * plumpPer(i) * bonus));
                    /*bot*/
                    mesh.MoveVertice(i * 2 + 1, (haver + sagPer(i) * bonus * new Vector2(0, -20)) - (perpTheta * 10 * plumpPer(i) * bonus));
                }
                mesh.MoveBehindOtherNode(playerLeaser.sprites[9]);

                float dir = A.x > B.x ? -1 : 1;
                mesh.MoveVertice(16, B + bonus * new Vector2(5 * dir, 4 * dir * (1.5f - bonus)));
                mesh.MoveVertice(17, B + bonus * new Vector2(5 * dir, -6 * dir * (1.5f - bonus)));
                mesh.MoveVertice(18, B + bonus * new Vector2(10 * dir, 0));
            }

            if(grabbedBy.Count == 0) {
                ChangeOverlap(false);
            } else
                ChangeOverlap(true);

            if (slatedForDeletetion || room != rCam.room) {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        float sagPer(int i)
        {
            switch (i) {
                case 0:
                case 7: return 0;

                case 1:
                case 6: return .4f;

                case 2:
                case 5: return .6f;

                case 3:
                case 4: return .7f;

                default: return 0;
            }
        }
        float plumpPer(int i)
        {
            switch (i) {
                case 0:
                case 7: return 1;

                case 1:
                case 6: return .9f;

                case 2:
                case 5: return .8f;

                case 3:
                case 4: return .75f;

                default: return 1;
            }
        }
        //these both help dictate the sizes of the floobert's segments

        TriangleMesh mesh;

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("lighterClosed", true);

            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
            {
            new TriangleMesh.Triangle(0, 1, 2)
            };
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false);
            sLeaser.sprites[1] = triangleMesh;

            tris = new TriangleMesh.Triangle[]
            {
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(1, 2, 3),
            new TriangleMesh.Triangle(2, 3, 4),
            new TriangleMesh.Triangle(3, 4, 5),
            new TriangleMesh.Triangle(4, 5, 6),
            new TriangleMesh.Triangle(5, 6, 7),
            new TriangleMesh.Triangle(6, 7, 8),
            new TriangleMesh.Triangle(7, 8, 9),
            new TriangleMesh.Triangle(8, 9, 10),
            new TriangleMesh.Triangle(9, 10,11),
            new TriangleMesh.Triangle(10,11,12),
            new TriangleMesh.Triangle(11,12,13),
            new TriangleMesh.Triangle(12,13,14),
            new TriangleMesh.Triangle(13,14,15),
            new TriangleMesh.Triangle(14,15,16),
            new TriangleMesh.Triangle(15,16,17),
            new TriangleMesh.Triangle(16,17,18)
            };
            mesh = new TriangleMesh("floobert" + SlugbaseVolatile.instance.idstring(player.playerState.playerNumber), tris, true);
            for (int j = mesh.vertices.Length - 1; j >= 0; j--) {
                float num = (float)(j / 2) / (float)(mesh.vertices.Length / 2);
                Vector2 vector;
                if (j % 2 == 0) {
                    vector = new Vector2(num, 0f);
                } else if (j < mesh.vertices.Length - 1) {
                    vector = new Vector2(num, 1f);
                } else {
                    vector = new Vector2(1f, 0f);
                }
                vector.x = Mathf.Lerp(mesh.element.uvBottomLeft.x, mesh.element.uvTopRight.x, vector.x);
                vector.y = Mathf.Lerp(mesh.element.uvBottomLeft.y, mesh.element.uvTopRight.y, vector.y);
                mesh.UVvertices[j] = vector;
            }
            sLeaser.sprites[2] = mesh;

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null) {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            for (int i = sLeaser.sprites.Length - 1; i >= 0; i--) {
                sLeaser.sprites[i].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = SlugbaseVolatile.instance.volatileColor(player, SlugbaseVolatile.LorO.Lighter);
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1].color = color;
            sLeaser.sprites[2].color = Color.white;
        }
    }

    public class AbstractLighter : AbstractPhysicalObject
    {
        public Player player;
        public AbstractLighter(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Player player) : base(world, FisLighter.Instance.Type, realizedObject, pos, ID)
        {
            this.player = player;
            this.world = world;
            destroyOnAbstraction = true;
        }

        public override string ToString()
        {
            return this.SaveToString();
        }

        public override void Realize()
        {
            base.Realize();

            if (realizedObject == null)
                realizedObject = new Lighter(this, this.world, player);
        }
    }

    public class FisLighter : Fisobs.Items.Fisob
    {
        public FisLighter() : base(EnumExt_Volatile.Lighter)
        {

        }

        public static readonly FisLighter Instance = new FisLighter();

        public override void LoadResources(RainWorld rainWorld)
        {
            //put your atlas loading bullshit here
        }

        public override AbstractPhysicalObject Parse(World world, Fisobs.Core.EntitySaveData entitySaveData, SandboxUnlock? unlock)
        {
            Debug.Log("tell ethanol that the game is parsing a lighter for some fucking reason");
            throw new NotImplementedException();
        }
    }
}
