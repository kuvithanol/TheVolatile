using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile
{
    public class Lighter : Rock
    {
        public Lighter(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {

        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            foreach(FSprite s in sLeaser.sprites) 
            {
                s.color = Color.white;
            }

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }

    public class AbstractLighter : AbstractPhysicalObject
    {
        public AbstractLighter(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, FisLighter.Instance.Type, realizedObject, pos, ID)
        {
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
                realizedObject = new Lighter(this, this.world);
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
