using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile
{
    public class Lighter : Rock
    {
        public Player player;
        int timeSinceHold = 0;
        bool open = false;
        bool lit = false;
        static List<Lighter> allLighters = new List<Lighter>();

        public Lighter(AbstractPhysicalObject abstractPhysicalObject, World world, Player player) : base(abstractPhysicalObject, world)
        {
            this.player = player;
            allLighters.Add(this);

            CustomAtlases.FetchAtlas("lighterClosed");
            CustomAtlases.FetchAtlas("lighterOpen");
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

            if (player.input[0].pckp) {
                timeSinceHold++;
                if (timeSinceHold > 6) {
                    open = true;
                }
                if (timeSinceHold == 12) {
                    lit = true;
                }
            } else {
                timeSinceHold = 0;
                open = false;
                lit = false;
            }
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

            RoomCamera.SpriteLeaser playerLeaser = null;
            foreach (RoomCamera.SpriteLeaser potentialPlayerLeaser in rCam.spriteLeasers) {
                Debug.Log("hmm yes i found a " + potentialPlayerLeaser.drawableObject.GetType().ToString() + "'s sleaser");

                if(potentialPlayerLeaser.drawableObject is PlayerGraphics p && p.owner == player) {
                    Debug.Log("foind slime :)");
                    playerLeaser = potentialPlayerLeaser;
                }
            }

            Vector2 playerPos = playerLeaser.sprites[0].GetPosition();
            Vector2 lighterPos = sLeaser.sprites[0].GetPosition();

            if (playerLeaser != null) {
                Debug.Log("trying to reposition");
                int s = 0;
                foreach (Vector2 chunk in new Vector2[] { playerPos, lighterPos }) {

                    mesh.MoveVertice(0 + s * 4, chunk + new Vector2(20, 20));
                    mesh.MoveVertice(1 + s * 4, chunk + new Vector2(-20, 20));
                    mesh.MoveVertice(2 + s * 4, chunk + new Vector2(20, -20));
                    mesh.MoveVertice(3 + s * 4, chunk + new Vector2(-20, -20));

                    s++;
                }
            }

            if (slatedForDeletetion || room != rCam.room) {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        TriangleMesh mesh;
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            color = Color.green * Color.gray;

            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("lighterClosed", true);

            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
            {
            new TriangleMesh.Triangle(0, 1, 2)
            };
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
            sLeaser.sprites[1] = triangleMesh;

            mesh = TriangleMesh.MakeLongMesh(2, false, false);
            sLeaser.sprites[2] = mesh;
            mesh.color = Color.green;

            AddToContainer(sLeaser, rCam, null);
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
