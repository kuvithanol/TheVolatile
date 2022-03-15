using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile
{
    public class Lighter : Rock
    {
        public Player owner;
        bool firstTick = true;
        public Lighter(AbstractPhysicalObject abstractPhysicalObject, World world, Player player) : base(abstractPhysicalObject, world)
        {
            this.owner = player;
        }

        public override void Update(bool eu)
        {
            if (firstTick) {
                owner.Grab(this, 1, 0, Creature.Grasp.Shareability.CanNotShare, 1, true, false);
            }
            base.Update(eu);
            firstTick = false;

            firstChunk.vel *= 0.8f;
            firstChunk.pos = Vector2.Lerp(firstChunk.pos, owner.firstChunk.pos, 0.05f);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            foreach(FSprite s in sLeaser.sprites) 
            {
                s.color = Color.green * Color.gray;
            }

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
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
