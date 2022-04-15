using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile.PlacedObs
{
    public class MountainShrine : CosmeticSprite
    {
        public MountainShrine(PlacedObject pObj)
        {
            this.pObj = pObj;
            //SoundHelper.GetCustomSound("ShrineActivate");
            hit = false;
        }

        public override void Update(bool eu)
        {
            if (room == null) return;
            base.Update(eu);

            pos = room.MiddleOfTile(pObj.pos) + offset + new Vector2(0, -35);
            interactionRange = false;
            if (!hit) {
                if (pObj != null) {
                    if (room != null && room.world != null && room.world.game != null) {
                        for (int i = 0; i < room.world.game.Players.Count; i++) {
                            if (room.world.game.Players[i] != null && !room.world.game.Players[i].slatedForDeletion && room.world.game.Players[i].realizedCreature != null) {
                                Player player = room.world.game.Players[i].realizedCreature as Player;
                                for (int g = 0; g < player.grasps.Length; g++) {
                                    interactionRange = Vector2.Distance(player.firstChunk.pos, this.pos) < 40;

                                    if (interactionRange && player.input[0].thrw) {
                                        Hit(/*sword.firstChunk*/);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Hit(/*BodyChunk chunk*/)
        {
            hit = true;

            //SoundHelper.PlayCustomSound("ShrineActivate", chunk, 0.45f, 1f);

            List<IntVector2> localTiles = new List<IntVector2>();
            for (int i = 20; i > -20; i--) {
                int xOffset = 10 * i;
                localTiles = SharedPhysics.RayTracedTilesArray(pos + new Vector2(xOffset, 200f), pos + new Vector2(xOffset, -200f));

                for (int t = 0; t < localTiles.Count; t++) {
                    if (room.GetTile(localTiles[t]).Terrain == Room.Tile.TerrainType.Air) {
                        tiles.Add(localTiles[t]);
                    }
                }
                localTiles.Clear();
            }

            for (int i = 0; i < Random.Range(3, 6); i++) {
                AbstractPhysicalObject absPhysOb = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, room.GetWorldCoordinate(tiles[Random.Range(0, tiles.Count)].ToVector2() * 20f), room.game.GetNewID());
                room.abstractRoom.AddEntity(absPhysOb);
                absPhysOb.RealizeInRoom();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];
            sLeaser.sprites[3] = new FSprite("mountainA", true) {
                shader = rCam.game.rainWorld.Shaders["ColoredSprite2"],
                sortZ = 0
            };
            sLeaser.sprites[2] = new FSprite("mountainB", true) {
                shader = rCam.game.rainWorld.Shaders["ColoredSprite2"],
                sortZ = 0
            };
            sLeaser.sprites[1] = new FSprite("mountainM", true) { alpha = .8f };
            sLeaser.sprites[0] = new FSprite("mountainO", true) { alpha = .6f };

            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);

            sLeaser.sprites[0].SetPosition(vector - camPos);
            sLeaser.sprites[1].SetPosition(vector - camPos);
            sLeaser.sprites[2].SetPosition(vector - camPos);
            sLeaser.sprites[3].SetPosition(vector - camPos);

            sLeaser.sprites[0].isVisible = !hit && interactionRange;
            sLeaser.sprites[1].isVisible = !hit;
            sLeaser.sprites[2].alpha = .9f;
            sLeaser.sprites[3].alpha = .8f;
        }

        readonly Vector2 offset = new Vector2(0f, 35f);
        public PlacedObject pObj;
        public Creature hitter;
        public bool interactionRange;
        public bool hit;
        public List<IntVector2> tiles = new List<IntVector2>();
    }
}
