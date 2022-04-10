﻿using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheVolatile
{
    public class Lighter : Rock
    {
        public Player player;
        int timeSinceClick = 0;
        bool open = false;
        public bool lit = false;
        bool iveBeenOpen = false;
        int delay = 0;
        const int resetDelay = 20;
        public static List<Lighter> allLighters = new List<Lighter>();

        public Lighter(AbstractPhysicalObject abstractPhysicalObject, World world, Player player) : base(abstractPhysicalObject, world)
        {
            this.player = player;
            allLighters.Add(this);

            soundLoop.sound = SoundID.Fire_Spear_Ignite;
            soundLoop.Start();
        }

        public static Lighter getMine(Player p)
        {
            foreach(Lighter lighter in allLighters) {
                if(lighter.player == p) {
                    return lighter;
                }
            }
            return null;
        }
        

        bool firstTickOfExisting = true;
        public override void Update(bool eu)
        {
            if (firstTickOfExisting) {
                player.Grab(this, 1, 0, Creature.Grasp.Shareability.CanNotShare, 1, true, false);
            }
            base.Update(eu);
            firstTickOfExisting = false;

            if (player.input[0].pckp && ((player.grasps[0]?.grabbed != null && player.grasps[0]?.grabbed == this) || (player.grasps[0]?.grabbed == null && player.grasps[1]?.grabbed != null && player.grasps[1]?.grabbed == this))) {
                timeSinceClick++;
                if (timeSinceClick > 8) {
                    open = true;
                }
                if (open && delay == 0 && (player.FoodInStomach > 0)) {
                    lit = true;
                    soundLoop.Volume = 1;
                }
            } else {
                timeSinceClick = 0;
                open = false;
                lit = false;
                soundLoop.Volume = 0;
            }

            if(Plugin.r.Next(3) == 1 && lit) {
                room.AddObject(new HolyFire.HolyFireSprite(firstChunk.pos + new Vector2(0, 4)));
            }

            if (delay != 0) delay--;

            if(!iveBeenOpen && open) {
                room.PlaySound(SoundID.Snail_Warning_Click, firstChunk.pos, 0.8f, 2f);
            }


            iveBeenOpen = open;
        }

        public bool semicost = false;
        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            if (!lit)
                base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            else {
                delay = resetDelay;
                if (semicost) {
                    reduceFood();
                    semicost = false;
                }else semicost = true;

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

                room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, pos, 3.5f, 0.8f);
                room.PlaySound(SoundID.Bomb_Explode, pos, 0.4f, 1.2f);
            }
        }

        void reduceFood()
        {
            player.AddFood(-1);
            foreach (var cam in player.room.game.cameras)
                if (cam.hud.owner == player && cam.hud.foodMeter is HUD.FoodMeter fm && fm.showCount > 0)
                    fm.circles[--fm.showCount].EatFade();
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 a = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            if (this.vibrate > 0) {
                a += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
            }
            sLeaser.sprites[0].x = a.x - camPos.x;
            sLeaser.sprites[0].y = a.y - camPos.y;
            sLeaser.sprites[0].element = open ? new FSprite("lighterOpen").element : new FSprite("lighterClosed").element;
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
            if (/*newContatiner == null*/ true) {
                newContatiner = rCam.ReturnFContainer("Background");
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

    public class FisLighter : Fisob
    {
        public FisLighter() : base("Lighter")
        {

        }

        public static readonly FisLighter Instance = new FisLighter();

        public override void LoadResources(RainWorld rainWorld)
        {
            //put your atlas loading bullshit here
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
        {
            Debug.Log("if you can read this, sorry !");
            throw new NotImplementedException();
        }
    }
}
